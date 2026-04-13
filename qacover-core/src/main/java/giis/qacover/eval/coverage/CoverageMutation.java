package giis.qacover.eval.coverage;

import java.util.List;

import giis.qacover.model.QueryModel;
import giis.qacover.model.RuleModel;

/**
 * Performs the required actions on the SQLMutation rules (get and evaluate the rules)
 */
class CoverageMutation implements ICoverageDecisor {

	private List<String[]> rows;
	private String orderCols = "";

	@Override
	public String prepareEvaluation(AbstractQueryStatement stmt, QueryModel model) {
		// Mutation needs store the query result and order by the columns in the query to get repeatable comparations
		// Requires the model have a parsed query ???
		String sql = model.getModel().getQuery();
		if (!"".equals(model.getModel().getParsedquery()))
			sql = model.getModel().getParsedquery();
		orderCols = getOrderCols(model); // also necessary to add order by to query and rules (if configured)
		sql = addOrderBy(sql, orderCols);
		this.rows = stmt.getReader(sql).getRows();
		return sql;
	}

	@Override
	public String getRuleQuery(RuleModel model) {
		// rule must have the same order than the query
		String sql = model.getSql();
		return addOrderBy(sql, this.orderCols);
	}

	@Override
	public boolean isCovered(AbstractQueryStatement stmt, String sql) {
		return !stmt.getReader(sql).equalRows(this.rows);
	}

	/**
	 * Gets the column numbers to order the queries from the model, returns empty string if no found
	 */
	private String getOrderCols(QueryModel model) {
		String order = "";
		// checks with containsKey for net compatibility (that fails if key does not exists)
		if (model.getModel().getSummary() != null && model.getModel().getSummary().containsKey("ordercols"))
			order = model.getModel().getSummary().get("ordercols");
		return order;
	}

	/**
	 * Transforms the sql query to add an order by that includes the orderCols, if not empty
	 */
	private String addOrderBy(String sql, String orderCols) {
		if (!"".equals(orderCols))
			sql += "\nORDER BY " + orderCols;
		return sql;
	}

}
