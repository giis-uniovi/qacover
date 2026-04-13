package giis.qacover.core.coverage;

import giis.qacover.core.query.AbstractQueryStatement;
import giis.qacover.model.QueryModel;
import giis.qacover.model.RuleModel;

/**
 * Decides whether a rule is covered or not (different implementations for fpc or mutation)
 */
interface ICoverageDecisor {

	/**
	 * Executes the preliminary actions on the query before evaluating all rules (only needed for mutation)
	 */
	void prepareEvaluation(AbstractQueryStatement stmt, QueryModel model);

	/**
	 * Gets the sql of a rule, with some preprocessing if required
	 */
	String getRuleQuery(RuleModel model);

	/**
	 * Determines if the sql of a rule generated for a query statement is covered
	 */
	boolean isCovered(AbstractQueryStatement stmt, String sql);

}