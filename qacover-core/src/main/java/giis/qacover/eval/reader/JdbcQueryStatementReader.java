package giis.qacover.eval.reader;

import java.sql.Connection;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.sql.Statement;
import java.util.ArrayList;
import java.util.List;

import giis.qacover.eval.query.IQueryStatementReader;
import giis.qacover.portable.QaCoverException;

/**
 * Implements the jdbc access to the data stored in the database to evaluate the rules
 */
public class JdbcQueryStatementReader implements IQueryStatementReader {
	private Connection conn;
	private String sql;

	public JdbcQueryStatementReader(Connection conn, String sql) {
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
			throw new QaCoverException("IQueryStatementReader.hasRows", e);
		}
	}

	@Override
	public List<String[]> getRows() {
		List<String[]> rows = new ArrayList<String[]>();
		try (Statement stmt = conn.createStatement(); ResultSet rs = stmt.executeQuery(sql)) {
			while (rs.next())
				rows.add(getRow(rs));
			return rows;
		} catch (SQLException e) {
			throw new QaCoverException(e);
		}
	}

	private String[] getRow(ResultSet rs) throws SQLException {
		int numCols = rs.getMetaData().getColumnCount();
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
			while (rs.next()) {
				currentRow++;
				if (currentRow > expected.size() - 1) // more rows in db than in the list
					return false;
				String[] actual = getRow(rs);
				if (!equalRow(expected.get(currentRow), actual))
					return false;
			}
			// expected still has more rows
			if (expected.size() > currentRow + 1) // NOSONAR more clear logic
				return false;
			return true;
		} catch (SQLException e) {
			throw new QaCoverException("IQueryStatementReader.equalRows", e);
		}
	}

	// This should use native array comparison when migrating the java version
	public static boolean equalRow(String[] expected, String[] actual) {
		if (expected.length != actual.length)
			return false;
		for (int i = 0; i < expected.length; i++)
			if (expected[i] == null && actual[i] != null || expected[i] != null && actual[i] == null // NOSONAR already indented
					|| (expected[i] != null && actual[i] != null && !expected[i].equals(actual[i])))
				return false;
		return true;
	}

}
