package giis.qacover.core.query;

import java.sql.Connection;

import org.slf4j.Logger;

import giis.portable.util.JavaCs;
import giis.qacover.model.QueryParameters;
import giis.qacover.model.Variability;

/**
 * General representation of a database query statement that is going to be evaluated.
 * 
 * As this is strongly platform dependent, each platform must declare an adapter that extends this class to
 * handle specific issues such as getting the connection, the IQueryStatementReader used for evaluation and
 * parameter management.
 * 
 * Also provides a generic method to replace parameters (only used in java, as net manages this inside the
 * adapter)
 */
public abstract class AbstractQueryStatement {
	protected QueryParameters parameters = new QueryParameters();
	protected String sql;
	protected Variability variant;
	protected Exception exception = null; // si ha habido algun error en la creacion

	/**
	 * Returns the connection to the database.
	 */
	public abstract Connection getConnection();

	/**
	 * Returns an object to browse the data accessed from the current connection
	 */
	public abstract IQueryStatementReader getReader(String sql);

	public String getSql() {
		return sql == null ? "" : sql;
	}

	public QueryParameters getParameters() {
		return parameters;
	}

	public Exception getException() {
		return exception;
	}

	public void setVariant(Variability currentVariant) {
		variant = currentVariant;
	}

	/**
	 * Replaces query parameter placeholders by their values
	 */
	public String getSqlWithValues(String sourceSql) {
		if (parameters.getSize() == 0)
			return sourceSql;
		for (String name : parameters.keySet())
			sourceSql = replaceSingleParameter(sourceSql, name, getParameters().getItem(name));
		return sourceSql;
	}

	protected String replaceSingleParameter(String sourceSql, String name, String value) {
		return sourceSql.replace(name, value);
	}

	/**
	 * Determines if the query is a select. Needed to filter other statements when a generic jdbc execute() method
	 * is used (does not differentiates between query and update)
	 */
	public static boolean isSelectQuery(String sql, Logger log) {
		String sqlStart = JavaCs.substring(sql.trim(), 0, 6);
		if (JavaCs.equalsIgnoreCase("select", sqlStart.toLowerCase()))
			return true;
		log.debug("---- Ignored by qacover. clause is " + sqlStart);
		return false;
	}

}
