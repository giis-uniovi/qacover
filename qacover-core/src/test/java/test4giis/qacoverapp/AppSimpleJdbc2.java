package test4giis.qacoverapp;
import java.sql.*;

import giis.qacover.model.Variability;

/**
 * Simple mock application only to test evaluation in different queries
 */
public class AppSimpleJdbc2 extends AppBase {
	public AppSimpleJdbc2(Variability targetVariant) throws SQLException {
		super(targetVariant);
	}
	public ResultSet queryNoParameters1Condition(int param1) throws SQLException {
		return executeQuery("select id,num,text from test where num>=" + param1);
	}
	public ResultSet queryNoParameters2Condition(int param1, String param2) throws SQLException {
		return executeQuery("select id,num,text from test where num>" + param1 + " and text='" + param2 + "'");
	}

}
