/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using Giis.Qacover.Core.Services;
using Giis.Qacover.Model;
using NUnit.Framework;

using Test4giis.Qacoverapp;

namespace Test4giis.Qacover
{
	/// <summary>Handling of nulls in rule generation and evaluation</summary>
	public class TestEvaluationNulls : Base
	{
		private AppSimpleJdbc app;

		/// <exception cref="Java.Sql.SQLException"/>
		[NUnit.Framework.SetUp]
		public override void SetUp()
		{
			base.SetUp();
			app = new AppSimpleJdbc(variant);
			SetUpTestData();
		}

		/// <exception cref="Java.Sql.SQLException"/>
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

		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestParameterAsNullValue()
		{
			DoParameterSetNull(false);
		}

		// assigning null value
		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestParameterAsSetNull()
		{
			DoParameterSetNull(true);
		}

		// using setNull
		/// <exception cref="Java.Sql.SQLException"/>
		public virtual void DoParameterSetNull(bool useSetNull)
		{
			Configuration.GetInstance().SetInferQueryParameters(false);
			string sql = "select id,num,text from test where text=?";
			rs = app.ExecuteQueryNulls(sql, null, useSetNull);
			AssertEvalResults(sql, string.Empty, SqlUtil.ResultSet2csv(rs, " "), "UNCOVERED SELECT id , num , text FROM test WHERE NOT(text = NULL)\n" + "UNCOVERED SELECT id , num , text FROM test WHERE (text = NULL)\n" + "COVERED   SELECT id , num , text FROM test WHERE (text IS NULL)", "{?1?=NULL}"
				, false, new Variability().IsNetCore());
		}

		/// <summary>
		/// With parameter inference, query has been parsed, but the parameter is not shown in the result,
		/// that means the NULL value is not considered in the parameter inference
		/// </summary>
		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestParameterInferNull()
		{
			Configuration.GetInstance().SetInferQueryParameters(true);
			string sql = "select id,num,text from test where text=NULL";
			rs = app.ExecuteQuery(sql);
			AssertEvalResults("SELECT id , num , text FROM test WHERE text = NULL", string.Empty, SqlUtil.ResultSet2csv(rs, " "), "UNCOVERED SELECT id , num , text FROM test WHERE NOT(text = NULL)\n" + "UNCOVERED SELECT id , num , text FROM test WHERE (text = NULL)\n" + "COVERED   SELECT id , num , text FROM test WHERE (text IS NULL)"
				, "{}", false, new Variability().IsNetCore());
		}
	}
}
