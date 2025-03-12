using NUnit.Framework;
using Giis.Qacover.Core.Services;
using Giis.Qacover.Model;
using Test4giis.Qacoverapp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////

namespace Test4giis.Qacover
{
    /// <summary>
    /// Handling of data types that are DBMS specific and must be configured in p6spy.properties
    /// </summary>
    public class TestDataTypeFormats : Base
    {
        protected AppSimpleJdbc app;
        [NUnit.Framework.SetUp]
        public override void SetUp()
        {
            base.SetUp();
            app = new AppSimpleJdbc(variant);

            // usa full name for easier conversion to C#
            //com.p6spy.engine.spy.P6SpyOptions.GetActiveInstance().SetDatabaseDialectBooleanFormat("numeric");
            //com.p6spy.engine.spy.P6SpyOptions.GetActiveInstance().SetDatabaseDialectDateFormat("yyyy-MM-dd");
            SetUpTestData();
            Configuration.GetInstance().SetRuleOptions("noboundaries");
        }

        [NUnit.Framework.TearDown]
        public override void TearDown()
        {
            base.TearDown();
            app.Close();
        }

        // centralize the setup for all variants
        public virtual void SetUpTestData()
        {
            string date1 = "'2020-02-01T00:00:00.000+0100'";
            string date2 = "'2020-01-02T00:00:00.000+0100'";
            if (variant.IsSqlServer() || variant.IsH2())
            {
                date1 = "'2020-02-01'";
                date2 = "'2020-01-02'";
            }
            else if (variant.IsOracle())
            {
                date1 = "DATE '2020-02-01'";
                date2 = "DATE '2020-01-02'";
            }

            string bitType = "number(1)";
            if (variant.IsSqlServer())
                bitType = "bit";
            else if (variant.IsPostgres() || variant.IsH2())
                bitType = "boolean";
            app.DropTable("dbmstest");
            app.ExecuteUpdateNative(new string[] { "create table dbmstest(id int not null, bool " + bitType + ", dat date)", "insert into dbmstest(id,bool,dat) values(1," + BoolValue(false) + "," + date1 + ")", "insert into dbmstest(id,bool,dat) values(2," + BoolValue(true) + "," + date2 + ")" });
        }

        protected virtual string BoolValue(bool value)
        {
            return value ? "1" : "0";
        }

        [Test]
        public virtual void TestEvalBooleanParameters()
        {
            Configuration.GetInstance().SetInferQueryParameters(false);
            string expOuput = "2 1";
            if (variant.IsNetCore() && variant.IsSqlServer())
                expOuput = "2 True";
            else if (variant.IsPostgres())
                expOuput = "2 t";
            else if (variant.IsH2())
                expOuput = "2 TRUE";
            rs = app.ExecuteQuery("select id,bool from dbmstest where bool=?", true);
            AssertEvalResults("select id,bool from dbmstest where bool=?", expOuput, SqlUtil.ResultSet2csv(rs, " "), "COVERED   SELECT id , bool FROM dbmstest WHERE NOT(bool = " + BoolValue(true).ToString() + ")\n" + "COVERED   SELECT id , bool FROM dbmstest WHERE (bool = " + BoolValue(true).ToString() + ")\n" + "UNCOVERED SELECT id , bool FROM dbmstest WHERE (bool IS NULL)", "{?1?=" + BoolValue(true).ToString() + "}", false, new Variability().IsNetCore());
        }

        [Test]
        public virtual void TestEvalDateParameters()
        {
            Configuration.GetInstance().SetInferQueryParameters(false);

            // column to check the results
            string dateColumn = "dat";
            if (variant.IsSqlite())
                dateColumn = "SUBSTR(dat , 1 , 10)";
            if (variant.IsSqlServer())
                dateColumn = "CONVERT(varchar , dat , 23)";

            // Each SGBD has different precisions in time
            string expOuput = "2 2020-01-02";
            if (variant.IsOracle())
                expOuput = "2 2020-01-02 00:00:00";
            else if (variant.IsJava() && variant.IsSqlite())
                expOuput = "";
            bool convertNetParameters = false;
            if (variant.IsNetCore())
                convertNetParameters = true;
            rs = app.ExecuteQuery("select id," + dateColumn + " from dbmstest where dat<?", Java.Sql.Date.ValueOf("2020-01-31"));
            AssertEvalResults("select id," + dateColumn + " from dbmstest where dat<?", expOuput, SqlUtil.ResultSet2csv(rs, " "), "COVERED   SELECT id , " + dateColumn + " FROM dbmstest WHERE NOT(dat < '2020-01-31')\n" + "COVERED   SELECT id , " + dateColumn + " FROM dbmstest WHERE (dat < '2020-01-31')\n" + "UNCOVERED SELECT id , " + dateColumn + " FROM dbmstest WHERE (dat IS NULL)", "{?1?='2020-01-31'}", false, convertNetParameters);
        }
    }
}