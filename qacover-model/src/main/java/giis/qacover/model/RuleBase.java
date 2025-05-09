package giis.qacover.model;

import giis.portable.util.JavaCs;

/**
 * Base class to store the standard results/indicators of the evaluation of 
 * (1) individual rules and (2) aggregated information of the evaluation of queries
 */
public abstract class RuleBase {
	protected static final String RDEAD = "dead"; // Covered
	protected static final String RCOUNT = "count";
	protected static final String RERROR = "error";
	public static final String TAG_ERROR = "error";

	// The indicator are stored as attributes, the implementatin
	// is specific to the subclass (query or rule)
	protected abstract String getAttribute(String name);
	protected abstract void setAttribute(String name, String value);

	@Override
	public String toString() {
		return "count=" + getCount() + ",dead=" + getDead() + (this.getError() > 0 ? ",error=" + getError() : "");
	}

	public void reset() {
		setCount(0);
		setDead(0);
		setError(0);
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
		if (getError() > 0) // evita este atributo si no hay error por ser excepcional (nunca decrementara hasta cero)
			setAttribute(RERROR, JavaCs.numToString(error));
	}
	public void addError(int value) {
		incrementIntAttribute(RERROR, value);
	}

}
