package test4giis.qacoverapp;
import java.sql.*;

import giis.qacover.model.Variability;

/**
 * Simple mock application to test errors
 */
public class AppSimpleJdbc3Errors extends AppBase {
	public AppSimpleJdbc3Errors(Variability targetVariant) throws SQLException {
		super(targetVariant);
	}
	public ResultSet query0Errors() throws SQLException {
		return executeQuery("select id,num,text from test where num<9");
	}
	public ResultSet query1ErrorAtQuery() throws SQLException {
		return executeQuery("select id,num,text from test where num<10");		
	}
	public ResultSet query1ErrorAtRule() throws SQLException {
		return executeQuery("select id,num,text from test where num < 11");		
	}
	public ResultSet queryMultipleErrors() throws SQLException {
		return executeQuery("select id,num,text from test where num<9");
	}

}
