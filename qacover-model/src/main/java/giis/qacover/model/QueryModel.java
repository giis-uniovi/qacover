package giis.qacover.model;

import java.util.ArrayList;
import java.util.List;

import giis.portable.util.JavaCs;
import giis.tdrules.model.shared.ModelUtil;
import giis.tdrules.openapi.model.TdRule;
import giis.tdrules.openapi.model.TdRules;

/**
 * Representation of a query, its rules and the measures obtained after the evaluation: 
 * - dead: number of coverage rules that have been covered 
 * - count: number of coverage rules generated in the query
 * - qrun: number of times the query has been evaluated.
 * 
 * Wraps a TdRules model and stores the measures in its summary attribute. Provides getters and setters to
 * manage these measures.
 */
public class QueryModel extends RuleBase {
	// How many times has been evaluated
	private static final String QRUN = "qrun";
	
	// Below additional attributes are used only when executor runs integrated with query interception,
	// they are not used when using the StandaloneEvaluator
	
	// Error message if the query itself has errors (it can't be evaluated)
	private static final String QERROR = "qerror";
	// Additional information about the query location in the source code
	private static final String CLASS_NAME = "class";
	private static final String METHOD_NAME = "method";
	private static final String LINE_NUMBER = "line";
	private static final String CLASS_FILE_NAME = "file";
	private static final String SOURCE_FILE_NAME = "source";

	// The model of the rules that is wrapped here
	protected TdRules model = null;

	@Override
	protected String getAttribute(String name) {
		return ModelUtil.safe(model.getSummary(), name);
	}
	@Override
	protected void setAttribute(String name, String value) {
		model.putSummaryItem(name, value);
	}

	/**
	 * Creates an instance with the wrapped model
	 */
	public QueryModel(TdRules rulesModel) {
		model = rulesModel;
	}

	/**
	 * Returns the wrapped TdRules model
	 */
	public TdRules getModel() {
		return model;
	}

	/**
	 * Creates an instance used to signal an error in the query execution,
	 * containing the sql and error message only
	 */
	public QueryModel(String sql, String error) {
		model = new TdRules();
		model.setRulesClass("sqlfpc");
		model.setQuery(sql);
		model.setError(error);
		this.setQerror(1); // para contabilizar en totales al igual que la cobertura
	}
	
	public String getDbms() {
		return getAttribute("dbms");
	}
	public void setDbms(String value) {
		this.setAttribute("dbms", value);
	}

	/**
	 * Returns a string representation of the main coverage measures of this query and each of its rules (one per row)
	 */
	public String getTextSummary() {
		return getTextSummary(false);
	}
	// By default do not include qrun, only for test, at least at this moment
	public String getTextSummary(boolean includeQRun) {
		StringBuilder sb = new StringBuilder();
		// The summary may have an additional error message
		String strSummary = (this.getQerror() > 0 ? "qerror=" + this.getQerror() + "," : "") 
				+ super.toString() 
				+ (includeQRun ? ",qrun=" + this.getQrun() : "");
		sb.append(strSummary);
		for (RuleModel rule : this.getRules())
			sb.append("\n").append(rule.getTextSummary());
		return sb.toString();
	}

	public void setLocation(String className, String methodName, int lineNumber, String fileName, String sourceFileName) {
		setAttribute(CLASS_NAME, className);
		setAttribute(METHOD_NAME, methodName);
		setAttribute(LINE_NUMBER, JavaCs.numToString(lineNumber));
		setAttribute(CLASS_FILE_NAME, fileName);
		setAttribute(SOURCE_FILE_NAME, sourceFileName);
	}

	public int getQrun() {
		return getIntAttribute(QRUN);
	}
	public void addQrun(int value) {
		incrementIntAttribute(QRUN, value);
	}
	public int getQerror() {
		return getIntAttribute(QERROR);
	}
	public void setQerror(int value) {
		setAttribute(QERROR, JavaCs.numToString(value));
	}
	public String getSql() {
		return model.getQuery();
	}
	public String getSourceLocation() {
		return getAttribute(SOURCE_FILE_NAME);
	}
	
	/**
	 * Returns all rules stored, wrapped as RuleModel objects
	 */
	public List<RuleModel> getRules() {
		List<RuleModel> rules = new ArrayList<RuleModel>();
		for (TdRule rule : ModelUtil.safe(model.getRules()))
			rules.add(new RuleModel(rule));
		return rules;
	}

	/**
	 * Reset values dead, count and error of all rules
	 */
	public void reset(){	
		for (RuleModel rule : getRules())
			rule.reset();
	}
	
	public String getErrorString() {
		return model.getError();
	}

	@Override
	public String toString() {
		return model.toString();
	}
}
