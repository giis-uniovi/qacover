package giis.qacover.driver;

import java.sql.Connection;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.sql.Statement;
import java.util.ArrayList;
import java.util.List;

import giis.qacover.core.IQueryStatementReader;
import giis.qacover.portable.QaCoverException;

/**
 * Provides access to the data stored in the database to evaluate the rules
 */
public class QueryStatementReader implements IQueryStatementReader {
	private Connection conn;
	private String sql;

	public QueryStatementReader(Connection conn, String sql) {
		this.conn = conn;
		this.sql = sql;
	}

	@Override
	public boolean hasRows() {
		// any exception is propagated to detect failures in individual rules
		try (Statement stmt = conn.createStatement()) {
			stmt.setMaxRows(1);
			try (ResultSet rs = stmt.executeQuery(sql)) {
				return rs.next();
			}
		} catch (SQLException e) {
			throw new QaCoverException("QueryReader.hasRows", e);
		}
	}

	@Override
	public List<String[]> getRows() {
		List<String[]> rows = new ArrayList<>();
		try (Statement stmt = conn.createStatement(); ResultSet rs = stmt.executeQuery(sql)) {
			int numCols = rs.getMetaData().getColumnCount();
			while (rs.next())
				rows.add(getRow(rs, numCols));
			return rows;
		} catch (SQLException e) {
			throw new QaCoverException("QueryReader.getRows", e);
		}
	}

	private String[] getRow(ResultSet rs, int numCols) throws SQLException {
		String[] row = new String[numCols];
		for (int i = 0; i < numCols; i++) {
			String value = rs.getString(i + 1);
			row[i] = rs.wasNull() ? null : value;
		}
		return row;
	}

	@Override
	public boolean equalRows(List<String[]> expected) {
		int currentRow = -1;
		try (Statement stmt = conn.createStatement(); ResultSet rs = stmt.executeQuery(sql)) {
			int numCols = rs.getMetaData().getColumnCount();
			while (rs.next()) {
				currentRow++;
				if (currentRow > expected.size() - 1) // more rows in db than in the list
					return false;
				if (!equalRow(rs, expected.get(currentRow), numCols))
					return false;
			}
			if (expected.size() > currentRow + 1) // NOSONAR more clear logic
				return false;
			return true;
		} catch (SQLException e) {
			throw new QaCoverException("QueryReader.equalRows", e);
		}
	}

	private boolean equalRow(ResultSet rs, String[] expected, int numCols) throws SQLException {
		if (expected.length != numCols)
			return false;
		String[] actual = getRow(rs, numCols);
		for (int i = 0; i < expected.length; i++)
			if (expected[i] == null && actual[i] != null || expected[i] != null && actual[i] == null // NOSONAR already indented
					|| (expected[i] != null && actual[i] != null && !expected[i].equals(actual[i])))
				return false;
		return true;
	}

}
