package giis.qacover.core;

import giis.qacover.core.services.RuleServices;
import giis.qacover.model.QueryModel;
import giis.qacover.model.ResultVector;
import giis.qacover.model.RuleModel;
import giis.qacover.model.SchemaModel;

/**
 * Base class of the delegates that perform the required actions
 * on the coverage rules (get and evaluate the rules).
 * Subclasses will implement the specific actions that depend
 * on the coverage rule class (fpc or mutation).
 */
public abstract class RuleDriver {

	/**
	 * Generate the model with the rules
	 */
	public abstract QueryModel getRules(RuleServices svc, String sql, SchemaModel schema, String fpcOptions);

	/**
	 * Determines the coverage of a rule, saving the results and returning if it is covered
	 */
	public String evaluateRule( RuleModel model, QueryStatement stmt) {
		String sql = model.getSql();
		model.addCount(1);
		try { // save results
			boolean isCovered = isCovered(stmt, sql);
			model.addDead(isCovered ? 1 : 0);
			return isCovered ? ResultVector.COVERED : ResultVector.UNCOVERED;
		} catch (Exception e) {
			model.addError(1);
			model.addErrorString(e.toString());
			model.setRuntimeError(e.toString());
			return ResultVector.RUNTIME_ERROR;
		}
	}
	
	/**
	 * Determines if the sql of a rule generated for a query statement is covered
	 */
	protected abstract boolean isCovered(QueryStatement stmt, String sql);

	String getLogString(RuleModel ruleModel, QueryStatement stmt, String res) {
		String sql = stmt.getSqlWithValues(ruleModel.getSql());
		String ruleWithSql = sql.replace("\r", "").replace("\n", " ").trim();
		String logString = res + " " + (ResultVector.COVERED.equals(res) ? "  " : "") + ruleWithSql;
		logString += ResultVector.RUNTIME_ERROR.equals(res) ? "\n" + ruleModel.getRuntimeError() : "";
		return logString;
	}
	
}
