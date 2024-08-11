package giis.qacover.core;

import java.util.List;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import giis.portable.util.JavaCs;
import giis.qacover.core.services.FaultInjector;
import giis.qacover.core.services.Configuration;
import giis.qacover.core.services.RuleServices;
import giis.qacover.core.services.StoreService;
import giis.qacover.model.QueryModel;
import giis.qacover.model.ResultVector;
import giis.qacover.model.RuleModel;
import giis.qacover.model.SchemaModel;
import giis.qacover.model.Variability;

/**
 * Manages the generation and execution of the rules for a query 
 * and keeps track of the coverage results
 */
public class CoverageManager {
	private boolean abortedStatus = false; // generation process interrupted for any reason
	private static final Logger log = LoggerFactory.getLogger(CoverageManager.class);
	private QueryModel model;
	private SchemaModel schema;
	private RuleDriver ruleDriver; // a delegate to get and evaluate the rules
	private ResultVector result = new ResultVector(0); // to avoid null if fails before generating rules

	// Different constructors for new rules, existing and errors
	public CoverageManager(RuleDriver ruleDriver) {
		this.ruleDriver = ruleDriver;
	}
	public CoverageManager(RuleDriver ruleDriver, QueryModel model) {
		this.model = model;
		this.ruleDriver = ruleDriver;
	}
	public CoverageManager(RuleDriver ruleDriver, String sql, String error) {
		this.model = new QueryModel(sql, error);
		this.ruleDriver = ruleDriver;
	}

	public QueryModel getModel() {
		return this.model;
	}
	/**
	 * Gets the model of the schema used to generate the rules, note that this is only present
	 * the first execution (when the rules are generated), the rest of executions
	 * retrieve the rules from the store and set the schema as null
	 */
	public SchemaModel getSchemaAtRuleGeneration() {
		return this.schema;
	}
	public ResultVector getResult() {
		return result;
	}
	public boolean isAborted() {
		return abortedStatus;
	}

	/**
	 * Generates a new set of coverage rules for a given query, that are kept in a QueryModel
	 */
	public void generate(RuleServices svc, StoreService store, QueryStatement stmt, String sql, Configuration config) {
		FaultInjector faultInjector = stmt.getFaultInjector();
		// To better handling large schemas first gets the tables involved in the query
		log.debug("Getting table names");
		store.setLastGeneratedSql(sql);
		if (faultInjector != null && faultInjector.isSchemaFaulty())
			sql = stmt.getFaultInjector().getSchemaFault();
		String[] tables = svc.getAllTableNames(sql, config.getDbStoretype());
		log.debug("Table names: " + JavaCs.deepToString(tables));
		
		// Check table name exclusions
		for (String table : tables)
			if (Configuration.valueInProperty(table, config.getTableExclusionsExact(), false)) {
				log.warn("Table " + table + " found in table exclusions property, skip generation");
				abortedStatus = true;
				return;
			}

		// Now, gets the schema with only the tables involved in teh query
		log.debug("Getting database schema");
		this.schema = svc.getSchemaModel(stmt.getConnection(), "", "", tables);

		// Gets the rules, always numbering jdbc parameters for further substitution
		log.debug("Getting sql coverage rules");
		String clientVersion = new Variability().getVersion();
		String fpcOptions = "clientname=" + config.getName() + new Variability().getVariantId() + " clientversion=" + clientVersion
				+ " numberjdbcparam" 
				+ ("mutation".equals(config.getRuleServiceType()) ? " getparsedquery" : "") // required to evaluate query
				+ " " + config.getFpcServiceOptions();
		store.setLastGeneratedInRules(svc.getRulesInput(sql, this.schema, fpcOptions));
		model = ruleDriver.getRules(svc, sql, this.schema, fpcOptions);
		// The DBMS is stored too to manage its variability in further actions
		model.setDbms(this.schema.getDbms());
	}

	/**
	 * Executes the rules that must be previously generated
	 */
	public void run(RuleServices svc, StoreService store, QueryStatement stmt, Configuration options) {
		svc.setErrorContext("Run SQLFpc coverage rules");
		store.setLastSqlRun(stmt.getSql());
		StringBuilder logsb = new StringBuilder();
		
		List<RuleModel> rules = model.getRules();
		FaultInjector injector = stmt.getFaultInjector();
		if (injector != null && injector.isSingleRuleFaulty())
			rules.get(0).setSql(injector.getSingleRuleFault());
		model.reset();
		result = new ResultVector(rules.size());
		stmt.setVariant(new Variability(model.getDbms()));
		
		// Mutation requires a previous step to read query results
		// note that it requires generate the rules to get the parsed query
		// that includes numbers in the parameters
		ruleDriver.prepareEvaluation(stmt, model.getModel().getParsedquery());

		// Executes and annotates the coverage/status of each rule
		for (int i = 0; i < rules.size(); i++) {
			RuleModel ruleModel = rules.get(i);
			String res;
			if (options.getOptimizeRuleEvaluation() && ruleModel.getDead() > 0)
				res = ResultVector.ALREADY_COVERED;
			else
				res = ruleDriver.evaluateRule(ruleModel, stmt);
			
			// Results for logging
			String logString = ruleDriver.getLogString(ruleModel, stmt, res);
			logsb.append((i == 0 ? "" : "\n") + logString);
			log.debug(logString);
			
			// store results
			result.setResult(i, res);
			if (ruleModel.getDead() > 0)
				model.addDead(1);
			if (ruleModel.getError() > 0)
				model.addError(1);
		}
		model.setCount(rules.size());
		model.addQrun(1);
		log.info(" SUMMARY: Covered " + model.getDead() + " out of " + model.getCount());
		store.setLastRulesLog(logsb.toString());
	}

}
