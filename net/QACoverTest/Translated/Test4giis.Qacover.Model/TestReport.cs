/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using System.Text;
using Giis.Portable.Util;
using Giis.Qacover.Core;
using Giis.Qacover.Core.Services;
using Giis.Qacover.Model;
using Giis.Qacover.Reader;
using Giis.Qacover.Report;
using Giis.Tdrules.Model.IO;
using Giis.Visualassert;
using NUnit.Framework;

using Test4giis.Qacover;
using Test4giis.Qacoverapp;

namespace Test4giis.Qacover.Model
{
	/// <summary>
	/// Report generation:
	/// Configures QACover to send rules and reports under a different folder (qacover-report)
	/// Uses the thest included in the sample (qacoversample) as the application under test
	/// Tests the readers in a more integrated way
	/// </summary>
	public class TestReport : Base
	{
		private string rulesPath = FileUtil.GetPath(Parameters.GetProjectRoot(), Parameters.GetReportSubdir(), "qacover-report", "rules");

		private string outPath = FileUtil.GetPath(Parameters.GetProjectRoot(), Parameters.GetReportSubdir(), "qacover-report", "reports");

		private string bmkPath = new Variability().IsJava() ? FileUtil.GetPath(Parameters.GetProjectRoot(), "src", "test", "resources", "qacover-report") : FileUtil.GetPath(Parameters.GetProjectRoot(), "resources", "qacover-report");

		private string reportAppPackage = new Variability().IsJava() ? "test4giis.qacoverapp." : "Test4giis.Qacoverapp.";

		private SoftVisualAssert va;

		// Each platform (java/net) has its own set of expected values for the reports
		/// <exception cref="Java.Sql.SQLException"/>
		[NUnit.Framework.SetUp]
		public override void SetUp()
		{
			base.SetUp();
			va = new SoftVisualAssert().SetFramework(Giis.Visualassert.Framework.Junit4).SetCallStackLength(3).SetBrightColors(true);
		}

		/// <exception cref="Java.Sql.SQLException"/>
		[NUnit.Framework.TearDown]
		public override void TearDown()
		{
			va.AssertAll("diff-aggregated-" + NUnit.Framework.TestContext.CurrentContext.Test.Name  + ".html");
			base.TearDown();
		}

		// Initial setup for the tests: infer parameters, no boundaries
		private void Reset()
		{
			Reset(true, false);
		}

		private void Reset(bool inferQueryParameters, bool boundaries)
		{
			options.Reset();
			options.SetInferQueryParameters(inferQueryParameters);
			if (!boundaries)
			{
				options.SetFpcServiceOptions("noboundaries");
			}
			QueryStatement.SetFaultInjector(null);
			options.SetStoreRulesLocation(rulesPath);
			options.SetStoreReportsLocation(outPath);
		}

		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestReports()
		{
			// Runs the application with partial comparison (readers and reports) at each
			Reset();
			new StoreService(options).DropRules().DropLast();
			// clean start
			RunReports1OfTestStore();
			AssertReaderByClass(0);
			new ReportManager().Run(rulesPath, outPath);
			AssertReports(reportAppPackage, "AppSimpleJdbc.html");
			Reset();
			RunReports2OfTestEvaluation();
			AssertReaderByClass(1);
			new ReportManager().Run(rulesPath, outPath);
			AssertReports(reportAppPackage, "AppSimpleJdbc2.html");
			// Excludes Jdk 1.4 because uses H2 with different message errors
			if (!new Variability().IsJava4())
			{
				Reset();
				RunReports3OfTestError();
				AssertReaderByClass(2);
				new ReportManager().Run(rulesPath, outPath);
				AssertReports(reportAppPackage, "AppSimpleJdbc3Errors.html");
			}
			AssertReaderByClassAll();
			AssertReaderByRunOrderAll();
			// Repeats the evaluation and report generation with a full check
			Reset();
			new ReportManager().Run(Configuration.GetInstance().GetStoreRulesLocation(), Configuration.GetInstance().GetStoreReportsLocation());
			string indexContent = FileUtil.FileRead(outPath, "index.html");
			FileUtil.FileWrite(outPath, "index.html", indexContent);
			AssertReports(string.Empty, "index.html");
			AssertReports(reportAppPackage, "AppSimpleJdbc.html");
			AssertReports(reportAppPackage, "AppSimpleJdbc2.html");
			AssertReports(reportAppPackage, "AppSimpleJdbc3Errors.html");
		}

		/// <exception cref="Java.Sql.SQLException"/>
		private void RunReports1OfTestStore()
		{
			AppSimpleJdbc app = new AppSimpleJdbc(variant);
			app.ExecuteUpdateNative(new string[] { "drop table if exists test", "create table test(id int not null, num int not null, text varchar(16))", "insert into test(id,num,text) values(1,99,'xyz')" });
			// These queries are parametrized, convert before into non parametrized to then infer
			rs = app.QueryNoParameters2Condition(90, "nnn");
			rs.Close();
			rs = app.QueryNoParameters2Condition(90, "nnn");
			rs.Close();
			rs = app.QueryNoParameters2Condition(90, "xyz");
			rs.Close();
			rs = app.QueryNoParameters1Condition(99);
			rs.Close();
			app.QueryDifferentSingleLine(true, 99, false, string.Empty);
			rs.Close();
			app.QueryDifferentSingleLine(true, 99, true, "'xyz'");
			app.QueryEqualDifferentLine("'xyz'", "'aaa'");
			Configuration.GetInstance().SetInferQueryParameters(true);
			app.QueryEqualDifferentLine("'xyz'", "'aaa'");
			rs.Close();
			app.Close();
		}

		/// <exception cref="Java.Sql.SQLException"/>
		private void RunReports2OfTestEvaluation()
		{
			AppSimpleJdbc2 app = new AppSimpleJdbc2(variant);
			app.ExecuteUpdateNative(new string[] { "drop table if exists test", "create table test(id int not null, num int not null, text varchar(16))", "insert into test(id,num,text) values(1,0,'abc')", "insert into test(id,num,text) values(2,99,'xyz')", "insert into test(id,num,text) values(3,99,null)"
				 });
			rs = app.QueryNoParameters1Condition(-1);
			rs.Close();
			rs = app.QueryNoParameters2Condition(98, "abc");
			rs.Close();
			rs = app.QueryNoParameters1Condition(-1);
			rs.Close();
			rs = app.QueryNoParameters1Condition(-1);
			rs.Close();
			app.Close();
		}

		/// <exception cref="Java.Sql.SQLException"/>
		private void RunReports3OfTestError()
		{
			AppSimpleJdbc3Errors app = new AppSimpleJdbc3Errors(variant);
			app.ExecuteUpdateNative(new string[] { "drop table if exists test", "create table test(id int not null, num int not null, text varchar(16))", "insert into test(id,num,text) values(2,99,'xyz')" });
			Reset(false, false);
			// first does not cause error, to check combination of no errors and errors
			rs = app.Query0Errors();
			rs.Close();
			// error in query
			Reset(false, false);
			options.SetFpcServiceUrl("http://giis.uniovi.es/noexiste.xml");
			rs = app.Query1ErrorAtQuery();
			rs.Close();
			Reset(false, true);
			// error in rule execution
			QueryStatement.SetFaultInjector(new FaultInjector().SetSingleRuleFaulty("selectar id,num,text from test where num < 9"));
			rs = app.Query1ErrorAtRule();
			rs.Close();
			// multiple errors in same query
			QueryStatement.SetFaultInjector(new FaultInjector().SetSingleRuleFaulty("selectar id,num,text from test where num < 9"));
			rs = app.QueryMultipleErrors();
			rs.Close();
			QueryStatement.SetFaultInjector(new FaultInjector().SetSingleRuleFaulty("selectar id,num,text from test where num < 9"));
			rs = app.QueryMultipleErrors();
			rs.Close();
			QueryStatement.SetFaultInjector(new FaultInjector().SetSingleRuleFaulty("select id,num,text from notable where num < 9"));
			rs = app.QueryMultipleErrors();
			rs.Close();
			app.Close();
		}

		private string[] expectedCols = new string[] { "QueryCollection: test4giis.qacoverapp.AppSimpleJdbc qcount=6,qerror=0,count=17,dead=7,error=0\n" + "  queryDifferentSingleLine\n" + "  queryDifferentSingleLine\n" + "  queryEqualDifferentLine\n" + "  queryEqualDifferentLine\n" + "  queryNoParameters1Condition\n"
			 + "  queryNoParameters2Condition", "QueryCollection: test4giis.qacoverapp.AppSimpleJdbc2 qcount=2,qerror=0,count=6,dead=4,error=0\n" + "  queryNoParameters1Condition\n" + "  queryNoParameters2Condition", "QueryCollection: test4giis.qacoverapp.AppSimpleJdbc3Errors qcount=4,qerror=1,count=8,dead=1,error=2\n"
			 + "  query0Errors\n" + "  query1ErrorAtQuery\n" + "  query1ErrorAtRule\n" + "  queryMultipleErrors" };

		private string expectedAllSummary = "CoverageCollection: qcount=12,qerror=1,count=31,dead=12,error=2";

		// Exptected values from the CoverageReaders
		private void AssertReaderByClass(int expectedIndex)
		{
			CoverageCollection ccol = new CoverageReader(Configuration.GetInstance().GetStoreRulesLocation()).GetByClass();
			QueryCollection qcol = ccol.Get(expectedIndex);
			va.AssertEquals(expectedCols[expectedIndex].ToLower(), qcol.ToString(true, false, false).ToLower(), "at assertReaderByClass(" + expectedIndex + ")");
		}

		// lowercase for netcore compatibility
		private void AssertReaderByClassAll()
		{
			CoverageCollection ccol = new CoverageReader(Configuration.GetInstance().GetStoreRulesLocation()).GetByClass();
			va.AssertEquals((expectedAllSummary + "\n" + expectedCols[0] + "\n" + expectedCols[1] + "\n" + expectedCols[2]).ToLower(), ccol.ToString(true, false, false).ToLower(), "at assertReaderByClassAll()");
		}

		// first checks all keys and then the data from CoverageReader
		private void AssertReaderByRunOrderAll()
		{
			StringBuilder allKeys = new StringBuilder();
			StringBuilder allData = new StringBuilder();
			CoverageReader cr = new CoverageReader(Configuration.GetInstance().GetStoreRulesLocation());
			QueryCollection cc = cr.GetByRunOrder();
			for (int i = 0; i < cc.Size(); i++)
			{
				allKeys.Append(cc.Get(i).GetKey() + "\n");
				allData.Append("*** " + cc.Get(i).GetKey() + "\n");
				allData.Append("sql: " + cc.Get(i).GetModel().GetSql() + "\n");
				allData.Append("parameters: " + cc.Get(i).GetParametersXml() + "\n");
				for (int j = 0; j < cc.Get(i).GetModel().GetRules().Count; j++)
				{
					allData.Append("rule" + j + ": " + new SqlRulesXmlSerializer().Serialize(cc.Get(i).GetModel().GetRules()[j].GetModel(), "fpcrule").Trim() + "\n");
				}
				if (!string.Empty.Equals(cc.Get(i).GetModel().GetErrorString()) || cc.Get(i).GetModel().GetRules().Count > 0)
				{
					SchemaModel schema = cc.Get(i).GetSchema();
					allData.Append("schema: " + new DbSchemaXmlSerializer().Serialize(schema.GetModel()) + "\n");
				}
			}
			FileUtil.FileWrite(outPath, "all-keys-by-run-order.txt", allKeys.ToString());
			FileUtil.FileWrite(outPath, "all-data-by-run-order.txt", allData.ToString());
			string actualKeys = FileUtil.FileRead(outPath, "all-keys-by-run-order.txt");
			string actualData = FileUtil.FileRead(outPath, "all-data-by-run-order.txt");
			string expectedKeys = FileUtil.FileRead(bmkPath, "all-keys-by-run-order.txt");
			string expectedData = FileUtil.FileRead(bmkPath, "all-data-by-run-order.txt");
			if (new Variability().IsNetCore())
			{
				expectedKeys = ReplaceExpectedStrings(expectedKeys);
				expectedData = ReplaceExpectedStrings(expectedData);
				expectedData = expectedData.Replace("\"INT\"", "\"int\"");
				expectedData = expectedData.Replace("\"VARCHAR\"", "\"varchar\"");
			}
			// line numbers as ### to allow comparison
			expectedKeys = JavaCs.ReplaceRegex(expectedKeys, "\\.\\d+\\.", ".###.");
			expectedData = JavaCs.ReplaceRegex(expectedData, "\\.\\d+\\.", ".###.");
			actualKeys = JavaCs.ReplaceRegex(actualKeys, "\\.\\d+\\.", ".###.");
			actualData = JavaCs.ReplaceRegex(actualData, "\\.\\d+\\.", ".###.");
			va.AssertEquals(expectedKeys.Replace("\r", string.Empty), actualKeys, "at assertReaderByRunOrderAll(), check expectedKeys");
			va.AssertEquals(expectedData.Replace("\r", string.Empty), actualData, "at assertReaderByRunOrderAll(), check expectedData");
		}

		// Html of generated reports
		private void AssertReports(string packageName, string className)
		{
			string expected = FileUtil.FileRead(bmkPath, packageName.ToLower() + className);
			string actual = FileUtil.FileRead(outPath, packageName + className);
			if (new Variability().IsNetCore())
			{
				expected = ReplaceExpectedStrings(expected);
			}
			expected = ReprocessReportForCompare(expected);
			actual = ReprocessReportForCompare(actual);
			va.AssertEquals(expected, actual, "at className: " + className);
		}

		public static string ReprocessReportForCompare(string content)
		{
			content = JavaCs.ReplaceRegex(content, ":\\d+<", ":###<");
			content = JavaCs.ReplaceRegex(content, "\\d{4}-\\d{2}-\\d{2}T\\d{2}:\\d{2}:\\d{2}\\.\\d{3}", "yyyy-MM-ddThh:mm:ss.SSS");
			content = JavaCs.ReplaceRegex(content, "\\[version .*\\]", "[version x.y.z]");
			content = JavaCs.ReplaceRegex(content, "Error at Get query table names: ApiException.*<\\/span>", "Error at Get query table names: ApiException<\\/span>");
			return content.Replace("\r", string.Empty);
		}

		private string ReplaceExpectedStrings(string expected)
		{
			expected = expected.Replace("test4giis.qacoverapp", "Test4in2test.Qacoverapp");
			expected = expected.Replace("queryDifferentSingleLine", "QueryDifferentSingleLine");
			expected = expected.Replace("queryEqualDifferentLine", "QueryEqualDifferentLine");
			expected = expected.Replace("queryNoParameters1Condition", "QueryNoParameters1Condition");
			expected = expected.Replace("queryNoParameters2Condition", "QueryNoParameters2Condition");
			expected = expected.Replace("query0Errors", "Query0Errors");
			expected = expected.Replace("query1ErrorAtQuery", "Query1ErrorAtQuery");
			expected = expected.Replace("query1ErrorAtRule", "Query1ErrorAtRule");
			expected = expected.Replace("queryMultipleErrors", "QueryMultipleErrors");
			expected = expected.Replace("java.lang.RuntimeException", "System.Exception");
			expected = expected.Replace("org.sqlite.SQLiteException", "Microsoft.Data.Sqlite.SqliteException");
			expected = expected.Replace("giis.qacover.portable", "Giis.Qacover.Portable");
			expected = expected.Replace("HTTP/1.1 404 Not Found", "NotFound");
			expected = expected.Replace("[SQLITE_ERROR] SQL error or missing database (near \"selectar\": syntax error)", "SQLite Error 1: 'near \"selectar\": syntax error'.");
			expected = expected.Replace("[SQLITE_ERROR] SQL error or missing database (no such table: notable)", "SQLite Error 1: 'no such table: notable'.");
			expected = expected.Replace("Giis.Tdrules.Openapi.Invoker.ApiException", "Giis.Tdrules.Openapi.Client.ApiException");
			return expected;
		}
	}
}
