package giis.qacover.core;

import giis.qacover.core.services.Configuration;
import giis.qacover.core.services.RuleServices;
import giis.qacover.model.QueryModel;
import giis.qacover.model.SchemaModel;


/**
 * Generate the model with the rules according to the coverage criterion specified in the configuration
 */
public class RuleGenerator {

	public QueryModel getRules(RuleServices svc, String sql, SchemaModel schema, String fpcOptions) {
		if ("mutation".equals(Configuration.getInstance().getRuleCriterion()))
			return svc.getMutationRulesModel(sql, schema, fpcOptions);
		else // fpc by default
			return svc.getFpcRulesModel(sql, schema, fpcOptions);
	}
}
