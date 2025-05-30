using Java.Lang;
using Java;
using NUnit.Framework;
using Giis.Portable.Util;
using Giis.Qacover.Core;
using Giis.Qacover.Core.Services;
using Giis.Qacover.Model;
using Giis.Qacover.Reader;
using Giis.Qacover.Report;
using Giis.Tdrules.Model.IO;
using Giis.Visualassert;
using Test4giis.Qacover;
using Test4giis.Qacoverapp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////

namespace Test4giis.Qacover.Model
{
    /// <summary>
    /// Report generation:
    /// As the application to be reported, uses three classes that include the
    /// most relevant situations that have been unit tested before,
    /// but here, the goal is to check everything in an integrated way.
    /// Configures QACover to send rules and reports under a different folder (qacover-report).
    /// 
    /// NOTE: when changing the report format, in addition to the htmls of
    /// qacover-core/src/test/resources/qacover-report, the following must be updted too:
    /// - dotnet reports at net/resources/qacover-report
    /// - IT reports at qacover-core/src/test/resources/qacover-uber-main
    /// - IT reports at qacover-core/src/test/resources/spring-petclinic-main
    /// </summary>
    public class TestReport : Base
    {
        // All comparisons are made over expected and actual files
        private string rulesPath = FileUtil.GetPath(Parameters.GetProjectRoot(), Parameters.GetReportSubdir(), "qacover-report", "rules");
        private string outPath = FileUtil.GetPath(Parameters.GetProjectRoot(), Parameters.GetReportSubdir(), "qacover-report", "reports");
        // Each platform (java/net) has its own set of expected values for the reports
        private string bmkPath = new Variability().IsJava() ? FileUtil.GetPath(Parameters.GetProjectRoot(), "src", "test", "resources", "qacover-report") : FileUtil.GetPath(Parameters.GetProjectRoot(), "resources", "qacover-report");
        private string reportAppPackage = new Variability().IsJava() ? "test4giis.qacoverapp." : "Test4giis.Qacoverapp.";
        private SoftVisualAssert va;
        [NUnit.Framework.SetUp]
        public override void SetUp()
        {
            base.SetUp();
            va = new SoftVisualAssert().SetCallStackLength(3).SetBrightColors(true);
        }

        [NUnit.Framework.TearDown]
        public override void TearDown()
        {
            va.AssertAll("diff-IntegratedTestReaderAndReport.html");
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
                options.SetRuleOptions("noboundaries");
            QueryStatement.SetFaultInjector(null);
            options.SetStoreRulesLocation(rulesPath);
            options.SetStoreReportsLocation(outPath);
        }

        [Test]
        public virtual void TestReports()
        {
            Reset();
            new StoreService(options).DropRules().DropLast(); // clean start
            FileUtil.CreateDirectory(outPath); // this is ensured by reporter, but we need it before to check readers

            // Runs the application mock classes to generate the rules
            RunReports1OfTestStore();
            RunReports2OfTestEvaluation();
            RunReports3OfTestError();

            // Before the report, checks the results of the readers
            AssertReaderByClassAll();
            AssertReaderKeysByRunOrderAll();
            AssertReaderDataByRunOrderAll();

            // Generate and test the html report
            // note that net runs four levels before solution root
            new ReportManager().Run(Configuration.GetInstance().GetStoreRulesLocation(), Configuration.GetInstance().GetStoreReportsLocation(), new Variability().IsJava() ? "src/test/java" : "../../../..", new Variability().IsJava() ? "" : "../../../..");
            string indexContent = FileUtil.FileRead(outPath, "index.html");
            FileUtil.FileWrite(outPath, "index.html", indexContent);
            AssertReports("", "index.html");
            AssertReports(reportAppPackage, "AppSimpleJdbc.html");
            AssertReports(reportAppPackage, "AppSimpleJdbc2.html");
            AssertReports(reportAppPackage, "AppSimpleJdbc3Errors.html");
        }

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
            app.QueryDifferentSingleLine(true, 99, false, "");
            rs.Close();
            app.QueryDifferentSingleLine(true, 99, true, "'xyz'");
            app.QueryEqualDifferentLine("'xyz'", "'aaa'");
            Configuration.GetInstance().SetInferQueryParameters(true);
            app.QueryEqualDifferentLine("'xyz'", "'aaa'");
            rs.Close();
            app.Close();
        }

        private void RunReports2OfTestEvaluation()
        {
            AppSimpleJdbc2 app = new AppSimpleJdbc2(variant);
            app.ExecuteUpdateNative(new string[] { "drop table if exists test", "create table test(id int not null, num int not null, text varchar(16))", "insert into test(id,num,text) values(1,0,'abc')", "insert into test(id,num,text) values(2,99,'xyz')", "insert into test(id,num,text) values(3,99,null)" });
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
            options.SetRuleServiceUrl("http://giis.uniovi.es/noexiste.xml");
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

        private void AssertReaderByClassAll()
        {
            CoverageCollection ccol = new CoverageReader(Configuration.GetInstance().GetStoreRulesLocation()).GetByClass();
            string expected = ccol.ToString(true, false, false);
            FileUtil.FileWrite(outPath, "all-by-class.txt", ccol.ToString(true, false, false));
            string actual = FileUtil.FileRead(bmkPath, "all-by-class.txt");
            va.AssertEquals(expected.Replace("\r", ""), actual.Replace("\r", ""), "at assertReaderByClassAll()");
        }

        private void AssertReaderKeysByRunOrderAll()
        {
            StringBuilder allKeys = new StringBuilder();
            CoverageReader cr = new CoverageReader(Configuration.GetInstance().GetStoreRulesLocation());
            HistoryReader cc = cr.GetHistory();
            for (int i = 0; i < cc.GetItems().Count; i++)
                allKeys.Append(cc.GetItems()[i].GetKey() + "|" + cc.GetItems()[i].GetParamsXml() + "\n");
            FileUtil.FileWrite(outPath, "all-keys-by-run-order.txt", allKeys.ToString());
            string actualKeys = FileUtil.FileRead(outPath, "all-keys-by-run-order.txt");
            string expectedKeys = FileUtil.FileRead(bmkPath, "all-keys-by-run-order.txt");
            va.AssertEquals(expectedKeys.Replace("\r", ""), actualKeys.Replace("\r", ""), "at assertReaderByRunOrderAll(), check expectedKeys");
        }

        private void AssertReaderDataByRunOrderAll()
        {
            StringBuilder allData = new StringBuilder();
            string rulesFolder = Configuration.GetInstance().GetStoreRulesLocation();
            CoverageReader cr = new CoverageReader(rulesFolder);
            HistoryReader cc = cr.GetHistory();
            for (int i = 0; i < cc.GetItems().Count; i++)
            {
                allData.Append("*** " + cc.GetItems()[i].GetKey() + "\n");

                // The history reader is independent from the models (the only join is the key),
                // we use a standalone query reader to get the model
                QueryReader qr = new QueryReader(rulesFolder, cc.GetItems()[i].GetKey());
                QueryModel model = qr.GetModel();
                allData.Append("sql: " + model.GetSql() + "\n");
                allData.Append("parameters: " + cc.GetItems()[i].GetParamsXml() + "\n");
                for (int j = 0; j < model.GetRules().Count; j++)
                    allData.Append("rule" + j + ": " + new TdRulesXmlSerializer().Serialize(model.GetRules()[j].GetModel(), "fpcrule").Trim() + "\n");
                if (!"".Equals(model.GetErrorString()) || model.GetRules().Count > 0)
                {
                    SchemaModel schema = qr.GetSchema();
                    allData.Append("schema: " + new TdSchemaXmlSerializer().Serialize(schema.GetModel()) + "\n");
                }
            }

            FileUtil.FileWrite(outPath, "all-data-by-run-order.txt", allData.ToString());
            string actualData = FileUtil.FileRead(outPath, "all-data-by-run-order.txt");
            string expectedData = FileUtil.FileRead(bmkPath, "all-data-by-run-order.txt");
            va.AssertEquals(expectedData.Replace("\r", ""), actualData.Replace("\r", ""), "at assertReaderByRunOrderAll(), check expectedData");
        }

        // Html of generated reports
        private void AssertReports(string packageName, string className)
        {
            string expected = FileUtil.FileRead(bmkPath, packageName.ToLower() + className);
            string actual = FileUtil.FileRead(outPath, packageName + className);
            expected = ReprocessReportForCompare(expected);
            actual = ReprocessReportForCompare(actual);
            va.AssertEquals(expected, actual, "at className: " + className);
        }

        public static string ReprocessReportForCompare(string content)
        {

            // to compare using a fixed date and version number
            content = JavaCs.ReplaceRegex(content, "\\d{4}-\\d{2}-\\d{2}T\\d{2}:\\d{2}:\\d{2}\\.\\d{3}", "yyyy-MM-ddThh:mm:ss.SSS");
            content = JavaCs.ReplaceRegex(content, "\\[version .*\\]", "[version x.y.z]");

            // error at query gives different content in local and CI, remove the variable part
            content = JavaCs.ReplaceRegex(content, "Query error: Error at Get query table names: .*<\\/span>", "Query error: Error at Get query table names: (rest of message removed)<\\/span>");
            return content.Replace("\r", "");
        }
    }
}