package giis.qacover.model;

import giis.portable.util.JavaCs;

/**
 * Base class to store common evaluation attributes related to
 * (1) individual rules and (2) aggregated information of the evaluation of queries
 */
public abstract class RuleBase {
	protected static final String RDEAD = "dead"; // Covered
	protected static final String RCOUNT = "count";
	protected static final String RERROR = "error";

	// The measures are stored in the summary attribute of the models, the implementation
	// is specific to the subclass (query or rule)
	protected abstract String getAttribute(String name);
	protected abstract void setAttribute(String name, String value);

	@Override
	public String toString() {
		// As in most of cases there is no error, it is not shown unless it has a positive value
		return "count=" + getCount() + ",dead=" + getDead() + (this.getError() > 0 ? ",error=" + getError() : "");
	}

	protected int getIntAttribute(String name) {
		String current = getAttribute(name);
		if (current == null || "".equals(current)) // no existe el atributo
			return 0;
		else
			return JavaCs.stringToInt(current);
	}
	protected void incrementIntAttribute(String name, int value) {
		String current = getAttribute(name);
		if (current == null || "".equals(current)) // no existe el atributo, pone el valor
			setAttribute(name, JavaCs.numToString(value));
		else // existe, incrementa
			setAttribute(name, JavaCs.numToString(value + JavaCs.stringToInt(getAttribute(name))));
	}

	public int getCount() {
		return getIntAttribute(RCOUNT);
	}
	public void setCount(int count) {
		setAttribute(RCOUNT, JavaCs.numToString(count));
	}
	public void addCount(int value) {
		incrementIntAttribute(RCOUNT, value);
	}
	public int getDead() {
		return getIntAttribute(RDEAD);
	}
	public void setDead(int dead) {
		setAttribute(RDEAD, JavaCs.numToString(dead));
	}
	public void addDead(int value) {
		incrementIntAttribute(RDEAD, value);
	}
	public int getError() {
		return getIntAttribute(RERROR);
	}
	public void setError(int error) {
		setAttribute(RERROR, JavaCs.numToString(error));
	}
	public void addError(int value) {
		incrementIntAttribute(RERROR, value);
	}

}
