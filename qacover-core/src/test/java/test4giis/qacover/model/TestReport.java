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
import giis.qacover.model.QueryModel;
import giis.qacover.model.SchemaModel;
import giis.qacover.model.Variability;
import giis.qacover.reader.CoverageCollection;
import giis.qacover.reader.CoverageReader;
import giis.qacover.reader.HistoryReader;
import giis.qacover.reader.QueryReader;
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
 * As the application to be reported, uses three classes that include the
 * most relevant situations that have been unit tested before,
 * but here, the goal is to check everything in an integrated way.
 * Configures QACover to send rules and reports under a different folder (qacover-report).
 * 
 * NOTE: when changing the report format, in addition to the htmls of 
 * qacover-core/src/test/resources/qacover-report, the following must be updted too:
 * - dotnet reports at net/resources/qacover-report
 * - IT reports at qacover-core/src/test/resources/qacover-uber-main 
 * - IT reports at qacover-core/src/test/resources/spring-petclinic-main
 */
public class TestReport extends Base {
	// All comparisons are made over expected and actual files
	private String rulesPath = FileUtil.getPath(Parameters.getProjectRoot(), Parameters.getReportSubdir(), "qacover-report", "rules");
	private String outPath = FileUtil.getPath(Parameters.getProjectRoot(), Parameters.getReportSubdir(), "qacover-report", "reports");
	// Each platform (java/net) has its own set of expected values for the reports
	private String bmkPath = new Variability().isJava()
			? FileUtil.getPath(Parameters.getProjectRoot(), "src", "test", "resources", "qacover-report")
			: FileUtil.getPath(Parameters.getProjectRoot(), "resources", "qacover-report");
	private String reportAppPackage = new Variability().isJava() ? "test4giis.qacoverapp." : "Test4giis.Qacoverapp.";
	private SoftVisualAssert va;

	@Before
	@Override
	public void setUp() throws SQLException {
		super.setUp();
		va = new SoftVisualAssert().setFramework(Framework.JUNIT4).setCallStackLength(3).setBrightColors(true);
	}

	@After
	@Override
	public void tearDown() throws SQLException {
		va.assertAll("diff-IntegratedTestReaderAndReport.html");
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
			options.setRuleOptions("noboundaries");
		QueryStatement.setFaultInjector(null);
		options.setStoreRulesLocation(rulesPath);
		options.setStoreReportsLocation(outPath);
	}

	@Test
	public void testReports() throws SQLException {
		reset();
		new StoreService(options).dropRules().dropLast(); // clean start
		FileUtil.createDirectory(outPath); // this is ensured by reporter, but we need it before to check readers
		
		// Runs the application mock classes to generate the rules
		runReports1OfTestStore();
		runReports2OfTestEvaluation();
		runReports3OfTestError();

		// Before the report, checks the results of the readers
		assertReaderByClassAll();
		assertReaderKeysByRunOrderAll();
		assertReaderDataByRunOrderAll();
		
		// Generate and test the html report
		// note that net runs four levels before solution root
		new ReportManager().run(Configuration.getInstance().getStoreRulesLocation(), 
				Configuration.getInstance().getStoreReportsLocation(),
				new Variability().isJava() ? "src/test/java" : "../../../..", 
				new Variability().isJava() ? "" : "../../../..");
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
		options.setRuleServiceUrl("http://giis.uniovi.es/noexiste.xml");
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
	
	private void assertReaderByClassAll() {
		CoverageCollection ccol = new CoverageReader(Configuration.getInstance().getStoreRulesLocation()).getByClass();
		String expected = ccol.toString(true, false, false);
		FileUtil.fileWrite(outPath, "all-by-class.txt", ccol.toString(true, false, false));
		
		String actual = FileUtil.fileRead(bmkPath, "all-by-class.txt");
		va.assertEquals(expected.replace("\r", ""), actual.replace("\r", ""), "at assertReaderByClassAll()"); 
	}

	private void assertReaderKeysByRunOrderAll() {
		StringBuilder allKeys = new StringBuilder();
		CoverageReader cr = new CoverageReader(Configuration.getInstance().getStoreRulesLocation());
		HistoryReader cc = cr.getHistory();
		for (int i = 0; i < cc.getItems().size(); i++)
			allKeys.append(cc.getItems().get(i).getKey() + "|" + cc.getItems().get(i).getParamsXml() + "\n");
		FileUtil.fileWrite(outPath, "all-keys-by-run-order.txt", allKeys.toString());
		
		String actualKeys = FileUtil.fileRead(outPath, "all-keys-by-run-order.txt");
		String expectedKeys = FileUtil.fileRead(bmkPath, "all-keys-by-run-order.txt");
		va.assertEquals(expectedKeys.replace("\r", ""), actualKeys.replace("\r", ""), "at assertReaderByRunOrderAll(), check expectedKeys");
	}
	
	private void assertReaderDataByRunOrderAll() {
		StringBuilder allData = new StringBuilder();
		String rulesFolder = Configuration.getInstance().getStoreRulesLocation();
		CoverageReader cr = new CoverageReader(rulesFolder);
		HistoryReader cc = cr.getHistory();
		for (int i = 0; i < cc.getItems().size(); i++) {
			allData.append("*** " + cc.getItems().get(i).getKey() + "\n");
			// The history reader is independent from the models (the only join is the key),
			// we use a standalone query reader to get the model
			QueryReader qr = new QueryReader(rulesFolder, cc.getItems().get(i).getKey());
			QueryModel model = qr.getModel();
			allData.append("sql: " + model.getSql() + "\n");
			allData.append("parameters: " + cc.getItems().get(i).getParamsXml() + "\n");
			for (int j = 0; j < model.getRules().size(); j++)
				allData.append("rule" + j + ": " + new TdRulesXmlSerializer().serialize(model.getRules().get(j).getModel(), "fpcrule").trim() + "\n");
			if (!"".equals(model.getErrorString()) || model.getRules().size() > 0) {
				SchemaModel schema = qr.getSchema();
				allData.append("schema: " + new TdSchemaXmlSerializer().serialize(schema.getModel()) + "\n");
			}
		}
		FileUtil.fileWrite(outPath, "all-data-by-run-order.txt", allData.toString());

		String actualData = FileUtil.fileRead(outPath, "all-data-by-run-order.txt");
		String expectedData = FileUtil.fileRead(bmkPath, "all-data-by-run-order.txt");
		va.assertEquals(expectedData.replace("\r", ""), actualData.replace("\r", ""), "at assertReaderByRunOrderAll(), check expectedData");
	}

	// Html of generated reports
	private void assertReports(String packageName, String className) {
		String expected = FileUtil.fileRead(bmkPath, packageName.toLowerCase() + className);
		String actual = FileUtil.fileRead(outPath, packageName + className);
		
		expected = reprocessReportForCompare(expected);
		actual = reprocessReportForCompare(actual);
		va.assertEquals(expected, actual, "at className: " + className);
	}
	
	public static String reprocessReportForCompare(String content) {
		// to compare using a fixed date and version number
		content = JavaCs.replaceRegex(content, "\\d{4}-\\d{2}-\\d{2}T\\d{2}:\\d{2}:\\d{2}\\.\\d{3}", "yyyy-MM-ddThh:mm:ss.SSS");
		content = JavaCs.replaceRegex(content, "\\[version .*\\]", "[version x.y.z]");
		// error at query gives different content in local and CI, remove the variable part
		content = JavaCs.replaceRegex(content, 
				"Query error: Error at Get query table names: .*<\\/span>", 
				"Query error: Error at Get query table names: (rest of message removed)<\\/span>");
		return content.replace("\r", "");
	}
	
}
