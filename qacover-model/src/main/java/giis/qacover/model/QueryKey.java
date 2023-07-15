package giis.qacover.model;

import giis.portable.util.JavaCs;

/**
 * Unique identifier of a query that is being evaluated in the form
 * package+class.method.line.hash, where the cash encodes the SQL query to allow
 * differentiate between queries that are executed from the same line.
 */
public class QueryKey {
	private String key;

	public QueryKey(String className, String methodName, int lineNumber, String sql) {
		key = className + "." + methodName + "." + lineNumber + "." + getSqlHash(sql);
		// On java, queries executed in the constructor are <init>, replace this kind of chars
		if (key.contains("<"))
			key = key.replace("<", "-");
		if (key.contains(">"))
			key = key.replace(">", "-");
	}

	public QueryKey(String stringKey) {
		key = stringKey;
		// If string comes from a filename, removes the extension because is not part of the key
		if (key.endsWith(".xml"))
			key = JavaCs.substring(key, 0, key.length() - 4);
	}

	private String getSqlHash(String sql) {
		if (new Variability().isJava4())
			return JavaCs.getHashMd5(sql); //standard sha256 hash not available in Java 1.4
		else
			return JavaCs.getHash(sql);
	}

	@Override
	public String toString() {
		return key;
	}

	public String getKey() {
		return key;
	}
	public String getClassName() {
		String[] parts = JavaCs.splitByDot(key);
		StringBuilder sb = new StringBuilder();
		for (int i = 0; i < parts.length - 3; i++) // remove method.line.sql
			sb.append(i == 0 ? "" : ".").append(parts[i]);
		return sb.toString();
	}
	public String getMethodName() {
		return getMethodName(false);
	}
	/**
	 * Gets the method name with the line number (opt in)
	 */
	public String getMethodName(boolean includeLineNumbers) {
		String[] parts = JavaCs.splitByDot(key);
		return parts[parts.length - 3] + (includeLineNumbers ? ":" + parts[parts.length - 2] : "");
	}
	public String getClassLine() {
		String[] parts = JavaCs.splitByDot(key);
		return parts[parts.length - 2];
	}

}
