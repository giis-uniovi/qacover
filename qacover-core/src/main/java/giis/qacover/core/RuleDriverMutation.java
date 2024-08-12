package giis.qacover.core;

import java.util.List;

import giis.qacover.core.services.RuleServices;
import giis.qacover.model.QueryModel;
import giis.qacover.model.SchemaModel;

/**
 * Delegate that performs the required actions
 * on the SQLMutation rules (get and evaluate the rules)
 */
public class RuleDriverMutation extends RuleDriver {

	private List<String[]> rows;
	
	@Override
	public QueryModel getRules(RuleServices svc, String sql, SchemaModel schema, String fpcOptions) {
		return svc.getMutationRulesModel(sql, schema, fpcOptions);
	}

	@Override
	public void prepareEvaluation(QueryStatement stmt, QueryModel model, String orderCols) {
		String sql = model.getModel().getParsedquery();
		sql = addOrderBy(sql, orderCols);
		this.rows = stmt.getReader(sql).getRows();
	}
	
	@Override
	protected boolean isCovered(QueryStatement stmt, String sql) {
		return !stmt.getReader(sql).equalRows(this.rows);
	}

}
