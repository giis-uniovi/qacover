package test4giis.qacover;

import java.sql.*;

import org.junit.After;
import org.junit.Before;
import org.junit.Test;

import giis.qacover.core.services.Configuration;
import giis.qacover.model.Variability;
import test4giis.qacoverapp.AppSimpleJdbc;

/**
 * Handling of data types that are DBMS specific and must be configured in p6spy.properties 
 */
public class TestDataTypeFormats extends Base {
	protected AppSimpleJdbc app;

	@Before
	public void setUp() throws SQLException {
		super.setUp();
		app = new AppSimpleJdbc(variant);
		// usa full name for easier conversion to C#
		com.p6spy.engine.spy.P6SpyOptions.getActiveInstance().setDatabaseDialectBooleanFormat("numeric");
		com.p6spy.engine.spy.P6SpyOptions.getActiveInstance().setDatabaseDialectDateFormat("yyyy-MM-dd");
		setUpTestData();
		Configuration.getInstance().setFpcServiceOptions("noboundaries");
	}

	@After
	public void tearDown() throws SQLException {
		super.tearDown();
		app.close();
	}

	// centralize the setup for all variants
	public void setUpTestData() {
		String date1 = "'2020-02-01T00:00:00.000+0100'";
		String date2 = "'2020-01-02T00:00:00.000+0100'";
		if (variant.isSqlServer() || variant.isH2()) {
			date1 = "'2020-02-01'";
			date2 = "'2020-01-02'";
		} else if (variant.isOracle()) {
			date1 = "DATE '2020-02-01'";
			date2 = "DATE '2020-01-02'";
		}
		String bitType = "number(1)";
		if (variant.isSqlServer())
			bitType = "bit";
		else if (variant.isPostgres() || variant.isH2())
			bitType = "boolean";
		app.dropTable("dbmstest");
		app.executeUpdateNative(new String[] { "create table dbmstest(id int not null, bool " + bitType + ", dat date)",
				"insert into dbmstest(id,bool,dat) values(1," + boolValue(false) + "," + date1 + ")",
				"insert into dbmstest(id,bool,dat) values(2," + boolValue(true) + "," + date2 + ")", });
	}

	protected String boolValue(boolean value) {
		return value ? "1" : "0";
	}

	@Test
	public void testEvalBooleanParameters() throws SQLException {
		Configuration.getInstance().setInferQueryParameters(false);
		String expOuput = "2 1";
		if (variant.isNetCore() && variant.isSqlServer()) //
			expOuput = "2 True";
		else if (variant.isPostgres()) // jdbc does not return true!!!, only t
			expOuput = "2 t";
		else if (variant.isH2())
			expOuput = "2 TRUE";

		rs = app.executeQuery("select id,bool from dbmstest where bool=?", true);
		assertEvalResults("select id,bool from dbmstest where bool=?", expOuput, SqlUtil.resultSet2csv(rs, " "),
				"COVERED   SELECT id , bool FROM dbmstest WHERE NOT(bool = " + boolValue(true).toString() + ")\n"
				+ "COVERED   SELECT id , bool FROM dbmstest WHERE (bool = " + boolValue(true).toString() + ")\n"
				+ "UNCOVERED SELECT id , bool FROM dbmstest WHERE (bool IS NULL)",
				"{?1?=" + boolValue(true).toString() + "}", false, new Variability().isNetCore());
	}

	@Test
	public void testEvalDateParameters() throws SQLException {
		Configuration.getInstance().setInferQueryParameters(false);
		// column to check the results
		String dateColumn = "dat";
		if (variant.isSqlite())
			dateColumn = "SUBSTR(dat , 1 , 10)";
		if (variant.isSqlServer())
			dateColumn = "CONVERT(varchar , dat , 23)";
		// Each SGBD has different precisions in time
		String expOuput = "2 2020-01-02";
		if (variant.isOracle())
			expOuput = "2 2020-01-02 00:00:00";
		else if (variant.isJava() && variant.isSqlite()) // el driver de sqlite en java no muestra salida correcta
			expOuput = "";
		boolean convertNetParameters = false;
		if (variant.isNetCore())
			convertNetParameters = true;

		rs = app.executeQuery("select id," + dateColumn + " from dbmstest where dat<?",
				java.sql.Date.valueOf("2020-01-31"));
		assertEvalResults("select id," + dateColumn + " from dbmstest where dat<?", expOuput,
				SqlUtil.resultSet2csv(rs, " "),
				"COVERED   SELECT id , " + dateColumn + " FROM dbmstest WHERE NOT(dat < '2020-01-31')\n"
				+ "COVERED   SELECT id , " + dateColumn + " FROM dbmstest WHERE (dat < '2020-01-31')\n"
				+ "UNCOVERED SELECT id , " + dateColumn + " FROM dbmstest WHERE (dat IS NULL)",
				"{?1?='2020-01-31'}", false, convertNetParameters);
	}
}
