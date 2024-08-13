package test4giis.qacoverapp;

import java.sql.ResultSet;
import java.sql.SQLException;

import giis.qacover.core.services.StackLocator;
import giis.qacover.model.Variability;

/**
 * Simple mock application
 */
public class AppSimpleJdbc extends AppBase {

	public AppSimpleJdbc(Variability targetVariant) throws SQLException {
		super(targetVariant);
	}
	//para pruebas de stacktrace
	public StackLocator myGetStackTraceTargetMethod() {
		return myGetStackTraceIgnoredMethod();
	}

	public ResultSet queryNoParameters1Condition(int param1) throws SQLException {
		return executeQuery("select id,num,text from test where num>=" + param1);
	}
	public ResultSet queryNoParameters2Condition(int param1, String param2) throws SQLException {
		return executeQuery("select id,num,text from test where num>" + param1 + " and text='" + param2 + "'");
	}
	public ResultSet queryParameters(int param1, String param2) throws SQLException {
		return executeQuery("select id,num,text from test where num>? and text=?",param1, param2);
	}
	
	//multiples queries en un metodo (misma y distinta linea)
	public void queryDifferentSingleLine(boolean run1, int param1, boolean run2, String param2) throws SQLException {
		//dos queries en misma linea con distintas reglas
		if (run1) {rs=executeQuery("select * from test where num=" + param1); rs.close();} if (run2) {rs=executeQuery("select * from test where text=" + param2); rs.close();}
	}
	public void queryEqualSingleLine(String param1, String param2) throws SQLException {
		rs=executeQuery("select * from test where text=" + param1); rs.close(); executeQuery("select * from test where text=" + param2);
	}
	public void queryEqualDifferentLine(String param1, String param2) throws SQLException {
		rs=executeQuery("select * from test where text=" + param1); 
		rs.close();
		//la misma en otra linea
		rs=executeQuery("select * from test where text=" + param2);
	}
	public ResultSet queryNoConditions() throws SQLException {
		return executeQuery("select id,num,text from test");
	}
	public ResultSet queryNoParametersQuotes(String param1, boolean bracketAtTable, boolean bracketAtColumn) throws SQLException {
		return executeQuery("select id,num,text from " + quoteIdentifier("test", bracketAtTable)
				+ " where " + quoteIdentifier("text", bracketAtColumn) + "='" + param1 + "'");
	}
	public ResultSet queryNoParametersBrackets(String param1, boolean bracketAtTable, boolean bracketAtColumn) throws SQLException {
		return executeQuery("select id,num,text from " + (bracketAtTable?"[test]":"test") 
				+ " where " + (bracketAtColumn?"[text]":"text") + "='" + param1 + "'");
	}
	private String quoteIdentifier(String name, boolean doQuote) {
		if (doQuote) //oracle ademas siempre pasa todo a mayusculas, luego debe estar mayusculas al poner quotes
			return "\"" + (variant.isOracle() || variant.isH2() ? name.toUpperCase() : name) + "\"";
		else
			return name;
	}

	public ResultSet queryParametersNamed(int param1, int param2, String param3) throws SQLException {
		return executeQuery("/* params=?1?,?1?,?2? */ select id,num,text from test where id=? or num=? or text=?",param1, param2, param3);
	}
	
	public ResultSet queryMutParameters(String param1) throws SQLException {
		return executeQueryMut("select id,txt from test where txt=?", param1);
		//PreparedStatement pstmt = conn.prepareStatement("select id,txt from test where txt=?");
		//pstmt.setString(1, param1);
		//return pstmt.executeQuery();
	}

}
