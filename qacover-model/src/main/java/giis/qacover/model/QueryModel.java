package giis.qacover.model;

import static giis.tdrules.model.ModelUtil.safe;

import java.util.ArrayList;
import java.util.List;

import giis.tdrules.openapi.model.TdRule;
import giis.tdrules.openapi.model.TdRules;

/**
 * Representation of a query, its rules and the results/indicators about the evaluation.
 * It is a wrapper of the TdRules model with additional information about the evaluation
 */
public class QueryModel extends RuleBase {
	// In addition to the standard attributes, indicates if the query itself 
	// has errors (it can't be evaluated)
	private static final String QERROR = "qerror";
	// How many times has been evaluated
	private static final String QRUN = "qrun";
	// It stores additional information about the query location
	private static final String CLASS_NAME = "class";
	private static final String METHOD_NAME = "method";
	private static final String LINE_NUMBER = "line";
	private static final String CLASS_FILE_NAME = "file";

	// The model of the rules that is wrapped here
	protected TdRules model = null;

	@Override
	protected String getAttribute(String name) {
		return safe(model.getSummary(), name);
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

	public String getTextSummary() {
		StringBuilder sb = new StringBuilder();
		// The summary may have an additional error message
		String strSummary = (this.getQerror() > 0 ? "qerror=" + this.getQerror() + "," : "") + super.toString();
		sb.append(strSummary);
		for (RuleModel rule : this.getRules())
			sb.append("\n").append(rule.getTextSummary());
		return sb.toString();
	}

	public void setLocation(String className, String methodName, int lineNumber, String fileName) {
		setAttribute(CLASS_NAME, className);
		setAttribute(METHOD_NAME, methodName);
		setAttribute(LINE_NUMBER, String.valueOf(lineNumber));
		setAttribute(CLASS_FILE_NAME, fileName);
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
		setAttribute(QERROR, String.valueOf(value));
	}
	public String getSql() {
		return model.getQuery();
	}

	public List<RuleModel> getRules() {
		List<RuleModel> rules = new ArrayList<>();
		for (TdRule rule : safe(model.getRules()))
			rules.add(new RuleModel(rule));
		return rules;
	}

	public String getErrorString() {
		return model.getError();
	}

	@Override
	public String toString() {
		return model.toString();
	}
}
