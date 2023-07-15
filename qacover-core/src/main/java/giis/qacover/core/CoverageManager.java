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

	// Different constructors for new rules, existing and errors
	public CoverageManager() {
		super();
	}
	public CoverageManager(QueryModel model) {
		this.model = model;
	}
	public CoverageManager(String sql, String error) {
		this.model = new QueryModel(sql, error);
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
	public boolean isAborted() {
		return abortedStatus;
	}

	/**
	 * Generates a new set of coverage rules for a given query, that are kept in a QueryModel
	 */
	public void generate(RuleServices svc, StoreService store, QueryStatement stmt, String sql) {
		FaultInjector faultInjector = stmt.getFaultInjector();
		// To better handling large schemas first gets the tables involved in the query
		log.debug("Getting table names");
		store.setLastGeneratedSql(sql);
		if (faultInjector != null && faultInjector.isSchemaFaulty())
			sql = stmt.getFaultInjector().getSchemaFault();
		String[] tables = svc.getAllTableNames(sql);
		log.debug("Table names: " + JavaCs.deepToString(tables));
		
		// Check table name exclusions
		for (String table : tables)
			if (Configuration.valueInProperty(table, Configuration.getInstance().getTableExclusionsExact(), false)) {
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
		Configuration config=Configuration.getInstance();
		String fpcOptions = "clientname=" + config.getName() + new Variability().getVariantId() + " clientversion=" + clientVersion
				+ " numberjdbcparam" + " " + config.getFpcServiceOptions();
		store.setLastGeneratedInRules(svc.getRulesInput(sql, this.schema, fpcOptions));
		model = svc.getRulesModel(sql, this.schema, fpcOptions);
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
		stmt.setVariant(new Variability(model.getDbms()));
		// Executes and annotates the coverage/status of each rule
		for (int i = 0; i < rules.size(); i++) {
			ManagedRule rule = new ManagedRule(rules.get(i));
			String res;
			if (options.getOptimizeRuleEvaluation() && rule.getModel().getDead() > 0)
				res = ManagedRule.ALREADY_COVERED;
			else
				res = rule.run(stmt);
			
			// Restults for logging
			String logString = getLogString(rule, stmt, res);
			logsb.append((i == 0 ? "" : "\n") + logString);
			log.debug(logString);
			
			// store results
			if (rule.getModel().getDead() > 0)
				model.addDead(1);
			if (rule.getModel().getError() > 0)
				model.addError(1);
		}
		model.setCount(rules.size());
		model.addQrun(1);
		log.info(" SUMMARY: Covered " + model.getDead() + " out of " + model.getCount());
		store.setLastRulesLog(logsb.toString());
	}

	private String getLogString(ManagedRule rule, QueryStatement stmt, String res) {
		String ruleWithSql = rule.getSqlWithValues(stmt).replace("\r", "").replace("\n", " ").trim();
		String logString = res + " " + (ManagedRule.COVERED.equals(res) ? "  " : "") + ruleWithSql;
		logString += ManagedRule.RUNTIME_ERROR.equals(res) ? "\n" + rule.getModel().getRuntimeError() : "";
		return logString;
	}

}
