/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using Giis.Portable.Util;
using Giis.Qacover.Core.Services;
using Giis.Qacover.Model;
using NLog;
using NUnit.Framework;

using Test4giis.Qacoverapp;

namespace Test4giis.Qacover
{
	/// <summary>
	/// Scenarios to check the storage of the rule evaluation
	/// Situations (* estan cubiertas):
	/// - Evaluation (parameter values): *query with different parameters / *different queries
	/// - Evaluation (coverage increase): *no incremento, *incremento, *varios
	/// - Store persistence: *new instance preserves previous results (default) / *does not preserves (after drop)
	/// - Location of different queries: *same line / *same method / *different method and class
	/// - Location of equal queries: *same line / *same method / different method, different class (always different provided that they are in differen lines)
	/// - Query exclusion: by partial mach / all / not excluded
	/// - Table query exclusion: excluded total match / not excluded partial match
	/// </summary>
	public class TestStore : Base
	{
		private static readonly Logger log = NLogUtil.GetLogger(typeof(TestStore));

		private AppSimpleJdbc app;

		/// <exception cref="Java.Sql.SQLException"/>
		[NUnit.Framework.SetUp]
		public override void SetUp()
		{
			base.SetUp();
			options.SetRuleOptions("noboundaries");
			app = new AppSimpleJdbc(variant);
			// aplicacion con los metodos a probar
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
			app.ExecuteUpdateNative(new string[] { "create table test(id int not null, num int not null, text varchar(16))", "insert into test(id,num,text) values(1,99,'xyz')" });
		}

		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestScenarioParametrized()
		{
			// also covers different queries, different methods, more than an increment in a rule
			rs = app.QueryParameters(90, "nnn");
			StoreService store = StoreService.GetLast();
			string firstFile = store.GetLastSavedQueryKey();
			log.Debug("Saved to file: " + store.GetLastSavedQueryKey());
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("count=4,dead=1\ncount=1,dead=0\ncount=1,dead=0\ncount=1,dead=1\ncount=1,dead=0", store.GetQueryModel(firstFile).GetTextSummary());
			rs.Close();
			rs = app.QueryParameters(90, "nnn");
			store = StoreService.GetLast();
			log.Debug("Saved to file: " + store.GetLastSavedQueryKey());
			NUnit.Framework.Legacy.ClassicAssert.AreEqual(firstFile, store.GetLastSavedQueryKey());
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("count=4,dead=1\ncount=2,dead=0\ncount=2,dead=0\ncount=2,dead=2\ncount=2,dead=0", store.GetQueryModel(firstFile).GetTextSummary());
			// increment a rule
			rs = app.QueryParameters(90, "xyz");
			store = StoreService.GetLast();
			log.Debug("Saved to file: " + store.GetLastSavedQueryKey());
			NUnit.Framework.Legacy.ClassicAssert.AreEqual(firstFile, store.GetLastSavedQueryKey());
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("count=4,dead=2\ncount=3,dead=1\ncount=3,dead=0\ncount=3,dead=2\ncount=3,dead=0", store.GetQueryModel(firstFile).GetTextSummary());
			rs.Close();
			// different query, another summary an file
			rs = app.QueryNoParameters1Condition(99);
			store = StoreService.GetLast();
			log.Debug("Saved to file: " + store.GetLastSavedQueryKey());
			string secondFile = store.GetLastSavedQueryKey();
			NUnit.Framework.Legacy.ClassicAssert.IsFalse(firstFile.Equals(store.GetLastSavedQueryKey()));
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("count=2,dead=1\n" + "count=1,dead=0\ncount=1,dead=1", store.GetQueryModel(secondFile).GetTextSummary());
			// still can read the first file
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("count=4,dead=2\n" + "count=3,dead=1\ncount=3,dead=0\ncount=3,dead=2\ncount=3,dead=0", store.GetQueryModel(firstFile).GetTextSummary());
			rs.Close();
		}

		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestScenarioNoParametrized()
		{
			// simplified version of the above without parameters
			Configuration.GetInstance().SetInferQueryParameters(true);
			rs = app.QueryNoParameters2Condition(90, "nnn");
			StoreService store = StoreService.GetLast();
			string firstFile = store.GetLastSavedQueryKey();
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("count=4,dead=1\ncount=1,dead=0\ncount=1,dead=0\ncount=1,dead=1\ncount=1,dead=0", store.GetQueryModel(firstFile).GetTextSummary());
			rs.Close();
			rs = app.QueryNoParameters2Condition(90, "nnn");
			store = StoreService.GetLast();
			NUnit.Framework.Legacy.ClassicAssert.AreEqual(firstFile, store.GetLastSavedQueryKey());
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("count=4,dead=1\ncount=2,dead=0\ncount=2,dead=0\ncount=2,dead=2\ncount=2,dead=0", store.GetQueryModel(firstFile).GetTextSummary());
			rs = app.QueryNoParameters2Condition(90, "xyz");
			store = StoreService.GetLast();
			NUnit.Framework.Legacy.ClassicAssert.AreEqual(firstFile, store.GetLastSavedQueryKey());
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("count=4,dead=2\ncount=3,dead=1\ncount=3,dead=0\ncount=3,dead=2\ncount=3,dead=0", store.GetQueryModel(firstFile).GetTextSummary());
			rs.Close();
		}

		// Based on the first step of testScenarioParametrized, with optimization to do not re-execute rules already covered
		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestScenarioOptimized()
		{
			Configuration.GetInstance().SetOptimizeRuleEvaluation(true);
			rs = app.QueryParameters(90, "nnn");
			// solo 3rd rule is covered from the beginning as in testScenarioParametrized
			rs.Close();
			app.ExecuteUpdateNative(new string[] { "insert into test(id,num,text) values(1,99,'nnn')" });
			rs = app.QueryParameters(90, "nnn");
			// this also covers 1st
			rs.Close();
			rs = app.QueryParameters(90, "nnn");
			rs.Close();
			StoreService store = StoreService.GetLast();
			string firstFile = store.GetLastSavedQueryKey();
			// without optimization, the result would be:
			// count=4,dead=2\ncount=3,dead=2\ncount=3,dead=0\ncount=3,dead=3\ncount=3,dead=0
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("count=4,dead=2\ncount=2,dead=1\ncount=3,dead=0\ncount=1,dead=1\ncount=3,dead=0", store.GetQueryModel(firstFile).GetTextSummary());
		}

		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestScenarioPersistence()
		{
			Configuration.GetInstance().SetInferQueryParameters(true);
			rs = app.QueryNoParameters2Condition(90, "nnn");
			StoreService store = StoreService.GetLast();
			string firstFile = store.GetLastSavedQueryKey();
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("count=4,dead=1\ncount=1,dead=0\ncount=1,dead=0\ncount=1,dead=1\ncount=1,dead=0", store.GetQueryModel(firstFile).GetTextSummary());
			rs.Close();
			// close here to allow a new rs
			// a new instance preserves previous coverage data
			store.DropLast();
			rs = app.QueryNoParameters2Condition(90, "nnn");
			store = StoreService.GetLast();
			NUnit.Framework.Legacy.ClassicAssert.AreEqual(firstFile, store.GetLastSavedQueryKey());
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("count=4,dead=1\ncount=2,dead=0\ncount=2,dead=0\ncount=2,dead=2\ncount=2,dead=0", store.GetQueryModel(firstFile).GetTextSummary());
			// but drop does not preserves any coverage data
			StoreService.GetLast().DropRules();
			store.DropLast();
			rs = app.QueryNoParameters2Condition(90, "nnn");
			store = StoreService.GetLast();
			NUnit.Framework.Legacy.ClassicAssert.AreEqual(firstFile, store.GetLastSavedQueryKey());
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("count=4,dead=1\ncount=1,dead=0\ncount=1,dead=0\ncount=1,dead=1\ncount=1,dead=0", store.GetQueryModel(firstFile).GetTextSummary());
			StoreService.GetLast().DropRules();
			store.DropLast();
			rs = app.QueryNoParameters2Condition(90, "xyz");
			store = StoreService.GetLast();
			NUnit.Framework.Legacy.ClassicAssert.AreEqual(firstFile, store.GetLastSavedQueryKey());
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("count=4,dead=1\ncount=1,dead=1\ncount=1,dead=0\ncount=1,dead=0\ncount=1,dead=0", store.GetQueryModel(firstFile).GetTextSummary());
			rs.Close();
		}

		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestDifferentQueriesSameLine()
		{
			// runs first time only to get the file name for further comparison
			app.QueryDifferentSingleLine(true, 99, false, string.Empty);
			StoreService store = StoreService.GetLast();
			string firstFile = store.GetLastSavedQueryKey();
			log.Debug("Saved to file: " + store.GetLastSavedQueryKey());
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("count=2,dead=1\ncount=1,dead=0\ncount=1,dead=1", store.GetQueryModel(firstFile).GetTextSummary());
			app.QueryDifferentSingleLine(true, 99, true, "'xyz'");
			store = StoreService.GetLast();
			string secondFile = store.GetLastSavedQueryKey();
			log.Debug("Saved to file (second): " + store.GetLastSavedQueryKey());
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("count=2,dead=1\ncount=2,dead=0\ncount=2,dead=2", store.GetQueryModel(firstFile).GetTextSummary());
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("count=3,dead=1\ncount=1,dead=0\ncount=1,dead=1\ncount=1,dead=0", store.GetQueryModel(secondFile).GetTextSummary());
		}

		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestSameQueriesSameLine()
		{
			if (new Variability().IsNetCore())
			{
				// ignored for .net (sharpen sets statements in different lines)
				return;
			}
			Configuration.GetInstance().SetInferQueryParameters(true);
			// to consider the same parametrized qeury
			app.QueryEqualSingleLine("'xyz'", "'aaa'");
			StoreService store = StoreService.GetLast();
			string firstFile = StoreService.GetLast().GetLastSavedQueryKey();
			log.Debug("Saved to file: " + firstFile);
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("count=3,dead=2\ncount=2,dead=1\ncount=2,dead=1\ncount=2,dead=0", store.GetQueryModel(firstFile).GetTextSummary());
		}

		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestDifferentQueriesDifferentLine()
		{
			app.QueryEqualDifferentLine("'xyz'", "'aaa'");
			StoreService store = StoreService.GetLast();
			// only compares the last
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("count=3,dead=1\ncount=1,dead=1\ncount=1,dead=0\ncount=1,dead=0", store.GetQueryModel(store.GetLastSavedQueryKey()).GetTextSummary());
		}

		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestSameQueriesDifferentLine()
		{
			Configuration.GetInstance().SetInferQueryParameters(true);
			// para que sean consideradas las dos queries como
			// una misma con parametros
			app.QueryEqualDifferentLine("'xyz'", "'aaa'");
			StoreService store = StoreService.GetLast();
			// only compares the last
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("count=3,dead=1\ncount=1,dead=1\ncount=1,dead=0\ncount=1,dead=0", store.GetQueryModel(store.GetLastSavedQueryKey()).GetTextSummary());
		}

		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestAbortByTableExclusion()
		{
			// full match (implies an abort on the rule evaluation)
			Base.ConfigureTestOptions().SetRuleOptions("noboundaries").AddTableExclusionExact("test");
			rs = app.QueryNoParameters1Condition(-1);
			AssertAbort("{}");
			// approximate match (no abort)
			Base.ConfigureTestOptions().SetRuleOptions("noboundaries").AddTableExclusionExact("tes");
			rs = app.QueryNoParameters1Condition(-1);
			AssertNoAbort();
		}

		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestAbortByClassExclusion()
		{
			// full match (abort)
			Base.ConfigureTestOptions().SetRuleOptions("noboundaries").AddClassExclusion("test4giis.qacoverapp.AppSimpleJdbc");
			rs = app.QueryNoParameters1Condition(-1);
			AssertAbort(string.Empty);
			// partial match
			Base.ConfigureTestOptions().SetRuleOptions("noboundaries").AddClassExclusion("test4giis.qacoverapp.AppSimpleJdb");
			rs = app.QueryNoParameters1Condition(-1);
			AssertAbort(string.Empty);
			// no match, normal processing
			Base.ConfigureTestOptions().SetRuleOptions("noboundaries").AddClassExclusion("test4giis.qacoverapp.AppSimpleJdbx");
			rs = app.QueryNoParameters1Condition(-1);
			AssertNoAbort();
		}

		// Check if a query has been processed (evaluated)
		private void AssertAbort(string expParams)
		{
			AssertEvalResults(false, string.Empty, "1 99 xyz", SqlUtil.ResultSet2csv(rs, " "), string.Empty, expParams, false, false);
			// falta por comprobar el ultimo fichero guardado
			StoreService store = StoreService.GetLast();
			NUnit.Framework.Legacy.ClassicAssert.AreEqual(string.Empty, store.GetLastSavedQueryKey());
		}

		private void AssertNoAbort()
		{
			AssertEvalResults(true, "select id,num,text from test where num>=-1", "1 99 xyz", SqlUtil.ResultSet2csv(rs, " "), "UNCOVERED SELECT id , num , text FROM test WHERE NOT(num >= -1)\n" + "COVERED   SELECT id , num , text FROM test WHERE (num >= -1)", "{}", false, false);
			StoreService store = StoreService.GetLast();
			string firstFile = store.GetLastSavedQueryKey();
			log.Debug("Saved to file: " + store.GetLastSavedQueryKey());
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("count=2,dead=1\ncount=1,dead=0\ncount=1,dead=1", store.GetQueryModel(firstFile).GetTextSummary());
		}
	}
}
