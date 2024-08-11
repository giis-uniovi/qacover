package giis.qacover.core.services;

import java.util.ArrayList;
import java.util.List;
import java.util.Properties;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import giis.portable.util.FileUtil;
import giis.portable.util.JavaCs;
import giis.portable.util.Parameters;
import giis.portable.util.PropertiesFactory;
import giis.qacover.model.Variability;
import giis.qacover.portable.Jdk14PropertiesFactory;
import giis.qacover.portable.QaCoverException;

/**
 * Global configuration options (singleton), read from properties file
 */
public class Configuration { // NOSONAR singleton allowed
	public static final String OPTIONS_FILE_PROPERTY = "qacover.properties";
	private static final String QACOVER_NAME = "qacover";
	private static final Logger log = LoggerFactory.getLogger(Configuration.class);
	private Properties properties;
	private static Configuration instance;
	private String name=QACOVER_NAME;
	private String storeRulesLocation;
	private String cacheRulesLocation;
	private String storeReportsLocation;
	private String ruleServiceType;
	private String fpcServiceUrl;
	private String fpcServiceOptions;
	private boolean optimizeRuleEvaluation;
	private boolean inferQueryParameters;
	private List<String> stackExclusions;
	private List<String> stackInclusions;
	private List<String> classExclusions; // do not generate for classes with this name (exact match)
	private List<String> tableExclusionsExact; // do not generate for queries using any of this tables (exact match)
	private String dbStoretype = null; // null means not configured, once configured will not be changed

	private Configuration() {
		reset();
	}

	public static Configuration getInstance() {
		if (instance == null)
			instance = new Configuration();
		return instance;
	}

	public Configuration reset() {
		this.properties = getProperties(OPTIONS_FILE_PROPERTY);
		String storeRootLocation = getProperty("qacover.store.root", Parameters.getProjectRoot());
		String defaultRulesSubDir = FileUtil.getPath(Parameters.getReportSubdir(), QACOVER_NAME, "rules");
		String defaultReportsSubDir = FileUtil.getPath(Parameters.getReportSubdir(), QACOVER_NAME, "reports");
		storeRulesLocation = FileUtil.getPath(storeRootLocation, getProperty("qacover.store.rules", defaultRulesSubDir));
		storeReportsLocation = FileUtil.getPath(storeRootLocation, getProperty("qacover.store.reports", defaultReportsSubDir));
		ruleServiceType = "fpc";
		fpcServiceUrl = getProperty("qacover.fpc.url", "https://in2test.lsi.uniovi.es/tdrules/api/v4");
		fpcServiceOptions = getProperty("qacover.fpc.options", "");
		optimizeRuleEvaluation = JavaCs.equalsIgnoreCase("true", getProperty("qacover.optimize.rule.evaluation", "false"));
		inferQueryParameters = JavaCs.equalsIgnoreCase("true", getProperty("qacover.query.infer.parameters", "false"));

		// Includes some predefined stack exclusions
		stackExclusions = new ArrayList<>();
		stackExclusions.add("giis.portable.");
		stackExclusions.add("giis.qacover.");
		stackExclusions.add("com.p6spy.");
		stackExclusions.add("java.");
		stackExclusions.add("javax.");
		stackExclusions.add("jdk.internal."); // for jdk11 (not needed in jdk8)
		stackExclusions.add("System."); // for .net
		for (String item : getMultiValueProperty("qacover.stack.exclusions"))
			stackExclusions.add(item);
		
		stackInclusions = getMultiValueProperty("qacover.stack.inclusions");
		classExclusions = getMultiValueProperty("qacover.class.exclusions");
		tableExclusionsExact = getMultiValueProperty("qacover.table.exclusions.exact");
		return this;
	}

	/**
	 * Loads the properties in the order:
	 * - system properties
	 * - class path
	 * - root folder
	 */
	public Properties getProperties(String propFileName) {
		Properties prop = null;
		if (new Variability().isJava()) {
			// Uses a custom method with custom implementation for java 1.4
			// (the general method raises exception when searching system.properties, GitLab #7
			log.info(propFileName + " search in system property");
			prop = new Jdk14PropertiesFactory().getPropertiesFromSystemProperty(new Variability().isJava4(),
					propFileName, propFileName);
			if (prop != null) {
				log.info(propFileName + " found in system property");
				return prop;
			}
			log.warn(propFileName + " not found in system property, trying classpath");
		}
		if (new Variability().isJava()) {
			log.info(propFileName + " search in classpath");
			prop = new PropertiesFactory().getPropertiesFromClassPath(propFileName);
			if (prop != null) {
				log.info(propFileName + " found in classpath");
				return prop;
			}
		}
		log.warn(propFileName + " not found in classpath, trying root path");
		String propFileNameWithPath = FileUtil.getPath(Parameters.getProjectRoot(), propFileName);
		log.info(propFileName + " search in root path: " + propFileNameWithPath);
		prop = new PropertiesFactory().getPropertiesFromFilename(propFileNameWithPath);
		if (prop != null) {
			log.info(propFileName + " found in root path");
			return prop;
		}
		log.error(propFileName + " not found in root path. Can't continue");
		throw new QaCoverException(propFileName + " not found in root path: "
				+ FileUtil.getFullPath(propFileNameWithPath) + " - Can't continue");
	}

	/**
	 * Reads a property with a default value.
	 * On net, makes a first read from an environment variable
	 */
	private String getProperty(String name, String defaultValue) {
		String value = null;
		// On .net docker environment variables replace configuration files
		if (new Variability().isNetCore()) {
			value = JavaCs.getEnvironmentVariable(name);
			if (value != null) {
				log.debug("Get parameter from environment: " + name + "=" + value); // NOSONAR for docker console
				return value;
			}
		}

		// Avoid exception when running in docker (no properties files)
		try {
			value = properties.getProperty(name, defaultValue);
			log.debug("Get parameter from qacover.properties: " + name + "=" + value); // NOSONAR for docker console
		} catch (Exception e) {
			value = defaultValue;
			log.debug("qacover.properties not found, using default value: " + name + "=" + value); // NOSONAR for docker console
		}
		return value;
	}

	/**
	 * Reads a property that represents a list of comma separated values
	 */
	private List<String> getMultiValueProperty(String name) {
		List<String> values = new ArrayList<>();
		String str = getProperty(name, "");
		if (!"".equals(str)) {
			String[] arr = JavaCs.splitByChar(str, ',');
			for (String item : arr)
				if (!"".equals(item.trim()))
					values.add(item.trim());
		}
		return values;
	}

	/**
	 * Checks if a value matches any of the values of a list (case insensitive).
	 * If matchStartsWith=true checks only that the value is a prefix.
	 */
	public static boolean valueInProperty(String value, List<String> searchIn, boolean matchStartsWith) {
		for (String item : searchIn) {
			if (matchStartsWith && value.toLowerCase().startsWith(item.toLowerCase())
					|| !matchStartsWith && JavaCs.equalsIgnoreCase(value, item))
				return true;
		}
		return false;
	}
	
	public Configuration setName(String name) {
		this.name = name;
		return this;
	}
	public String getName() {
		return this.name;
	}

	public String getStoreRulesLocation() {
		return storeRulesLocation;
	}
	public void setStoreRulesLocation(String location) {
		storeRulesLocation = location;
	}
	public String getCacheRulesLocation() {
		return cacheRulesLocation;
	}
	public Configuration setCacheRulesLocation(String location) {
		// Ensures that location is relative to project root and not null
		cacheRulesLocation = location != null && !"".equals(location) ? FileUtil.getPath(Parameters.getProjectRoot(), location) : "";
		return this;
	}
	public String getStoreReportsLocation() {
		return storeReportsLocation;
	}
	public void setStoreReportsLocation(String location) {
		storeReportsLocation = location;
	}

	public String getRuleServiceType() {
		return ruleServiceType;
	}
	public String getFpcServiceUrl() {
		return fpcServiceUrl;
	}
	public String getFpcServiceOptions() {
		return fpcServiceOptions;
	}
	public Configuration setRuleServiceType(String ruleServiceType) {
		this.ruleServiceType = ruleServiceType;
		return this;
	}
	public Configuration setFpcServiceOptions(String fpcServiceOptions) {
		this.fpcServiceOptions = fpcServiceOptions;
		return this;
	}
	public Configuration setFpcServiceUrl(String fpcServiceUrl) {
		this.fpcServiceUrl = fpcServiceUrl;
		return this;
	}

	public boolean getOptimizeRuleEvaluation() {
		return optimizeRuleEvaluation;
	}
	public Configuration setOptimizeRuleEvaluation(boolean optimizeRuleEvaluation) {
		this.optimizeRuleEvaluation = optimizeRuleEvaluation;
		return this;
	}

	public boolean getInferQueryParameters() {
		return inferQueryParameters;
	}
	public Configuration setInferQueryParameters(boolean inferQueryParameters) {
		this.inferQueryParameters = inferQueryParameters;
		return this;
	}

	public List<String> getStackExclusions() {
		return stackExclusions;
	}
	public List<String> getStackInclusions() {
		return stackInclusions;
	}
	public Configuration addStackExclusion(String exclusion) { // for testing
		stackExclusions.add(exclusion);
		return this;
	}

	public List<String> getClassExclusions() {
		return classExclusions;
	}
	public Configuration addClassExclusion(String exclusion) { // for testing
		classExclusions.add(exclusion);
		return this;
	}

	public List<String> getTableExclusionsExact() {
		return tableExclusionsExact;
	}
	public Configuration addTableExclusionExact(String exclusion) { // for testing
		tableExclusionsExact.add(exclusion);
		return this;
	}

	public String getDbStoretype() {
		return this.dbStoretype;
	}
	public void setDbStoretype(String storetype) {
		this.dbStoretype = storetype;
		log.info("Configure db store type as: {}", storetype);
	}

}
