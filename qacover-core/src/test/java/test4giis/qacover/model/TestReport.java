package test4giis.qacover.model;

import java.lang.StringBuilder;
import java.sql.*;

import org.junit.After;
import org.junit.Before;
import org.junit.Test;

import giis.portable.util.FileUtil;
import giis.portable.util.JavaCs;
import giis.portable.util.Parameters;
import giis.qacover.core.QueryStatement;
import giis.qacover.core.services.FaultInjector;
import giis.qacover.core.services.Configuration;
import giis.qacover.core.services.StoreService;
import giis.qacover.model.SchemaModel;
import giis.qacover.model.Variability;
import giis.qacover.reader.CoverageCollection;
import giis.qacover.reader.CoverageReader;
import giis.qacover.reader.QueryCollection;
import giis.qacover.report.ReportManager;
import giis.tdrules.model.io.TdRulesXmlSerializer;
import giis.tdrules.model.io.TdSchemaXmlSerializer;
import giis.visualassert.Framework;
import giis.visualassert.SoftVisualAssert;
import test4giis.qacover.Base;
import test4giis.qacoverapp.AppSimpleJdbc;
import test4giis.qacoverapp.AppSimpleJdbc2;
import test4giis.qacoverapp.AppSimpleJdbc3Errors;

/**
 * Report generation:
 * Configures QACover to send rules and reports under a different folder (qacover-report)
 * Uses the thest included in the sample (qacoversample) as the application under test
 * Tests the readers in a more integrated way
 */
public class TestReport extends Base {
	private String rulesPath = FileUtil.getPath(Parameters.getProjectRoot(), Parameters.getReportSubdir(), "qacover-report", "rules");
	private String outPath = FileUtil.getPath(Parameters.getProjectRoot(), Parameters.getReportSubdir(), "qacover-report", "reports");
	// Each platform (java/net) has its own set of expected values for the reports
	private String bmkPath = new Variability().isJava()
			? FileUtil.getPath(Parameters.getProjectRoot(), "src", "test", "resources", "qacover-report")
			: FileUtil.getPath(Parameters.getProjectRoot(), "resources", "qacover-report");
	private String reportAppPackage = new Variability().isJava() ? "test4giis.qacoverapp." : "Test4giis.Qacoverapp.";
	private SoftVisualAssert va;

	@Before
	public void setUp() throws SQLException {
		super.setUp();
		va = new SoftVisualAssert().setFramework(Framework.JUNIT4).setCallStackLength(3).setBrightColors(true);
	}

	@After
	public void tearDown() throws SQLException {
		va.assertAll("diff-aggregated-" + testName.getMethodName() + ".html");
		super.tearDown();
	}

	// Initial setup for the tests: infer parameters, no boundaries
	private void reset() {
		reset(true, false);
	}
	private void reset(boolean inferQueryParameters, boolean boundaries) {
		options.reset();
		options.setInferQueryParameters(inferQueryParameters);
		if (!boundaries)
			options.setFpcServiceOptions("noboundaries");
		QueryStatement.setFaultInjector(null);
		options.setStoreRulesLocation(rulesPath);
		options.setStoreReportsLocation(outPath);
	}

	@Test
	public void testReports() throws SQLException {
		// Runs the application with partial comparison (readers and reports) at each
		reset();
		new StoreService(options).dropRules().dropLast(); // clean start
		runReports1OfTestStore();
		assertReaderByClass(0);
		new ReportManager().run(rulesPath, outPath);
		assertReports(reportAppPackage, "AppSimpleJdbc.html");

		reset();
		runReports2OfTestEvaluation();
		assertReaderByClass(1);
		new ReportManager().run(rulesPath, outPath);
		assertReports(reportAppPackage, "AppSimpleJdbc2.html");

		// Excludes Jdk 1.4 because uses H2 with different message errors
		if (!new Variability().isJava4()) {
			reset();
			runReports3OfTestError();
			assertReaderByClass(2);
			new ReportManager().run(rulesPath, outPath);
			assertReports(reportAppPackage, "AppSimpleJdbc3Errors.html");
		}

		assertReaderByClassAll();

		assertReaderByRunOrderAll();
		
		// Repeats the evaluation and report generation with a full check
		reset();
		new ReportManager().run(Configuration.getInstance().getStoreRulesLocation(), Configuration.getInstance().getStoreReportsLocation());
		String indexContent = FileUtil.fileRead(outPath, "index.html");
		FileUtil.fileWrite(outPath, "index.html", indexContent);
		assertReports("", "index.html");
		assertReports(reportAppPackage, "AppSimpleJdbc.html");
		assertReports(reportAppPackage, "AppSimpleJdbc2.html");
		assertReports(reportAppPackage, "AppSimpleJdbc3Errors.html");
	}

	private void runReports1OfTestStore() throws SQLException {
		AppSimpleJdbc app = new AppSimpleJdbc(variant);
		app.executeUpdateNative(new String[] { "drop table if exists test",
				"create table test(id int not null, num int not null, text varchar(16))",
				"insert into test(id,num,text) values(1,99,'xyz')" });
		// These queries are parametrized, convert before into non parametrized to then infer
		rs = app.queryNoParameters2Condition(90, "nnn");
		rs.close();
		rs = app.queryNoParameters2Condition(90, "nnn");
		rs.close();
		rs = app.queryNoParameters2Condition(90, "xyz");
		rs.close();

		rs = app.queryNoParameters1Condition(99);
		rs.close();
		app.queryDifferentSingleLine(true, 99, false, "");
		rs.close();
		app.queryDifferentSingleLine(true, 99, true, "'xyz'");
		app.queryEqualDifferentLine("'xyz'", "'aaa'");
		Configuration.getInstance().setInferQueryParameters(true);
		app.queryEqualDifferentLine("'xyz'", "'aaa'");
		rs.close();
		app.close();
	}

	private void runReports2OfTestEvaluation() throws SQLException {
		AppSimpleJdbc2 app = new AppSimpleJdbc2(variant);
		app.executeUpdateNative(new String[] { "drop table if exists test",
				"create table test(id int not null, num int not null, text varchar(16))",
				"insert into test(id,num,text) values(1,0,'abc')", "insert into test(id,num,text) values(2,99,'xyz')",
				"insert into test(id,num,text) values(3,99,null)" });
		rs = app.queryNoParameters1Condition(-1);
		rs.close();
		rs = app.queryNoParameters2Condition(98, "abc");
		rs.close();
		rs = app.queryNoParameters1Condition(-1);
		rs.close();
		rs = app.queryNoParameters1Condition(-1);
		rs.close();
		app.close();
	}

	private void runReports3OfTestError() throws SQLException {
		AppSimpleJdbc3Errors app = new AppSimpleJdbc3Errors(variant);
		app.executeUpdateNative(new String[] { "drop table if exists test",
				"create table test(id int not null, num int not null, text varchar(16))",
				"insert into test(id,num,text) values(2,99,'xyz')", });
		reset(false, false);
		// first does not cause error, to check combination of no errors and errors
		rs = app.query0Errors();
		rs.close();
		// error in query
		reset(false, false);
		options.setFpcServiceUrl("http://giis.uniovi.es/noexiste.xml");
		rs = app.query1ErrorAtQuery();
		rs.close();
		reset(false, true);
		// error in rule execution
		QueryStatement.setFaultInjector(
				new FaultInjector().setSingleRuleFaulty("selectar id,num,text from test where num < 9"));
		rs = app.query1ErrorAtRule();
		rs.close();

		// multiple errors in same query
		QueryStatement.setFaultInjector(
				new FaultInjector().setSingleRuleFaulty("selectar id,num,text from test where num < 9"));
		rs = app.queryMultipleErrors();
		rs.close();
		QueryStatement.setFaultInjector(
				new FaultInjector().setSingleRuleFaulty("selectar id,num,text from test where num < 9"));
		rs = app.queryMultipleErrors();
		rs.close();
		QueryStatement.setFaultInjector(
				new FaultInjector().setSingleRuleFaulty("select id,num,text from notable where num < 9"));
		rs = app.queryMultipleErrors();
		rs.close();

		app.close();
	}
	
	// Exptected values from the CoverageReaders
	private String[] expectedCols = new String[] {
			"QueryCollection: test4giis.qacoverapp.AppSimpleJdbc qcount=6,qerror=0,count=17,dead=7,error=0\n" + 
			"  queryDifferentSingleLine\n" + 
			"  queryDifferentSingleLine\n" + 
			"  queryEqualDifferentLine\n" + 
			"  queryEqualDifferentLine\n" + 
			"  queryNoParameters1Condition\n" + 
			"  queryNoParameters2Condition",
			"QueryCollection: test4giis.qacoverapp.AppSimpleJdbc2 qcount=2,qerror=0,count=6,dead=4,error=0\n" + 
			"  queryNoParameters1Condition\n" + 
			"  queryNoParameters2Condition",
			"QueryCollection: test4giis.qacoverapp.AppSimpleJdbc3Errors qcount=4,qerror=1,count=8,dead=1,error=2\n" + 
			"  query0Errors\n" + 
			"  query1ErrorAtQuery\n" + 
			"  query1ErrorAtRule\n" + 
			"  queryMultipleErrors"
			};
	private String expectedAllSummary = "CoverageCollection: qcount=12,qerror=1,count=31,dead=12,error=2";
	
	private void assertReaderByClass(int expectedIndex) {
		CoverageCollection ccol = new CoverageReader(Configuration.getInstance().getStoreRulesLocation()).getByClass();
		QueryCollection qcol = ccol.get(expectedIndex);
		va.assertEquals(expectedCols[expectedIndex].toLowerCase(),
				qcol.toString(true, false, false).toLowerCase(),
				"at assertReaderByClass(" + expectedIndex + ")"); // lowercase for netcore compatibility
	}
	private void assertReaderByClassAll() {
		CoverageCollection ccol = new CoverageReader(Configuration.getInstance().getStoreRulesLocation()).getByClass();
		va.assertEquals((expectedAllSummary + "\n" + expectedCols[0] + "\n" + expectedCols[1] + "\n" + expectedCols[2])
				.toLowerCase(), 
				ccol.toString(true, false, false).toLowerCase(), 
				"at assertReaderByClassAll()"); 
	}

	// first checks all keys and then the data from CoverageReader
	private void assertReaderByRunOrderAll() {
		StringBuilder allKeys = new StringBuilder();
		StringBuilder allData = new StringBuilder();
		CoverageReader cr = new CoverageReader(Configuration.getInstance().getStoreRulesLocation());
		QueryCollection cc = cr.getByRunOrder();
		for (int i = 0; i < cc.size(); i++) {
			allKeys.append(cc.get(i).getKey() + "\n");
			allData.append("*** " + cc.get(i).getKey() + "\n");
			allData.append("sql: " + cc.get(i).getModel().getSql() + "\n");
			allData.append("parameters: " + cc.get(i).getParametersXml() + "\n");
			for (int j = 0; j < cc.get(i).getModel().getRules().size(); j++)
				allData.append("rule" + j + ": " + new TdRulesXmlSerializer().serialize(cc.get(i).getModel().getRules().get(j).getModel(), "fpcrule").trim() + "\n");
			if (!"".equals(cc.get(i).getModel().getErrorString()) || cc.get(i).getModel().getRules().size() > 0) {
				SchemaModel schema = cc.get(i).getSchema();
				allData.append("schema: " + new TdSchemaXmlSerializer().serialize(schema.getModel()) + "\n");
			}
		}
		FileUtil.fileWrite(outPath, "all-keys-by-run-order.txt", allKeys.toString());
		FileUtil.fileWrite(outPath, "all-data-by-run-order.txt", allData.toString());

		String actualKeys = FileUtil.fileRead(outPath, "all-keys-by-run-order.txt");
		String actualData = FileUtil.fileRead(outPath, "all-data-by-run-order.txt");
		String expectedKeys = FileUtil.fileRead(bmkPath, "all-keys-by-run-order.txt");
		String expectedData = FileUtil.fileRead(bmkPath, "all-data-by-run-order.txt");
		if (new Variability().isNetCore()) {
			expectedKeys = replaceExpectedStrings(expectedKeys);
			expectedData = replaceExpectedStrings(expectedData);
			//expectedData = expectedData.replace("\"INT\"", "\"int\"");
			//expectedData = expectedData.replace("\"VARCHAR\"", "\"varchar\"");
		}
		// line numbers as ### to allow comparison
		expectedKeys = JavaCs.replaceRegex(expectedKeys, "\\.\\d+\\.", ".###.");
		expectedData = JavaCs.replaceRegex(expectedData, "\\.\\d+\\.", ".###.");
		actualKeys = JavaCs.replaceRegex(actualKeys, "\\.\\d+\\.", ".###.");
		actualData = JavaCs.replaceRegex(actualData, "\\.\\d+\\.", ".###.");
		va.assertEquals(expectedKeys.replace("\r", ""), actualKeys.replace("\r", ""), "at assertReaderByRunOrderAll(), check expectedKeys");
		va.assertEquals(expectedData.replace("\r", ""), actualData.replace("\r", ""), "at assertReaderByRunOrderAll(), check expectedData");
	}

	// Html of generated reports
	private void assertReports(String packageName, String className) {
		String expected = FileUtil.fileRead(bmkPath, packageName.toLowerCase() + className);
		String actual = FileUtil.fileRead(outPath, packageName + className);
		if (new Variability().isNetCore())
			expected = replaceExpectedStrings(expected);
		expected = reprocessReportForCompare(expected);
		actual = reprocessReportForCompare(actual);
		va.assertEquals(expected, actual, "at className: " + className);
	}
	public static String reprocessReportForCompare(String content) {
		content = JavaCs.replaceRegex(content, ":\\d+<", ":###<");
		content = JavaCs.replaceRegex(content, "\\d{4}-\\d{2}-\\d{2}T\\d{2}:\\d{2}:\\d{2}\\.\\d{3}", "yyyy-MM-ddThh:mm:ss.SSS");
		content = JavaCs.replaceRegex(content, "\\[version .*\\]", "[version x.y.z]");
		content = JavaCs.replaceRegex(content, "Error at Get query table names: ApiException.*<\\/span>", "Error at Get query table names: ApiException<\\/span>");
		return content.replace("\r", "");
	}
	private String replaceExpectedStrings(String expected) {
		expected = expected.replace("test4giis.qacoverapp", "Test4in2test.Qacoverapp");
		expected = expected.replace("queryDifferentSingleLine", "QueryDifferentSingleLine");
		expected = expected.replace("queryEqualDifferentLine", "QueryEqualDifferentLine");
		expected = expected.replace("queryNoParameters1Condition", "QueryNoParameters1Condition");
		expected = expected.replace("queryNoParameters2Condition", "QueryNoParameters2Condition");
		expected = expected.replace("query0Errors", "Query0Errors");
		expected = expected.replace("query1ErrorAtQuery", "Query1ErrorAtQuery");
		expected = expected.replace("query1ErrorAtRule", "Query1ErrorAtRule");
		expected = expected.replace("queryMultipleErrors", "QueryMultipleErrors");
		expected = expected.replace("java.lang.RuntimeException", "System.Exception");
		expected = expected.replace("org.sqlite.SQLiteException", "Microsoft.Data.Sqlite.SqliteException");
		expected = expected.replace("giis.qacover.portable", "Giis.Qacover.Portable");
		expected = expected.replace("HTTP/1.1 404 Not Found", "NotFound");
		expected = expected.replace("[SQLITE_ERROR] SQL error or missing database (near \"selectar\": syntax error)", "SQLite Error 1: 'near \"selectar\": syntax error'.");
		expected = expected.replace("[SQLITE_ERROR] SQL error or missing database (no such table: notable)", "SQLite Error 1: 'no such table: notable'.");
		expected = expected.replace("Giis.Tdrules.Openapi.Invoker.ApiException", "Giis.Tdrules.Openapi.Client.ApiException");
		return expected;
	}
	
}
