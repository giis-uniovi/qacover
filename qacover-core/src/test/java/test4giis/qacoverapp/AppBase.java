package test4giis.qacoverapp;

import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.sql.Statement;

import giis.qacover.core.services.StackLocator;
import giis.qacover.model.Variability;
import giis.qacover.portable.QaCoverException;
import test4giis.qacover.SqlUtil;

/**
 * Base class for the mock application to test QACover.
 * This class must be excluded in the configuration stack.
 * This class is not converted to .NET (there is a specific implementation)
 */
public class AppBase {
	protected Variability variant;
	private ConnectionFactory cf;
	protected Connection conn;
	ResultSet rs;

	public AppBase(Variability targetVariant) throws SQLException {
		variant = targetVariant;
		cf = new ConnectionFactory(variant);
		conn = cf.getConnection();
	}

	public void close() throws SQLException {
		if (rs != null)
			rs.close();
		conn.close();
	}
	
	/**
	 * Delete a table if exists
	 */
	public void dropTable(String tableName) {
		if (variant.isSqlite() || variant.isH2())
			executeUpdateNative(new String[] { "drop table if exists " + tableName });
		else {
			try {
				executeUpdateNative(new String[] { "drop table " + tableName });
			} catch (QaCoverException e) {
				// ignore exception
			}
		}
	}
	
	/**
	 * Execution of a set of updates against the native connection to avoid interception, used for loading test data
	 */
	public void executeUpdateNative(String[] sqlArray) {
		Connection cn = null;
		Statement stmt = null;
		try { // no try with resources for java 1.4 compatibility
			cn = cf.getNativeConnection(); // this is the native driver that will be used
			stmt = cn.createStatement();
			for (int i = 0; i < sqlArray.length; i++)
				stmt.executeUpdate(sqlArray[i]);
		} catch (SQLException e) {
			throw new QaCoverException(e);
		} finally {
		    try { if (stmt != null) stmt.close(); } catch (Exception e) {};
		    // Tests for jdk 1.4 fail if connection is closed here!!!, keep open
		    if (variant.isJava8())		    
		    	try { if (cn != null) cn.close(); } catch (Exception e) {};
		}
	}
	/**
	 * Execution of a select against the native connection to avoid interception,
	 * used to verify results.
	 * Returns a blank separated csv
	 */
	public String executeSelectNative(String sql) {
		Connection cn = null;
		Statement stmt = null;
		ResultSet rs = null;
		try { // no try with resources for java 1.4 compatibility
			cn = cf.getNativeConnection();
			stmt = cn.createStatement();
			rs = stmt.executeQuery(sql);
			return SqlUtil.resultSet2csv(rs, " ");
		} catch (SQLException e) {
			throw new QaCoverException(e);
		} finally {
		    try { if (rs != null) rs.close(); } catch (Exception e) {};
		    try { if (stmt != null) stmt.close(); } catch (Exception e) {};
		    // Tests for jdk 1.4 fail if connection is closed here!!!, keep open
		    if (variant.isJava8())		    
		    	try { if (cn != null) cn.close(); } catch (Exception e) {};
		}
	}
	
	// Different read queries that use the database application to be intercepted 
	
	public ResultSet executeQuery(String sql) throws SQLException {
		Statement stmt = conn.createStatement();
		return stmt.executeQuery(sql);
	}
	public ResultSet executeQuery(String sql, int param1) throws SQLException {
		PreparedStatement pstmt = conn.prepareStatement(sql);
		pstmt.setInt(1, param1);
		return pstmt.executeQuery();
	}
	public ResultSet executeQueryNulls(String sql, String param1, boolean useSetNull) throws SQLException {
		PreparedStatement pstmt = conn.prepareStatement(sql);
		// setNull parameters indicates if using setNull method or a direct assignment to null
		if (useSetNull && param1 == null)
			pstmt.setNull(1, java.sql.Types.VARCHAR);
		else
			pstmt.setString(1, param1);
		return pstmt.executeQuery();
	}
	public ResultSet executeQuery(String sql, int param1, String param2) throws SQLException {
		PreparedStatement pstmt = conn.prepareStatement(sql);
		pstmt.setInt(1, param1);
		pstmt.setString(2, param2);
		return pstmt.executeQuery();
	}
	public ResultSet executeQuery(String sql, String param1, int param2) throws SQLException {
		PreparedStatement pstmt = conn.prepareStatement(sql);
		pstmt.setString(1, param1);
		pstmt.setInt(2, param2);
		return pstmt.executeQuery();
	}
	public ResultSet executeQuery(String sql, int param1, int param2) throws SQLException {
		PreparedStatement pstmt = conn.prepareStatement(sql);
		pstmt.setInt(1, param1);
		pstmt.setInt(2, param2);
		return pstmt.executeQuery();
	}
	public ResultSet executeQuery(String sql, int param1, int param2, String param3) throws SQLException {
		PreparedStatement pstmt = conn.prepareStatement(sql);
		pstmt.setInt(1, param1);
		pstmt.setInt(2, param2);
		pstmt.setString(3, param3);
		return pstmt.executeQuery();
	}

	public ResultSet queryParameters(int param1, String param2) throws SQLException {
		PreparedStatement pstmt = conn.prepareStatement("select id,num,text from test where num>? and text=?");
		pstmt.setInt(1, param1);
		pstmt.setString(2, param2);
		return pstmt.executeQuery();
	}
	public ResultSet queryParameters(int param1) throws SQLException {
		PreparedStatement pstmt = conn.prepareStatement("select id,num,text from test where num>?");
		pstmt.setInt(1, param1);
		return pstmt.executeQuery();
	}
	public ResultSet queryParameters(String param1) throws SQLException {
		PreparedStatement pstmt = conn.prepareStatement("select id,num,text from test where text=?");
		pstmt.setString(1, param1);
		return pstmt.executeQuery();
	}

	public ResultSet executeQuery(String sql, java.sql.Date param1) throws SQLException {
		PreparedStatement pstmt = conn.prepareStatement(sql);
		pstmt.setDate(1, param1);
		return pstmt.executeQuery();
	}
	public ResultSet executeQuery(String sql, boolean param1) throws SQLException {
		PreparedStatement pstmt = conn.prepareStatement(sql);
		pstmt.setBoolean(1, param1);
		return pstmt.executeQuery();
	}
	
	// Generic methods for any query and execution method (used in TestExecute)
	
	public void executeUpdate(String sql, int param1, boolean parametrized) throws SQLException {
		if (parametrized) {
			PreparedStatement pstmt = conn.prepareStatement(sql);
			pstmt.setInt(1, param1);
			pstmt.executeUpdate();
		} else {
			sql = sql.replaceAll("\\?", Integer.toString(param1));
			Statement stmt = conn.createStatement();
			stmt.executeUpdate(sql);
		}
	}
	public ResultSet executeGeneric(String sql, int param1, boolean parametrized, boolean isSelect)
			throws SQLException {
		if (parametrized) {
			PreparedStatement pstmt = conn.prepareStatement(sql);
			pstmt.setInt(1, param1);
			pstmt.execute();
			return isSelect ? pstmt.getResultSet() : null;
		} else {
			sql = sql.replaceAll("\\?", Integer.toString(param1));
			Statement stmt = conn.createStatement();
			stmt.execute(sql);
			return isSelect ? stmt.getResultSet() : null;
		}
	}
	
	// To compare results of queries with parameters (different in .NET)
    public static String ruleParamsToAssert(String sql, int paramCount) { //NOSONAR
        return sql;
    }
    public static String jdbcParamsToAssert(String sql, int paramCount) { //NOSONAR
        return sql;
    }
    
	// To check the stacktrace, this method is ignore by configuration, the interaction point must be in the child class
	public StackLocator myGetStackTraceIgnoredMethod() {
		return new StackLocator();
	}
	
	// To test mutations (java only)
	
	public Connection getConnectionNative() throws SQLException {
		return cf.getNativeConnection();
	}
	public ResultSet executeQueryMut(String sql, String param1) throws SQLException {
		PreparedStatement pstmt = conn.prepareStatement(sql);
		pstmt.setString(1, param1);
		return pstmt.executeQuery();
	}

}
