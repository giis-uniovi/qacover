package giis.qacover.model;

import giis.tdrules.model.shared.ModelUtil;
import giis.tdrules.openapi.model.TdRule;

/**
 * Representation of a coverage rule and the measures obtained after the evaluation:
 * - dead: number of times the has been covered
 * - count: number of times the rule has been evaluated
 * - error: number of times the rule raised an error when evaluated
 * - errorString: Contains all error messages (if any).
 * 
 * Wraps a TdRules model and stores the measures in its summary attribute. Provides getters and setters to
 * manage these measures.
 */
public class RuleModel extends RuleBase {
	// The model of the rules that is wrapped here
	protected TdRule model = null;

	public RuleModel(TdRule ruleModel) {
		model = ruleModel;
	}

	/**
	 * Returns the wrapped TdRule model
	 */
	public TdRule getModel() {
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
		return model.getQuery();
	}
	public void setSql(String sql) {
		model.setQuery(sql);
	}
	
	/**
	 * Returns a string representation of the main coverage measures of this rule
	 */
	public String getTextSummary() {
		return super.toString();
	}

	// Other specific attributes of the wrapped model
	public String getId() {
		return model.getId();
	}
	public String getCategory() {
		return model.getCategory();
	}
	public String getMainType() {
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
	
	/**
	 *  Reset values of this rule
	 */
	public void reset() {
		super.setDead(0);
		super.setCount(0);
		super.setError(0);
	}

	@Override
	public String toString() {
		return model.toString();
	}

}
