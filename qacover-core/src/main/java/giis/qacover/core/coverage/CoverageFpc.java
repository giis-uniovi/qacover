package giis.qacover.core.coverage;

import giis.qacover.core.query.AbstractQueryStatement;
import giis.qacover.model.QueryModel;
import giis.qacover.model.RuleModel;

/**
 * Performs the required actions on the SQLfpc coverage rules (get and evaluate the rules)
 */
class CoverageFpc implements ICoverageDecisor {

	@Override
	public String prepareEvaluation(AbstractQueryStatement stmt, QueryModel model) {
		// no preparation actions, evaluation is made by checking number of rows for each rule
		return "";
	}

	@Override
	public String getRuleQuery(RuleModel model) {
		return model.getSql(); // no preprocessing for fpc
	}
	
	@Override
	public boolean isCovered(AbstractQueryStatement stmt, String sql) {
		return stmt.getReader(sql).hasRows();
	}

}
