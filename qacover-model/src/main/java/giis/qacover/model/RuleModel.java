package giis.qacover.model;

import giis.tdrules.model.ModelUtil;
import giis.tdrules.openapi.model.SqlRule;

/**
 * Representation of single rule and the results/indicators about the
 * evaluation. It is a wrapper of the TdRules rule model with additional
 * information about the evaluation
 */
public class RuleModel extends RuleBase {
	protected String runtimeError = "";
	// The model of the rules that is wrapped here
	protected SqlRule model = null;

	public RuleModel(SqlRule ruleModel) {
		model = ruleModel;
	}

	public SqlRule getModel() {
		return model;
	}

	@Override
	protected String getAttribute(String name) {
		return ModelUtil.safe(model.getSummary(), name);
	}
	@Override
	protected void setAttribute(String name, String value) {
		model.putSummaryItem(name, value);
	}

	public String getSql() {
		return model.getSql();
	}
	public void setSql(String sql) {
		model.setSql(sql);
	}
	public String getTextSummary() {
		return super.toString();
	}
	public String getRuntimeError() {
		return this.runtimeError;
	}
	public void setRuntimeError(String error) {
		this.runtimeError = error;
	}

	// Other specific attributes of the wrapped model
	public String getId() {
		return model.getId();
	}
	public String getCategory() {
		return model.getCategory();
	}
	public String getType() {
		return model.getMaintype();
	}

	public String getSubtype() {
		return model.getSubtype();
	}
	public String getLocation() {
		return model.getLocation();
	}
	public String getDescription() {
		return model.getDescription();
	}

	public void addErrorString(String msg) {
		if (model.getError().contains(msg)) // Avoid repeating existing messages
			return;
		model.setError("".equals(model.getError()) ? msg : model.getError() + "\n" + msg);
	}
	public String getErrorString() {
		return model.getError();
	}

	@Override
	public String toString() {
		return model.toString();
	}

}
