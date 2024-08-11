/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using Giis.Portable.Util;
using Giis.Qacover.Core;
using Giis.Qacover.Core.Services;
using Giis.Qacover.Model;
using Giis.Qacover.Portable;
using NUnit.Framework;

using Test4giis.Qacoverapp;

namespace Test4giis.Qacover
{
	/// <summary>
	/// Behaviour when something fails or query can't be executed
	/// Comportamiento cuando no se puede ejecutar parte de la funcionalidad de una query
	/// Situations (* are covered):
	/// - Unexpected failure: *connecting the service / *internal exception
	/// - SQL failures (generated in the driver): *wrong sql syntax / *table is not in the schema
	/// - SQL parameter falures: more / less parameters than needed
	/// - Failures notified by the core.services: *inferring parameters / *geting tables of the query / *getting schema / *generating rules
	/// - Failure in a rule execution: *(rule is not invalidated, but is marked as error
	/// - Multiple executions, same query with fault: *same fault / *different fault
	/// - Multiple executions, same rule with fault: *same fault / *different fault
	/// Extensive use of the FaultInjector service,
	/// and old fashion exception handling for C# compatibility
	/// </summary>
	public class TestFaults : Base
	{
		private AppSimpleJdbc3Errors app;

		/// <exception cref="Java.Sql.SQLException"/>
		[NUnit.Framework.SetUp]
		public override void SetUp()
		{
			base.SetUp();
			options.SetFpcServiceOptions("noboundaries");
			app = new AppSimpleJdbc3Errors(variant);
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
			app.ExecuteUpdateNative(new string[] { "drop table if exists test", "create table test(id int not null, num int not null, text varchar(16))", "insert into test(id,num,text) values(1,99,'xyz')" });
		}

		private void AssertExceptionMessage(string expected, string actual)
		{
			// To make easier comparison and compatible with .net uses ignorecase and partial match (from the beginning)
			int count = expected.Length;
			actual = actual.Replace("\r", string.Empty);
			string compareTo = actual.Length >= count ? JavaCs.Substring(actual, 0, count) : actual;
			if (!Contains(compareTo.ToLower(), expected.ToLower()))
			{
				if (new Variability().IsJava())
				{
					NUnit.Framework.Legacy.ClassicAssert.AreEqual("Expected not included in actual.", expected, actual);
				}
				else
				{
					NUnit.Framework.Legacy.ClassicAssert.AreEqual(expected, actual, "Expected not included in actual.");
				}
			}
		}

		private bool Contains(string text, string substring)
		{
			return text.IndexOf(substring) != -1;
		}

		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestFaultConnectingService()
		{
			options.SetFpcServiceUrl("http://giis.uniovi.es/noexiste.xml").SetCacheRulesLocation(string.Empty);
			// disable cache to run the actual service
			rs = app.ExecuteQuery("select id,num,text from test where num<9");
			AssertExceptionMessage(new Variability().IsJava() ? "Error at Get query table names: ApiException" : "Error at Get query table names: Giis.Tdrules.Openapi.Client.ApiException: Error calling QueryEntitiesPost", StoreService.GetLast().GetLastGenStatus());
		}

		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestFaultUnexpectedException()
		{
			QueryStatement.SetFaultInjector(new FaultInjector().SetUnexpectedException("Injected unexpected exception"));
			rs = app.ExecuteQuery("select id,num,text from test where num<9");
			AssertExceptionMessage("Error at : giis.qacover.portable.QaCoverException: Injected unexpected exception", StoreService.GetLast().GetLastGenStatus());
		}

		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestFaultJdbcSqlSyntax()
		{
			try
			{
				rs = app.ExecuteQuery("select id,num,text from test where num lt 9");
				NUnit.Framework.Legacy.ClassicAssert.Fail("se esperaba excepcion");
			}
			catch (Exception e)
			{
				AssertExceptionMessage(variant.IsJava() ? (variant.IsJava8() ? "org.sqlite.SQLiteException: [SQLITE_ERROR] SQL error or missing database (near \"lt\": syntax error)" : "org.h2.jdbc.JdbcSQLException: Syntax error in SQL statement") : "System.Data.SQLite.SQLiteException: SQL logic error\nnear \"lt\": syntax error"
					, QaCoverException.GetString(e));
			}
		}

		//: "Microsoft.Data.Sqlite.SqliteException: SQLite Error 1: 'near \"lt\": syntax error'."
		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestFaultJdbcTableNotExists()
		{
			try
			{
				rs = app.ExecuteQuery("select id,num,text from noexiste where num<9");
				NUnit.Framework.Legacy.ClassicAssert.Fail("se esperaba excepcion");
			}
			catch (Exception e)
			{
				AssertExceptionMessage(variant.IsJava() ? (variant.IsJava8() ? "org.sqlite.SQLiteException: [SQLITE_ERROR] SQL error or missing database (no such table: noexiste)" : "org.h2.jdbc.JdbcSQLException: Table NOEXISTE not found") : "System.Data.SQLite.SQLiteException: SQL logic error\nno such table: noexiste"
					, QaCoverException.GetString(e));
			}
		}

		//: "Microsoft.Data.Sqlite.SqliteException: SQLite Error 1: 'no such table: noexiste'."
		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestFaultServiceInferParameters()
		{
			options.SetInferQueryParameters(true);
			QueryStatement.SetFaultInjector(new FaultInjector().SetInferFaulty("Injected fault at infer"));
			rs = app.ExecuteQuery("select id,num,text from test where num < 9");
			AssertExceptionMessage("Error at Infer query parameters: giis.qacover.portable.QaCoverException: Injected fault at infer", StoreService.GetLast().GetLastGenStatus());
		}

		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestFaultServiceGetTables()
		{
			QueryStatement.SetFaultInjector(new FaultInjector().SetTablesFaulty("Injected fault at get tables"));
			rs = app.ExecuteQuery("select id,num,text from test where num < 9");
			AssertExceptionMessage("Error at Get query table names: giis.qacover.portable.QaCoverException: Injected fault at get tables", StoreService.GetLast().GetLastGenStatus());
		}

		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestFaultServiceGetSchema()
		{
			QueryStatement.SetFaultInjector(new FaultInjector().SetSchemaFaulty("select id,num,text from noexiste where num < 9"));
			rs = app.ExecuteQuery("select id,num,text from test where num < 9");
			string msg = "Error at Get database schema: giis.tdrules.store.rdb.SchemaException: SchemaReaderJdbc.setTableType: Can't find table or view: noexiste";
			if (new Variability().IsNetCore())
			{
				//en .net el namespace se renombra para evitar conflictos on otros paquetes
				msg = msg.Replace("giis.util", "giis.qacover.util");
			}
			AssertExceptionMessage(msg, StoreService.GetLast().GetLastGenStatus());
		}

		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestFaultServiceGetRules()
		{
			QueryStatement.SetFaultInjector(new FaultInjector().SetRulesFaulty("Injected fault at rules"));
			rs = app.ExecuteQuery("select id,num,text from test where num < 9");
			AssertExceptionMessage("Error at Generate SQLFpc coverage rules: giis.qacover.portable.QaCoverException: Injected fault at rules", StoreService.GetLast().GetLastGenStatus());
		}

		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestFaultServiceRunRules()
		{
			QueryStatement.SetFaultInjector(new FaultInjector().SetSingleRuleFaulty("selectar id,num,text from test where num < 9"));
			rs = app.ExecuteQuery("select id,num,text from test where num < 9");
			rs.Close();
			// Result is success because a single rule has failed, not the query
			AssertExceptionMessage("success", StoreService.GetLast().GetLastGenStatus());
			// More checks
			QueryModel model = StoreService.GetLast().GetQueryModel(StoreService.GetLast().GetLastSavedQueryKey());
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("count=2,dead=0,error=1\n" + "count=1,dead=0,error=1\n" + "count=1,dead=0", model.GetTextSummary());
			// Rule with fault is the first one, checks its message
			IList<RuleModel> rules = model.GetRules();
			//NOSONAR no using typed names for compatibility with downgrade to jdk 1.4
			string errorString = rules[0].GetErrorString();
			AssertExceptionMessage(variant.IsJava() ? (variant.IsJava8() ? "giis.qacover.portable.QaCoverException: QueryReader.hasRows. Caused by: org.sqlite.SQLiteException: [SQLITE_ERROR] SQL error or missing database (near \"selectar\": syntax error)" : "giis.qacover.portable.QaCoverException: QueryReader.hasRows. Caused by: org.h2.jdbc.JdbcSQLException: Syntax error in SQL statement"
				) : "Giis.Qacover.Portable.QaCoverException: QueryReader.hasRows. Caused by: System.Data.SQLite.SQLiteException: SQL logic error\nnear \"selectar\": syntax error", errorString);
		}

		//: "giis.Qacover.Portable.QaCoverException: SpyStatementAdapter.hasRows. Caused by: Microsoft.Data.Sqlite.SqliteException: SQLite Error 1: 'near \"selectar\": syntax error'."
		// Multiple execution scenarios
		//  - same query with fault: same fault / different fault
		//  - same rule with fault: same fault / different fault
		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestMultipleFaultsQuery()
		{
			//primer error en la query, comprueba tanto los cotnadores como el texto de error
			QueryStatement.SetFaultInjector(new FaultInjector().SetTablesFaulty("Injected fault at get tables"));
			rs = app.QueryMultipleErrors();
			rs.Close();
			AssertMultipleFaults(true, "qerror=1,count=0,dead=0", "Error at Get query table names: giis.qacover.portable.QaCoverException: Injected fault at get tables");
			//segundo error del mismo texto, nada cambia
			QueryStatement.SetFaultInjector(new FaultInjector().SetTablesFaulty("Injected fault at get tables"));
			rs = app.QueryMultipleErrors();
			rs.Close();
			AssertMultipleFaults(true, "qerror=1,count=0,dead=0", "Error at Get query table names: giis.qacover.portable.QaCoverException: Injected fault at get tables");
			//otro error pero con diferente texto, remplaza al anterior
			QueryStatement.SetFaultInjector(new FaultInjector().SetUnexpectedException("Injected unexpected exception"));
			rs = app.QueryMultipleErrors();
			rs.Close();
			AssertMultipleFaults(true, "qerror=1,count=0,dead=0", "Error at : giis.qacover.portable.QaCoverException: Injected unexpected exception");
		}

		internal string exceptionInvalidSql = new Variability().IsJava() ? (new Variability().IsJava8() ? "giis.qacover.portable.QaCoverException: QueryReader.hasRows. Caused by: org.sqlite.SQLiteException: [SQLITE_ERROR] SQL error or missing database (near \"selectar\": syntax error)" : 
			"giis.qacover.portable.QaCoverException: QueryReader.hasRows. Caused by: org.h2.jdbc.JdbcSQLException: Syntax error in SQL statement") : "Giis.Qacover.Portable.QaCoverException: QueryReader.hasRows. Caused by: System.Data.SQLite.SQLiteException: SQL logic error\nnear \"selectar\": syntax error";

		//: "giis.Qacover.Portable.QaCoverException: SpyStatementAdapter.hasRows. Caused by: Microsoft.Data.Sqlite.SqliteException: SQLite Error 1: 'near \"selectar\": syntax error'."
		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestMultipleFaultsRule()
		{
			//primer error en la query, comprueba tanto los contadores como el texto de error
			QueryStatement.SetFaultInjector(new FaultInjector().SetSingleRuleFaulty("selectar id,num,text from test where num < 9"));
			rs = app.QueryMultipleErrors();
			rs.Close();
			AssertMultipleFaults(false, "count=2,dead=0,error=1\n" + "count=1,dead=0,error=1\n" + "count=1,dead=0", exceptionInvalidSql);
			//mismo error, no cambia el mensaje, aunque si los contadores de las reglas individuales
			QueryStatement.SetFaultInjector(new FaultInjector().SetSingleRuleFaulty("selectar id,num,text from test where num < 9"));
			rs = app.QueryMultipleErrors();
			rs.Close();
			AssertMultipleFaults(false, "count=2,dead=0,error=1\n" + "count=2,dead=0,error=2\n" + "count=2,dead=0", exceptionInvalidSql);
			//otro error differente, ademas de incrementar contadores, anyade el mensaje
			QueryStatement.SetFaultInjector(new FaultInjector().SetSingleRuleFaulty("select id,num,text from notable where num < 9"));
			rs = app.QueryMultipleErrors();
			rs.Close();
			AssertMultipleFaults(false, "count=2,dead=0,error=1\n" + "count=3,dead=0,error=3\n" + "count=3,dead=0", exceptionInvalidSql);
			//esto no impide que otras reglas se cubran, modifico la bd para que se cubra la segunda, esta seguira fallando pues hemos modificado el sql de la regla
			QueryStatement.SetFaultInjector(new FaultInjector().SetSingleRuleFaulty("select id,num,text from notable where num < 9"));
			app.ExecuteUpdateNative(new string[] { "update test set num=5 where id=1" });
			rs = app.QueryMultipleErrors();
			rs.Close();
			AssertMultipleFaults(false, "count=2,dead=1,error=1\n" + "count=4,dead=0,error=4\n" + "count=4,dead=1", exceptionInvalidSql);
		}

		private void AssertMultipleFaults(bool checkErrorsAtQuery, string expectedSummary, string expectedErrorString)
		{
			//Obiene las regas del almacenamiento y comprueba el resumen
			QueryModel model = StoreService.GetLast().GetQueryModel(StoreService.GetLast().GetLastSavedQueryKey());
			NUnit.Framework.Legacy.ClassicAssert.AreEqual(expectedSummary, model.GetTextSummary());
			//texto detallado de los errores
			if (checkErrorsAtQuery)
			{
				string errorString = model.GetErrorString();
				AssertExceptionMessage(expectedErrorString, errorString);
			}
			else
			{
				//la que tiene fallo inyectado es la primera, comprueba el mensaje
				IList<RuleModel> rules = model.GetRules();
				string errorString = rules[0].GetErrorString();
				AssertExceptionMessage(expectedErrorString, errorString);
			}
		}
	}
}
