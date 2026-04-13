package giis.qacover.eval;

import java.sql.Connection;
import java.util.HashMap;
import java.util.Map;

import giis.qacover.core.coverage.CoverageService;
import giis.qacover.model.QueryModel;

/**
 * A standalone evaluator of fpc and mutation coverage rules that does not depends on the QACover
 * configurations and p6spy interceptors (Only available for Java)
 * 
 * Provides a main overloaded method (evaluate) that given a model of the rules performs the appropriate
 * evaluation according to the rule class of the model. The required model is a wrapper of a TdRules model
 * obtained from the TdRules service. The indicators about coverage are stored in the TdRules model in the
 * summary attribute (both in the query and in each rule). These attributes can be accessed by methods in the
 * wrapper.
 * 
 * Also provides additional fluent setters to configure the logging of the rules evaluated and to optimize the
 * evaluation by skipping execution of rules that are already covered.
 */
public class StandaloneEvaluator {

	private Connection conn;
	private boolean debugSql = false;
	private boolean skipIfCovered = false;

	/**
	 * Creates an evaluator with an open Jdbc connection,
	 */
	public StandaloneEvaluator(Connection conn) {
		this.conn = conn;
	}

	/**
	 * If set to true configures the evaluator to do not re-execute rules that have already been covered,
	 */
	public StandaloneEvaluator setSkipIfCovered(boolean skip) {
		this.skipIfCovered = skip;
		return this;
	}

	/**
	 * If set to true configures the evaluator to log the SQL of the queries that are executed
	 */
	public StandaloneEvaluator setDebugSql(boolean debug) {
		this.debugSql = debug;
		return this;
	}

	/**
	 * Performs the evaluation of a model where the query and rules are parametrized with the style of TdRules
	 * (?1?, ?2? ...). The parameters are stored as a map of parameter and value.
	 */
	public void evaluate(QueryModel model, Map<String, String> params) {
		StandaloneJdbcQueryStatement stmt = new StandaloneJdbcQueryStatement(conn, model.getSql(), params);

		CoverageService coverage = new CoverageService(model).setDebugSql(debugSql);
		coverage.evaluateQuery(stmt, skipIfCovered);
	}

	/**
	 * Performs the evaluation of a model with additional optional arguments to specify the parameter values that
	 * are passed to the query. If using parameters, their names in the rules must follow the style of TdRules
	 * (?1?, ?2? ...).
	 */
	public void evaluate(QueryModel model, String... paramValues) {
		Map<String, String> paramMap = new HashMap<>();
		for (int i = 1; i <= paramValues.length; i++)
			paramMap.put("?" + i + "?", paramValues[i - 1]);
		this.evaluate(model, paramMap);
	}

}
