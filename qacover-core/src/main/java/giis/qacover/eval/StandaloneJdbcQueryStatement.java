package giis.qacover.eval;

import java.sql.Connection;
import java.util.Map;
import java.util.Map.Entry;

import giis.qacover.eval.coverage.AbstractQueryStatement;
import giis.qacover.eval.reader.JdbcQueryStatementReader;

class StandaloneJdbcQueryStatement extends AbstractQueryStatement {

	private Connection conn;

	public StandaloneJdbcQueryStatement(Connection conn, String sql, Map<String, String> params) {
		this.conn = conn;
		super.sql = sql;
		for (Entry<String, String> entry : params.entrySet())
			super.parameters.putItem(entry.getKey(), entry.getValue(), entry.getValue());
	}

	@Override
	public Connection getConnection() {
		return conn;
	}

	@Override
	public JdbcQueryStatementReader getReader(String sql) {
		return new JdbcQueryStatementReader(conn, getSqlWithValues(sql));
	}

}
