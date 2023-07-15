/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using Giis.Qacover.Core.Services;
using Giis.Qacover.Model;
using NUnit.Framework;

using Test4giis.Qacoverapp;

namespace Test4giis.Qacover
{
	/// <summary>
	/// Rule generation and evaluation with jdbc connections created with DriverManager
	/// and using the executeQuery methods (other execution methods are in TestExecute)
	/// Situations
	/// - type of query: no parameters / with parameters / using Apache DbUtils (in separate class)
	/// - number of parameters: one / more
	/// - datatype of parameters: int / string
	/// - number of conditions: one / two / none
	/// - additional FPC options: with / without
	/// - number of rules covered: one / two /none
	/// - positoin of rules covered: boundaries
	/// - Identifiers: combine table /column with double quote / brackets(sqlserver)
	/// - Named jdbc parameters: can be replaced / duplicated value error
	/// </summary>
	public class TestEvaluation : Base
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
			app.ExecuteUpdateNative(new string[] { "create table test(id int not null, num int not null, text varchar(16))", "insert into test(id,num,text) values(1,0,'abc')", "insert into test(id,num,text) values(2,99,'xyz')", "insert into test(id,num,text) values(3,99,null)" });
		}

		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestEvalNoParameters()
		{
			rs = app.QueryNoParameters1Condition(-1);
			AssertEvalResults("select id,num,text from test where num>=-1", "1 0 abc\n2 99 xyz\n3 99 NULL", SqlUtil.ResultSet2csv(rs, " "), "COVERED   SELECT id , num , text FROM test WHERE (num = 0)\n" + "UNCOVERED SELECT id , num , text FROM test WHERE (num = -1)\n" + "UNCOVERED SELECT id , num , text FROM test WHERE (num = -2)"
				, "{}");
		}

		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestEvalInferParameters()
		{
			// no prepared statement, parameters replaced in the string
			Configuration.GetInstance().SetInferQueryParameters(true);
			rs = app.QueryNoParameters2Condition(98, "abc");
			// sql has been parsed and parameters are inferend
			AssertEvalResults("SELECT id , num , text FROM test WHERE num > ?1? AND text = ?2?", string.Empty, SqlUtil.ResultSet2csv(rs, " "), "UNCOVERED SELECT id , num , text FROM test WHERE (num = 98 + 1) AND (text = 'abc')\n" + "UNCOVERED SELECT id , num , text FROM test WHERE (num = 98) AND (text = 'abc')\n"
				 + "UNCOVERED SELECT id , num , text FROM test WHERE (num = 98 - 1) AND (text = 'abc')\n" + "UNCOVERED SELECT id , num , text FROM test WHERE (text = 'abc') AND (num > 98)\n" + "COVERED   SELECT id , num , text FROM test WHERE NOT(text = 'abc') AND (num > 98)\n" + "COVERED   SELECT id , num , text FROM test WHERE (text IS NULL) AND (num > 98)"
				);
		}

		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestEvalInferParametersNegative()
		{
			Configuration.GetInstance().SetInferQueryParameters(true);
			rs = app.QueryNoParameters1Condition(-1);
			AssertEvalResults("SELECT id , num , text FROM test WHERE num >= ?1?", "1 0 abc\n2 99 xyz\n3 99 NULL", SqlUtil.ResultSet2csv(rs, " "), "COVERED   SELECT id , num , text FROM test WHERE (num = -1 + 1)\n" + "UNCOVERED SELECT id , num , text FROM test WHERE (num = -1)\n" + "UNCOVERED SELECT id , num , text FROM test WHERE (num = -1 - 1)"
				);
		}

		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestEvalFpcOptions()
		{
			Configuration.GetInstance().Reset().SetFpcServiceOptions("noboundaries");
			rs = app.QueryNoParameters1Condition(-1);
			AssertEvalResults("select id,num,text from test where num>=-1", "1 0 abc\n2 99 xyz\n3 99 NULL", SqlUtil.ResultSet2csv(rs, " "), "UNCOVERED SELECT id , num , text FROM test WHERE NOT(num >= -1)\n" + "COVERED   SELECT id , num , text FROM test WHERE (num >= -1)");
		}

		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestEvalParameters()
		{
			Configuration.GetInstance().SetInferQueryParameters(false);
			rs = app.QueryParameters(98, "abc");
			AssertEvalResults("select id,num,text from test where num>? and text=?", string.Empty, SqlUtil.ResultSet2csv(rs, " "), "UNCOVERED SELECT id , num , text FROM test WHERE (num = 98 + 1) AND (text = 'abc')\n" + "UNCOVERED SELECT id , num , text FROM test WHERE (num = 98) AND (text = 'abc')\n"
				 + "UNCOVERED SELECT id , num , text FROM test WHERE (num = 98 - 1) AND (text = 'abc')\n" + "UNCOVERED SELECT id , num , text FROM test WHERE (text = 'abc') AND (num > 98)\n" + "COVERED   SELECT id , num , text FROM test WHERE NOT(text = 'abc') AND (num > 98)\n" + "COVERED   SELECT id , num , text FROM test WHERE (text IS NULL) AND (num > 98)"
				, "{?1?=98, ?2?='abc'}", false, new Variability().IsNetCore());
		}

		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestEvalParametersNamed()
		{
			if (new Variability().IsJava4() || new Variability().IsNetCore())
			{
				return;
			}
			Configuration.GetInstance().SetInferQueryParameters(false).SetFpcServiceOptions("noboundaries");
			rs = app.QueryParametersNamed(1, 1, "abc");
			AssertEvalResults("/* params=?1?,?1?,?2? */ select id,num,text from test where id=? or num=? or text=?", "1 0 abc", SqlUtil.ResultSet2csv(rs, " "), "COVERED   /*params=1,1,'abc'*/ SELECT id , num , text FROM test WHERE NOT(id = 1) AND NOT(num = 1) AND NOT(text = 'abc')\n" + "UNCOVERED /*params=1,1,'abc'*/ SELECT id , num , text FROM test WHERE (id = 1) AND NOT(num = 1) AND NOT(text = 'abc')\n"
				 + "UNCOVERED /*params=1,1,'abc'*/ SELECT id , num , text FROM test WHERE (num = 1) AND NOT(id = 1) AND NOT(text = 'abc')\n" + "UNCOVERED /*params=1,1,'abc'*/ SELECT id , num , text FROM test WHERE (text = 'abc') AND NOT(id = 1) AND NOT(num = 1)\n" + "COVERED   /*params=1,1,'abc'*/ SELECT id , num , text FROM test WHERE (text IS NULL) AND NOT(id = 1) AND NOT(num = 1)"
				);
			QueryModel model = StoreService.GetLast().GetQueryModel(StoreService.GetLast().GetLastSavedQueryKey());
			string sql = model.GetSql();
			NUnit.Framework.Assert.AreEqual("/*params=?1?,?1?,?2?*/ SELECT id , num , text FROM test WHERE id = ?1? OR num = ?1? OR text = ?2?", sql);
		}

		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestEvalParametersNamedInconsistency()
		{
			if (new Variability().IsJava4() || new Variability().IsNetCore())
			{
				return;
			}
			Configuration.GetInstance().SetInferQueryParameters(false).SetFpcServiceOptions("noboundaries");
			rs = app.QueryParametersNamed(1, 2, "abc");
			NUnit.Framework.Assert.AreEqual("Error at : giis.qacover.portable.QaCoverException: StatementAdapter: Parameter ?1? had been assigned to 1. Can't be assigned to a new value 2", StoreService.GetLast().GetLastGenStatus());
		}

		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestEvalNoConditions()
		{
			rs = app.QueryNoConditions();
			AssertEvalResults("select id,num,text from test", "1 0 abc\n2 99 xyz\n3 99 NULL", SqlUtil.ResultSet2csv(rs, " "), string.Empty);
		}

		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestEvalNoConditionsInferParams()
		{
			Configuration.GetInstance().Reset().SetInferQueryParameters(true);
			rs = app.QueryNoConditions();
			AssertEvalResults("SELECT id , num , text FROM test", "1 0 abc\n2 99 xyz\n3 99 NULL", SqlUtil.ResultSet2csv(rs, " "), string.Empty);
		}

		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestEvalIdentifiersWithQuotes()
		{
			if (new Variability().IsJava4() || new Variability().IsNetCore())
			{
				return;
			}
			// ignore in java 1.4, uses h2 that causes error when text has double quotes
			string table = "test";
			// Oracle and H2: quoted identifier is case sensitive
			string column = variant.IsOracle() || variant.IsH2() ? "\"TEXT\"" : "\"text\"";
			rs = app.QueryNoParametersQuotes("xyz", false, true);
			//quotes solo en columna
			AssertEvalResults("select id,num,text from test where " + column + "='xyz'", "2 99 xyz", SqlUtil.ResultSet2csv(rs, " "), "COVERED   SELECT id , num , text FROM test WHERE NOT(" + column + " = 'xyz')\n" + "COVERED   SELECT id , num , text FROM test WHERE (" + column + " = 'xyz')\n"
				 + "COVERED   SELECT id , num , text FROM test WHERE (" + column + " IS NULL)");
			rs = app.QueryNoParametersQuotes("xyz", true, true);
			//quotes en columna y tabla
			table = variant.IsOracle() || variant.IsH2() ? "\"TEST\"" : "\"test\"";
			AssertEvalResults("select id,num,text from " + table + " where " + column + "='xyz'", "2 99 xyz", SqlUtil.ResultSet2csv(rs, " "), "COVERED   SELECT id , num , text FROM " + table + " WHERE NOT(" + column + " = 'xyz')\n" + "COVERED   SELECT id , num , text FROM " + table + " WHERE ("
				 + column + " = 'xyz')\n" + "COVERED   SELECT id , num , text FROM " + table + " WHERE (" + column + " IS NULL)");
		}
		/*
		public void testEvalIdentifiersWithBrackets() throws SQLException {
		//Options.getInstance().reset().setInferQueryParameters(true);
		rs = app.queryNoParametersBrackets("xyz", false, true);
		assertResults("select id,num,text from test where [text]='xyz'", "2 99 xyz", SqlUtil.resultSet2csv(rs," "),
		"COVERED   SELECT id , num , text FROM test WHERE NOT([text] = 'xyz')\n" +
		"COVERED   SELECT id , num , text FROM test WHERE ([text] = 'xyz')\n" +
		"COVERED   SELECT id , num , text FROM test WHERE ([text] IS NULL)");
		rs = app.queryNoParametersBrackets("xyz", true, true);
		assertResults("select id,num,text from [test] where [test]='xyz'", "2 99 xyz", SqlUtil.resultSet2csv(rs," "),
		"COVERED   SELECT id , num , text FROM [test] WHERE NOT([text] = 'xyz')\n" +
		"COVERED   SELECT id , num , text FROM [test] WHERE ([text] = 'xyz')\n" +
		"COVERED   SELECT id , num , text FROM [test] WHERE ([text] IS NULL)");
		}
		*/
	}
}
