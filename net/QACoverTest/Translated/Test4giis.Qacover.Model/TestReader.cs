/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using Giis.Portable.Util;
using Giis.Qacover.Core.Services;
using Giis.Qacover.Model;
using Giis.Qacover.Reader;
using NUnit.Framework;

using Test4giis.Qacover;
using Test4giis.Qacoverapp;

namespace Test4giis.Qacover.Model
{
	/// <summary>Unit tests for CoverageReader and dependent objects.</summary>
	/// <remarks>Unit tests for CoverageReader and dependent objects. More integrated tests in TestReport</remarks>
	public class TestReader : Base
	{
		private AppSimpleJdbc app;

		private string qline1;

		// line number of the interaction point used here (different on Java an .NET)(
		/// <exception cref="Java.Sql.SQLException"/>
		[NUnit.Framework.SetUp]
		public override void SetUp()
		{
			base.SetUp();
			app = new AppSimpleJdbc(variant);
			SetUpTestData();
			qline1 = new Variability().IsNetCore() ? "29" : "23";
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
			app.ExecuteUpdateNative(new string[] { "create table test(id int not null, num int not null, text varchar(16))", "insert into test(id,num,text) values(1,0,'abc')" });
		}

		// Query evaluation and Coverage reader instantiation
		/// <exception cref="Java.Sql.SQLException"/>
		private CoverageReader GetCoverageReader()
		{
			Configuration.GetInstance().SetInferQueryParameters(false);
			// Situations: no parameters / parameters / parameters contains vertical bar / query evaluated twice
			rs = app.QueryParameters(98, "abc");
			rs.Close();
			rs = app.QueryNoParameters1Condition(-1);
			// this must be before if ordering is alphabetic
			rs.Close();
			rs = app.QueryParameters(98, "a|c");
			StoreService store = StoreService.GetLast();
			return new CoverageReader(store.GetStoreLocation());
		}

		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestReaderByClass()
		{
			CoverageReader reader = GetCoverageReader();
			// List of classes CoverageCollection
			CoverageCollection classes = reader.GetByClass();
			NUnit.Framework.Assert.AreEqual(1, classes.Size());
			// Content of a class: QueryCollection
			QueryCollection query = classes.Get(0);
			AssertEqualsCs("test4giis.qacoverapp.AppSimpleJdbc", query.GetName());
			NUnit.Framework.Assert.AreEqual(2, query.Size());
			// Each query: QueryReader
			QueryReader item = query.Get(0);
			AssertEqualsCs("test4giis.qacoverapp.AppSimpleJdbc", item.GetKey().GetClassName());
			AssertEqualsCs("queryNoParameters1Condition", item.GetKey().GetMethodName());
			NUnit.Framework.Assert.AreEqual(qline1, item.GetKey().GetClassLine());
			AssertEqualsCs("test4giis.qacoverapp.AppSimpleJdbc.queryNoParameters1Condition." + qline1 + ".63629c65b13acf17c46df6199346b2fa414b78edfddccf3ba7f875eca30393b3", item.GetKey().ToString());
			NUnit.Framework.Assert.AreEqual("select id,num,text from test where num>=-1", item.GetSql());
			SchemaModel schema = item.GetSchema();
			NUnit.Framework.Assert.AreEqual("test", schema.GetModel().GetEntities()[0].GetName());
			NUnit.Framework.Assert.AreEqual("id", schema.GetModel().GetEntities()[0].GetAttributes()[0].GetName());
			// this view is static, no execution data
			NUnit.Framework.Assert.AreEqual(string.Empty, item.GetTimestamp());
			NUnit.Framework.Assert.AreEqual(string.Empty, item.GetParametersXml());
			// can access the QueryModel data
			NUnit.Framework.Assert.AreEqual("select id,num,text from test where num>=-1", item.GetModel().GetSql());
			IList<RuleModel> allRules = item.GetModel().GetRules();
			NUnit.Framework.Assert.AreEqual(3, allRules.Count);
			NUnit.Framework.Assert.AreEqual("SELECT id , num , text FROM test WHERE (num = 0)", allRules[0].GetSql());
			NUnit.Framework.Assert.AreEqual("SELECT id , num , text FROM test WHERE (num = -1)", allRules[1].GetSql());
			NUnit.Framework.Assert.AreEqual("SELECT id , num , text FROM test WHERE (num = -2)", allRules[2].GetSql());
			// basic comprobation of a second item
			item = query.Get(1);
			AssertEqualsCs("test4giis.qacoverapp.AppSimpleJdbc", item.GetKey().GetClassName());
			AssertEqualsCs("queryParameters", item.GetKey().GetMethodName());
		}

		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestReaderByClassAsString()
		{
			CoverageReader reader = GetCoverageReader();
			CoverageCollection classes = reader.GetByClass();
			string classesStr = "CoverageCollection:\n" + "QueryCollection: test4giis.qacoverapp.AppSimpleJdbc\n" + "  queryNoParameters1Condition\n" + "  queryParameters";
			AssertEqualsCs(classesStr, classes.ToString());
			AssertEqualsCs(classesStr, classes.ToString(false, false, false));
			string classesStrAll = "CoverageCollection: qcount=2,qerror=0,count=9,dead=1,error=0\n" + "QueryCollection: test4giis.qacoverapp.AppSimpleJdbc qcount=2,qerror=0,count=9,dead=1,error=0\n" + "  queryNoParameters1Condition:23 test4giis.qacoverapp.AppSimpleJdbc.queryNoParameters1Condition.23.63629c65b13acf17c46df6199346b2fa414b78edfddccf3ba7f875eca30393b3\n"
				 + "  queryParameters:29 test4giis.qacoverapp.AppSimpleJdbc.queryParameters.29.d4a43c80328b80e4552866634547537e7254a10aba076820f905c4617b60aff9";
			if (new Variability().IsJava())
			{
				// ignores in .net (too many differences to compare)
				AssertEqualsCs(classesStrAll, classes.ToString(true, true, true));
			}
			NUnit.Framework.Assert.AreEqual("qcount=2,qerror=0,count=9,dead=1,error=0", classes.GetSummary().ToString());
			QueryCollection query = classes.Get(0);
			AssertEqualsCs("QueryCollection: test4giis.qacoverapp.AppSimpleJdbc\n" + "  queryNoParameters1Condition\n" + "  queryParameters", query.ToString());
			NUnit.Framework.Assert.AreEqual("qcount=2,qerror=0,count=9,dead=1,error=0", query.GetSummary().ToString());
		}

		/// <exception cref="Java.Sql.SQLException"/>
		/// <exception cref="Java.Text.ParseException"/>
		[Test]
		public virtual void TestReaderByRunOrder()
		{
			CoverageReader reader = GetCoverageReader();
			// This should return the collection with an evaluation in each item
			QueryCollection query = reader.GetByRunOrder();
			NUnit.Framework.Assert.AreEqual(3, query.Size());
			QueryReader item = query.Get(0);
			AssertEqualsCs("test4giis.qacoverapp.AppSimpleJdbc", item.GetKey().GetClassName());
			AssertEqualsCs("queryParameters", item.GetKey().GetMethodName());
			AssertEqualsCs(SqlCs("SELECT id , num , text FROM test WHERE num > ?1? AND text = ?2?"), item.GetSql());
			JavaCs.ParseIsoDate(item.GetTimestamp());
			NUnit.Framework.Assert.AreEqual(SqlCs("<parameters><parameter name=\"?1?\" value=\"98\" /><parameter name=\"?2?\" value=\"'abc'\" /></parameters>"), item.GetParametersXml());
			item = query.Get(1);
			AssertEqualsCs("test4giis.qacoverapp.AppSimpleJdbc", item.GetKey().GetClassName());
			AssertEqualsCs("queryNoParameters1Condition", item.GetKey().GetMethodName());
			NUnit.Framework.Assert.AreEqual("select id,num,text from test where num>=-1", item.GetSql());
			JavaCs.ParseIsoDate(item.GetTimestamp());
			NUnit.Framework.Assert.AreEqual("<parameters></parameters>", item.GetParametersXml());
			item = query.Get(2);
			AssertEqualsCs("test4giis.qacoverapp.AppSimpleJdbc", item.GetKey().GetClassName());
			AssertEqualsCs("queryParameters", item.GetKey().GetMethodName());
			AssertEqualsCs(SqlCs("SELECT id , num , text FROM test WHERE num > ?1? AND text = ?2?"), item.GetSql());
			JavaCs.ParseIsoDate(item.GetTimestamp());
			NUnit.Framework.Assert.AreEqual(SqlCs("<parameters><parameter name=\"?1?\" value=\"98\" /><parameter name=\"?2?\" value=\"'a|c'\" /></parameters>"), item.GetParametersXml());
		}

		private string SqlCs(string sql)
		{
			// for compatibility between java and .net parameters
			if (new Variability().IsNetCore())
			{
				return sql.Replace(" , ", ",").Replace(" = ", "=").Replace(" > ", ">").Replace("?1?", "@param1").Replace("?2?", "@param2");
			}
			else
			{
				return sql;
			}
		}

		// Main invalid situations
		[Test]
		public virtual void TestReaderInvalidFolderNotExist()
		{
			try
			{
				CoverageReader reader = new CoverageReader("pathnotexist");
				reader.GetByClass();
				NUnit.Framework.Assert.Fail("Deberia producirse una excepcion");
			}
			catch (Exception e)
			{
				NUnit.Framework.Assert.AreEqual("Can't browse directory at path pathnotexist", e.Message);
			}
		}

		[Test]
		public virtual void TestReaderInvalidIndexNotExist()
		{
			try
			{
				CoverageReader reader = new CoverageReader(".");
				// folder exists, but no index
				reader.GetByRunOrder();
				NUnit.Framework.Assert.Fail("Deberia producirse una excepcion");
			}
			catch (Exception e)
			{
				AssertContains("Error reading file", e.Message);
				AssertContains("00HISTORY.log", e.Message);
			}
		}

		[Test]
		public virtual void TestJavaCsFilePath()
		{
			// getPath uses apache commons to concatenate paths, but returns null
			// if first parameter is relative.
			// Check that patch to solve this works
			AssertContains("aa/xx", FileUtil.GetPath("aa", "xx").Replace("\\", "/"));
			AssertContains("aa/xx", FileUtil.GetPath("./aa", "xx").Replace("\\", "/"));
			AssertContains("bb/aa/xx", FileUtil.GetPath("../bb/aa", "xx").Replace("\\", "/"));
			AssertContains("bb/aa/xx", FileUtil.GetPath("../../bb/aa", "xx").Replace("\\", "/"));
		}
	}
}
