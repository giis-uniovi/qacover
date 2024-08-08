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
			NUnit.Framework.Legacy.ClassicAssert.AreEqual(1, classes.Size());
			// Content of a class: QueryCollection
			QueryCollection query = classes.Get(0);
			AssertEqualsCs("test4giis.qacoverapp.AppSimpleJdbc", query.GetName());
			NUnit.Framework.Legacy.ClassicAssert.AreEqual(2, query.Size());
			// Each query: QueryReader
			QueryReader item = query.Get(0);
			AssertEqualsCs("test4giis.qacoverapp.AppSimpleJdbc", item.GetKey().GetClassName());
			AssertEqualsCs("queryNoParameters1Condition", item.GetKey().GetMethodName());
			NUnit.Framework.Legacy.ClassicAssert.AreEqual(qline1, item.GetKey().GetClassLine());
			AssertEqualsCs("test4giis.qacoverapp.AppSimpleJdbc.queryNoParameters1Condition." + qline1 + ".63629c65b13acf17c46df6199346b2fa414b78edfddccf3ba7f875eca30393b3", item.GetKey().ToString());
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("select id,num,text from test where num>=-1", item.GetSql());
			SchemaModel schema = item.GetSchema();
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("test", schema.GetModel().GetEntities()[0].GetName());
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("id", schema.GetModel().GetEntities()[0].GetAttributes()[0].GetName());
			// this view is static, no execution data
			NUnit.Framework.Legacy.ClassicAssert.AreEqual(string.Empty, item.GetTimestamp());
			NUnit.Framework.Legacy.ClassicAssert.AreEqual(string.Empty, item.GetParametersXml());
			// can access the QueryModel data
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("select id,num,text from test where num>=-1", item.GetModel().GetSql());
			IList<RuleModel> allRules = item.GetModel().GetRules();
			NUnit.Framework.Legacy.ClassicAssert.AreEqual(3, allRules.Count);
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("SELECT id , num , text FROM test WHERE (num = 0)", allRules[0].GetSql());
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("SELECT id , num , text FROM test WHERE (num = -1)", allRules[1].GetSql());
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("SELECT id , num , text FROM test WHERE (num = -2)", allRules[2].GetSql());
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
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("qcount=2,qerror=0,count=9,dead=1,error=0", classes.GetSummary().ToString());
			QueryCollection query = classes.Get(0);
			AssertEqualsCs("QueryCollection: test4giis.qacoverapp.AppSimpleJdbc\n" + "  queryNoParameters1Condition\n" + "  queryParameters", query.ToString());
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("qcount=2,qerror=0,count=9,dead=1,error=0", query.GetSummary().ToString());
		}

		// Main invalid situations
		[Test]
		public virtual void TestReaderInvalidFolderNotExist()
		{
			try
			{
				CoverageReader reader = new CoverageReader("pathnotexist");
				reader.GetByClass();
				NUnit.Framework.Legacy.ClassicAssert.Fail("Deberia producirse una excepcion");
			}
			catch (Exception e)
			{
				NUnit.Framework.Legacy.ClassicAssert.AreEqual("Can't browse directory at path pathnotexist", e.Message);
			}
		}

		[Test]
		public virtual void TestReaderInvalidIndexNotExist()
		{
			try
			{
				CoverageReader reader = new CoverageReader(".");
				// folder exists, but no index
				reader.GetHistory();
				NUnit.Framework.Legacy.ClassicAssert.Fail("Deberia producirse una excepcion");
			}
			catch (Exception e)
			{
				AssertContains("Error reading file", e.Message);
				AssertContains("00HISTORY.log", e.Message);
			}
		}

		// Collect the history items to access to the list of parameters used to run
		// each query using a V1 history store format
		/// <exception cref="Java.Sql.SQLException"/>
		/// <exception cref="Java.Text.ParseException"/>
		[Test]
		public virtual void TestHistoryReaderV1()
		{
			//Creates a coverage reader from the already saved v1 files
			CoverageReader reader = new CoverageReader(new Variability().IsJava() ? "src/test/resources/historyV1" : "../../../../../qacover-core/src/test/resources/historyV1");
			HistoryReader all = reader.GetHistory();
			// Reads all classes to get the query keys used to select queries in the history
			CoverageCollection classes = reader.GetByClass();
			NUnit.Framework.Legacy.ClassicAssert.AreEqual(1, classes.Size());
			QueryCollection query = classes.Get(0);
			AssertEqualsCs("test4giis.qacoverapp.appsimplejdbc", query.GetName().ToLower());
			NUnit.Framework.Legacy.ClassicAssert.AreEqual(2, query.Size());
			// two methods
			// First query has only one execution
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("querynoparameters1condition", query.Get(0).GetKey().GetMethodName().ToLower());
			HistoryReader history = all.GetHistoryAtQuery(query.Get(0).GetKey());
			IList<HistoryModel> model = history.GetItems();
			NUnit.Framework.Legacy.ClassicAssert.AreEqual(1, model.Count);
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("<parameters></parameters>", model[0].GetParamsXml());
			// Second query has two executions
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("queryparameters", query.Get(1).GetKey().GetMethodName().ToLower());
			history = all.GetHistoryAtQuery(query.Get(1).GetKey());
			model = history.GetItems();
			NUnit.Framework.Legacy.ClassicAssert.AreEqual(2, model.Count);
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("<parameters><parameter name=\"?1?\" value=\"98\" /><parameter name=\"?2?\" value=\"'abc'\" /></parameters>", model[0].GetParamsXml());
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("<parameters><parameter name=\"?1?\" value=\"98\" /><parameter name=\"?2?\" value=\"'a|c'\" /></parameters>", model[1].GetParamsXml());
			// Invalid query, creates a query key by changing the class name of an existing key
			QueryKey invalid = new QueryKey(query.Get(0).GetKey().GetKey().Replace("AppSimpleJdbc", "InvalidClass"));
			history = all.GetHistoryAtQuery(invalid);
			model = history.GetItems();
			NUnit.Framework.Legacy.ClassicAssert.AreEqual(0, model.Count);
		}

		// Collect the history items to access to the list of parameters used to run
		// each query
		/// <exception cref="Java.Sql.SQLException"/>
		/// <exception cref="Java.Text.ParseException"/>
		[Test]
		public virtual void TestHistoryReader()
		{
			CoverageReader reader = GetCoverageReader();
			HistoryReader all = reader.GetHistory();
			// Reads all classes to get the query keys used to select queries in the history
			CoverageCollection classes = reader.GetByClass();
			NUnit.Framework.Legacy.ClassicAssert.AreEqual(1, classes.Size());
			QueryCollection query = classes.Get(0);
			AssertEqualsCs("test4giis.qacoverapp.appsimplejdbc", query.GetName().ToLower());
			NUnit.Framework.Legacy.ClassicAssert.AreEqual(2, query.Size());
			// two methods
			// First query has only one execution
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("querynoparameters1condition", query.Get(0).GetKey().GetMethodName().ToLower());
			HistoryReader history = all.GetHistoryAtQuery(query.Get(0).GetKey());
			IList<HistoryModel> model = history.GetItems();
			NUnit.Framework.Legacy.ClassicAssert.AreEqual(1, model.Count);
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("[]", model[0].GetParamsJson());
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("#oo", model[0].GetResult());
			// Second query has two executions
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("queryparameters", query.Get(1).GetKey().GetMethodName().ToLower());
			history = all.GetHistoryAtQuery(query.Get(1).GetKey());
			model = history.GetItems();
			NUnit.Framework.Legacy.ClassicAssert.AreEqual(2, model.Count);
			NUnit.Framework.Legacy.ClassicAssert.AreEqual(Javacsparm("[{\"name\":\"?1?\",\"value\":\"98\"},{\"name\":\"?2?\",\"value\":\"'abc'\"}]"), model[0].GetParamsJson());
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("oooooo", model[0].GetResult());
			NUnit.Framework.Legacy.ClassicAssert.AreEqual(Javacsparm("[{\"name\":\"?1?\",\"value\":\"98\"},{\"name\":\"?2?\",\"value\":\"'a|c'\"}]"), model[1].GetParamsJson());
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("oooooo", model[1].GetResult());
			// Invalid query, creates a query key by changing the class name of an existing key
			QueryKey invalid = new QueryKey(query.Get(0).GetKey().GetKey().Replace("AppSimpleJdbc", "InvalidClass"));
			history = all.GetHistoryAtQuery(invalid);
			model = history.GetItems();
			NUnit.Framework.Legacy.ClassicAssert.AreEqual(0, model.Count);
		}

		private string Javacsparm(string @params)
		{
			return new Variability().IsJava() ? @params : @params.Replace("?1?", "@param1").Replace("?2?", "@param2");
		}

		// Collect the source code lines with coverage of queries
		// Basic test of main situations: 
		// - only queries, with source, source not found
		// - path with leading/railing spaces
		// - sorting by line
		// - multiple queries in class, multiple queries in line (tested in TestReport)
		// Integrated test in TestReport
		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestSourceCodeCollection()
		{
			bool isJava = new Variability().IsJava();
			QueryCollection queries = GetCoverageReader().GetByClass().Get(0);
			// (1) Only queries, no source code
			SourceCodeCollection sources = new SourceCodeCollection();
			sources.AddQueries(queries);
			NUnit.Framework.Legacy.ClassicAssert.AreEqual(2, sources.GetLines().Count);
			// Position of each query and basic content
			IList<int> keys = sources.GetLineNumbers();
			int key0 = keys[0];
			int key1 = keys[1];
			NUnit.Framework.Legacy.ClassicAssert.AreEqual(1, sources.GetLines()[key0].GetQueries().Count);
			NUnit.Framework.Legacy.ClassicAssert.AreEqual(1, sources.GetLines()[key1].GetQueries().Count);
			NUnit.Framework.Legacy.ClassicAssert.IsNull(sources.GetLines()[key0].GetSource());
			NUnit.Framework.Legacy.ClassicAssert.IsNull(sources.GetLines()[key1].GetSource());
			// Order of execution: queryParameters queryNoParameters1Condition queryParameters
			// but the second is in a line before the first, it appear at the first position
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("select id,num,text from test where num>=-1", sources.GetLines()[key0].GetQueries()[0].GetSql());
			NUnit.Framework.Legacy.ClassicAssert.AreEqual(isJava ? "SELECT id , num , text FROM test WHERE num > ?1? AND text = ?2?" : "select id,num,text from test where num>@param1 and text=@param2", sources.GetLines()[key1].GetQueries()[0].GetSql());
			// Test location of source code (net stores an absolute path, note that this test is run 4 levels below solution folder)
			NUnit.Framework.Legacy.ClassicAssert.AreEqual(isJava ? "test4giis/qacoverapp/AppSimpleJdbc.java" : FileUtil.GetFullPath("../../../../QACoverTest/Translated/Test4giis.Qacoverapp/AppSimpleJdbc.cs").Replace("\\", "/"), sources.GetLines()[key0].GetQueries()[0].GetModel().GetSourceLocation
				());
			// Source folder/files setup. In net the rules have an absolute path that requires a projectRoot to resolve
			string projectFolder = isJava ? string.Empty : "../../../../../net";
			// this solution root
			string sourceFolder = isJava ? "src/test/java" : "../../../..";
			// in this case sources are just under project root
			string noSourceFolder = isJava ? "src/nosources" : "../../../../../otherproject/QACoverTest";
			// (2) Add source code (found in second path, that requires trim), now there is source content and coverage
			sources.AddSources(queries.Get(0), noSourceFolder + ", " + sourceFolder + " ", projectFolder);
			NUnit.Framework.Legacy.ClassicAssert.AreEqual(1, sources.GetLines()[key0].GetQueries().Count);
			NUnit.Framework.Legacy.ClassicAssert.AreEqual(1, sources.GetLines()[key1].GetQueries().Count);
			NUnit.Framework.Legacy.ClassicAssert.IsNotNull(sources.GetLines()[key0].GetSource());
			NUnit.Framework.Legacy.ClassicAssert.IsNotNull(sources.GetLines()[key1].GetSource());
			// Check first and last line, with source content, without coverage
			int numLines = sources.GetLines().Count;
			NUnit.Framework.Legacy.ClassicAssert.AreEqual(0, sources.GetLines()[1].GetQueries().Count);
			NUnit.Framework.Legacy.ClassicAssert.IsNotNull(sources.GetLines()[1]);
			NUnit.Framework.Legacy.ClassicAssert.AreEqual(0, sources.GetLines()[numLines].GetQueries().Count);
			NUnit.Framework.Legacy.ClassicAssert.IsNotNull(sources.GetLines()[numLines]);
			// (3) Source code can't be located at any file
			// Queries are already generated, reset the sources
			sources = new SourceCodeCollection();
			sources.AddQueries(queries);
			sources.AddSources(queries.Get(0), noSourceFolder, projectFolder);
			// Same checks than at the beginning of this test
			NUnit.Framework.Legacy.ClassicAssert.AreEqual(2, sources.GetLines().Count);
			NUnit.Framework.Legacy.ClassicAssert.AreEqual(1, sources.GetLines()[key0].GetQueries().Count);
			NUnit.Framework.Legacy.ClassicAssert.AreEqual(1, sources.GetLines()[key1].GetQueries().Count);
			NUnit.Framework.Legacy.ClassicAssert.IsNull(sources.GetLines()[key0].GetSource());
			NUnit.Framework.Legacy.ClassicAssert.IsNull(sources.GetLines()[key1].GetSource());
		}
	}
}
