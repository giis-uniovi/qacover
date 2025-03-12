using Java;
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
    /// Handling of nulls in rule generation and evaluation
    /// </summary>
    public class TestEvaluationNulls : Base
    {
        private AppSimpleJdbc app;
        [NUnit.Framework.SetUp]
        public override void SetUp()
        {
            base.SetUp();
            app = new AppSimpleJdbc(variant);
            SetUpTestData();
        }

        [NUnit.Framework.TearDown]
        public override void TearDown()
        {
            base.TearDown();
            app.Close();
        }

        public virtual void SetUpTestData()
        {
            app.DropTable("test");
            app.ExecuteUpdateNative(new string[] { "create table test(id int not null, num int not null, text varchar(16))", "insert into test(id,num,text) values(1,0,'abc')", "insert into test(id,num,text) values(3,99,null)" });
        }

        [Test]
        public virtual void TestParameterAsNullValue()
        {
            DoParameterSetNull(false); // assigning null value
        }

        [Test]
        public virtual void TestParameterAsSetNull()
        {
            DoParameterSetNull(true); // using setNull
        }

        public virtual void DoParameterSetNull(bool useSetNull)
        {
            Configuration.GetInstance().SetInferQueryParameters(false);
            string sql = "select id,num,text from test where text=?";
            rs = app.ExecuteQueryNulls(sql, null, useSetNull);
            AssertEvalResults(sql, "", SqlUtil.ResultSet2csv(rs, " "), "UNCOVERED SELECT id , num , text FROM test WHERE NOT(text = NULL)\n" + "UNCOVERED SELECT id , num , text FROM test WHERE (text = NULL)\n" + "COVERED   SELECT id , num , text FROM test WHERE (text IS NULL)", "{?1?=NULL}", false, new Variability().IsNetCore());
        }

        /// <summary>
        /// With parameter inference, query has been parsed, but the parameter is not shown in the result,
        /// that means the NULL value is not considered in the parameter inference
        /// </summary>
        [Test]
        public virtual void TestParameterInferNull()
        {
            Configuration.GetInstance().SetInferQueryParameters(true);
            string sql = "select id,num,text from test where text=NULL";
            rs = app.ExecuteQuery(sql);
            AssertEvalResults("SELECT id , num , text FROM test WHERE text = NULL", "", SqlUtil.ResultSet2csv(rs, " "), "UNCOVERED SELECT id , num , text FROM test WHERE NOT(text = NULL)\n" + "UNCOVERED SELECT id , num , text FROM test WHERE (text = NULL)\n" + "COVERED   SELECT id , num , text FROM test WHERE (text IS NULL)", "{}", false, new Variability().IsNetCore());
        }
    }
}