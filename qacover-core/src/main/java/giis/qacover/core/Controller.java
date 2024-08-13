package giis.qacover.core;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import giis.qacover.core.services.Configuration;
import giis.qacover.core.services.RuleServices;
import giis.qacover.core.services.StackLocator;
import giis.qacover.core.services.StoreService;
import giis.qacover.model.QueryModel;
import giis.qacover.model.SchemaModel;
import giis.qacover.portable.QaCoverException;

/**
 * When p6spy finds an elegible statement to evaluate the coverage, 
 * creates a new controller instancia and calls it to manage the process.
 * 
 * This class manages the storage of the rules, generation of new or use of
 * existing rules, calling the CoverageManager to get the coverage and top level
 * errors.
 */
public class Controller {
	private static final Logger log = LoggerFactory.getLogger(Controller.class);

	public void processSql(QueryStatement stmt) {
		log.debug("**** processSql");
		Configuration options = Configuration.getInstance();
		StoreService store = new StoreService(options);
		StackLocator stack = new StackLocator();
		RuleServices svc = new RuleServices(stmt.getFaultInjector());
		CoverageManager rm;
		log.info("LOCATION: " + stack.getLocationAsString());

		// Checking exclusion list
		if (Configuration.valueInProperty(stack.getClassName(), options.getClassExclusions(), true)) {
			log.warn("Class " + stack.getClassName() + " found in class exclusions property, skip");
			return;
		}
		// Processing and storing, controls successful/unsuccessful.
		try {
			if (stmt.getException() != null)
				throw stmt.getException();
			rm = mainProcessSql(svc, store, stack, stmt, options); // ejecuta reglas de la query
			if (rm.isAborted()) // aborted, maybe due to of exclusion of queries/tables
				return;
		} catch (Exception e) {
			rm = mainProcessError(svc, store, stmt, e);
		}
		log.debug("Saving evaluation results");
		rm.getModel().setLocation(stack.getClassName(), stack.getMethodName(), stack.getLineNumber(),
				stack.getFileName(), stack.getSourceFileName());
		store.put(stack, stmt.getSql(), stmt.getParameters(), rm.getModel(), rm.getSchemaAtRuleGeneration(), rm.getResult());
	}

	private CoverageManager mainProcessError(RuleServices svc, StoreService store, QueryStatement stmt, Throwable e) {
		String errorMsg = "Error at " + svc.getErrorContext() + ": " + QaCoverException.getString(e);
		String sql = stmt != null ? stmt.getSql() : "";
		RuleDriver rd = new RuleDriverFactory().getDriver();
		CoverageManager rm = new CoverageManager(rd, sql, errorMsg); // construye error en el formato xml de las reglas
		store.setLastGenStatus(errorMsg);
		log.error("  ERROR: " + errorMsg, e);
		return rm;
	}

	private CoverageManager mainProcessSql(RuleServices svc, StoreService store, StackLocator stack, QueryStatement stmt,
			Configuration options) {
		// Set the configuration of data store type if not already set
		if (options.getDbStoretype() == null) {
			SchemaModel schema = svc.getSchemaModel(stmt.getConnection(), "", "", new String[] {});
			options.setDbStoretype(schema.getDbms());
		}

		// Optional parameter inference may lead to a change in the sql and parameters
		if (options.getInferQueryParameters() && stmt.getParameters().size() == 0)
			stmt.inferParameters(svc, options.getDbStoretype());
		log.info("  SQL: " + stmt.getSql());
		log.info("  PARAMS: " + stmt.getParameters().toString());
		store.setLastParametersRun(stmt.getParameters().toString());

		// CoverageManager is constructed from rules generated in a previous query
		// or by generating a fresh set of rules
		RuleDriver rd = new RuleDriverFactory().getDriver(); // delegate to get and evaluate the rules
		CoverageManager rm = getCoverageManager(rd, store, stack, stmt, options.getRuleCriterion());
		if (rm == null) {
			log.debug("Generating new coverage rules for this query");
			rm = new CoverageManager(rd);
			rm.generate(svc, store, stmt, stmt.getSql(), options);
		} else {
			log.debug("Using existing coverage rules for this query");
		}
		if (rm.isAborted()) {
			log.warn("SKIP Coverage rules evaluation");
			return rm;
		}
		if (rm.getModel().getQerror() > 0) {
			log.warn("The set of coverage rules had previous errors, skip evaluation");
			return rm;
		}
		log.debug("BEGIN Coverage rules evaluation");
		rm.run(svc, store, stmt, options);
		store.setLastGenStatus("success");
		log.debug("END Coverage rules evaluation");
		return rm;
	}

	private CoverageManager getCoverageManager(RuleDriver ruleDriver, StoreService store, StackLocator stack, QueryStatement stmt, String currentCriterion) {
		QueryModel model = store.get(stack.getClassName(), stack.getMethodName(), stack.getLineNumber(), stmt.getSql());
		if (model == null)
			return null; // to signal the need to create a new model
		// If a previous run generated a model with a different criterion, 
		// a new coverage manager must be created to overwrite the existing model
		String ruleCriterion = model.getModel().getRulesClass();
		if (!ruleCriterion.equals(currentCriterion)) {
			log.warn("Current {} coverage criterion does not match with the stored rule {} criterion."
					+ " Existing rule will be overwritten", currentCriterion, ruleCriterion);
			return null; // to signal the need to create a new model that will overwrite the stored rule
		}
		// new empty rule
		return new CoverageManager(ruleDriver, model);
	}

}
