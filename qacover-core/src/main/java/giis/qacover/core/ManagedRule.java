package giis.qacover.core;

import giis.qacover.model.RuleModel;

/**
 * Wraps a rule, providing actions needed by the CoverageManager
 */
public class ManagedRule {
	public static final String RUNTIME_ERROR = "RUNTIME_ERROR";
	public static final String UNCOVERED = "UNCOVERED";
	public static final String COVERED = "COVERED";
	public static final String ALREADY_COVERED = "ALREADY_COVERED";
	private RuleModel model;

	public ManagedRule(RuleModel model) {
		this.model = model;
	}

	public RuleModel getModel() {
		return this.model;
	}

	/**
	 * Determines the coverage of this rule, saving the results, returning if it is covered
	 */
	public String run(QueryStatement stmt) {
		String sqlWithoutValues = model.getSql();
		model.addCount(1);
		try { // save results
			boolean isCovered = stmt.hasRows(sqlWithoutValues);
			model.addDead(isCovered ? 1 : 0);
			return isCovered ? COVERED : UNCOVERED;
		} catch (Exception e) {
			model.addError(1);
			model.addErrorString(e.toString());
			model.setRuntimeError(e.toString());
			return RUNTIME_ERROR;
		}
	}

	/**
	 * Gets the sql of the rule with the parameters replaced
	 */
	public String getSqlWithValues(QueryStatement stmt) {
		String sql = model.getSql();
		return stmt.getSqlWithValues(sql);
	}

}
