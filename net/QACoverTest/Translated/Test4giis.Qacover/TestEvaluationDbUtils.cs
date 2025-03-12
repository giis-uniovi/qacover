using NUnit.Framework;
using NLog;
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
    /// Extension of TestEvaluation for queries created with Apache DbUtils
    /// </summary>
    public class TestEvaluationDbUtils : Base
    {
        private static readonly Logger log = Giis.Portable.Util.NLogUtil.GetLogger(typeof(TestEvaluationDbUtils));
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
            app.ExecuteUpdateNative(new string[] { "create table test(id int not null, num int not null, text varchar(16))", "insert into test(id,num,text) values(1,0,'abc')", "insert into test(id,num,text) values(2,99,'xyz')", "insert into test(id,num,text) values(3,99,null)" });
        }

        [Test]
        public virtual void TestEvalDbUtilsParameters()
        {
            AppDbUtils app = new AppDbUtils(variant); // Need a different mock application to use DbUtils
            if (new Variability().IsNetCore())
                return;
            app.QueryDbUtils(98, "abc");
            log.Debug("Output of native resultset");
            AssertEvalResults("select id,num,text from test where text=? and num>?", "", "", "UNCOVERED SELECT id , num , text FROM test WHERE (text = 'abc') AND (num > 98)\n" + "COVERED   SELECT id , num , text FROM test WHERE NOT(text = 'abc') AND (num > 98)\n" + "COVERED   SELECT id , num , text FROM test WHERE (text IS NULL) AND (num > 98)\n" + "UNCOVERED SELECT id , num , text FROM test WHERE (num = 98 + 1) AND (text = 'abc')\n" + "UNCOVERED SELECT id , num , text FROM test WHERE (num = 98) AND (text = 'abc')\n" + "UNCOVERED SELECT id , num , text FROM test WHERE (num = 98 - 1) AND (text = 'abc')");
        }
    }
}