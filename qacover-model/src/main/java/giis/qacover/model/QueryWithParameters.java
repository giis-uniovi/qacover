package giis.qacover.model;

/**
 * DTO to store both a sql query and its parameters
 */
public class QueryWithParameters {
	private String sql;
	protected QueryParameters parameters = new QueryParameters();

	public QueryParameters getParams() {
		return parameters;
	}

	public void putParam(String name, String value) {
		parameters.put(name, value);
	}

	public String getSql() {
		return sql;
	}

	public void setSql(String sql) {
		this.sql = sql;
	}
}
