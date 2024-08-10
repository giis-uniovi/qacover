package giis.qacover.core;

import giis.qacover.core.services.RuleServices;
import giis.qacover.model.QueryModel;
import giis.qacover.model.SchemaModel;

/**
 * Delegate that performs the required actions
 * on the SQLfpc coverage rules (get and evaluate the rules)
 */
public class RuleDriverFpc extends RuleDriver {

	@Override
	public QueryModel getRules(RuleServices svc, String sql, SchemaModel schema, String fpcOptions) {
		return svc.getRulesModel(sql, schema, fpcOptions);
	}

	@Override
	protected boolean isCovered(QueryStatement stmt, String sql) {
		return stmt.hasRows(sql);
	}
	
}
