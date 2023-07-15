package test4giis.qacover;

import java.lang.StringBuilder;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.ArrayList;
import java.util.List;

import test4giis.qacoverapp.AppSimpleJdbc;

/**
 * Utility to validate the handling of different datatypes in a SGBD:
 * Stores a list different types, and values to create 
 * statements that check these data types can be created and viewed
 */
public class DataTypeAssert extends Base {
	public List<String[]> columns = new ArrayList<String[]>();

	// Ads a column of a given datatype to generate an insertion 
	// of inValue and a select of outValue
	public void add(String type, String inValue, String outValue) {
		columns.add(new String[] { type, inValue, outValue });
	}

	public String getSqlCreate() {
		StringBuilder sb = new StringBuilder();
		sb.append("create table dbmstypes(");
		for (int i = 0; i < columns.size(); i++)
			sb.append((i == 0 ? "" : ", ") + "col" + i + " " + columns.get(i)[0]);
		sb.append(")");
		return sb.toString();
	}

	public String getSqlInsert(boolean firstSerial) {
		int first = firstSerial ? 1 : 0; // si el primero es serial se salta el valor a insert
		StringBuilder sb = new StringBuilder();
		sb.append("insert into dbmstypes(");
		for (int i = first; i < columns.size(); i++)
			sb.append((i == first ? "" : ",") + "col" + i);
		sb.append(") values (");
		for (int i = first; i < columns.size(); i++)
			sb.append((i == first ? "" : ",") + columns.get(i)[1]);
		sb.append(")");
		return sb.toString();
	}

	public String getSqlColumns() {
		StringBuilder sb = new StringBuilder();
		for (int i = 0; i < columns.size(); i++)
			sb.append((i == 0 ? "" : " , ") + "col" + i);
		return sb.toString();
	}

	public String getOutputs() {
		StringBuilder sb = new StringBuilder();
		for (int i = 0; i < columns.size(); i++)
			sb.append((i == 0 ? "" : " ") + columns.get(i)[2]);
		return sb.toString();
	}

	// Main assertion: create table, execute insertion and then select with the configured types and values
	public void assertAll(boolean firstSerial, AppSimpleJdbc app, ResultSet rs) throws SQLException {
		app.dropTable("dbmstypes");
		app.executeUpdateNative(new String[] { getSqlCreate(), getSqlInsert(firstSerial) });
		String sql = "select " + getSqlColumns() + " from dbmstypes where col0<10";
		rs = app.executeQuery(sql);
		// Another assert to check that datatype are handled when evaluating rules
		assertEvalResults(sql, getOutputs(), SqlUtil.resultSet2csv(rs, " "),
				"UNCOVERED SELECT " + getSqlColumns() + " FROM dbmstypes WHERE NOT(col0 < 10)\n" + "COVERED   SELECT "
						+ getSqlColumns() + " FROM dbmstypes WHERE (col0 < 10)");
	}

}
