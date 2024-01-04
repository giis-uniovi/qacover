package test4giis.qacover;

import giis.portable.util.JavaCs;

import giis.qacover.core.QueryStatement;
import giis.qacover.core.services.FaultInjector;
import giis.qacover.core.services.StoreService;
import giis.qacover.model.QueryModel;
import giis.qacover.model.RuleModel;
import giis.qacover.model.Variability;
import giis.qacover.portable.QaCoverException;
import test4giis.qacoverapp.AppSimpleJdbc3Errors;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.fail;

import java.sql.SQLException;
import java.util.List;

import org.junit.After;
import org.junit.Before;
import org.junit.Test;

/**
 * Behaviour when something fails or query can't be executed
 * Comportamiento cuando no se puede ejecutar parte de la funcionalidad de una query
 * Situations (* are covered):
 *  - Unexpected failure: *connecting the service / *internal exception
 *  - SQL failures (generated in the driver): *wrong sql syntax / *table is not in the schema
 *  - SQL parameter falures: more / less parameters than needed 
 *  - Failures notified by the core.services: *inferring parameters / *geting tables of the query / *getting schema / *generating rules
 *  - Failure in a rule execution: *(rule is not invalidated, but is marked as error
 *  - Multiple executions, same query with fault: *same fault / *different fault
 *  - Multiple executions, same rule with fault: *same fault / *different fault
 *  
 * Extensive use of the FaultInjector service, 
 * and old fashion exception handling for C# compatibility
 */
public class TestFaults extends Base {
	private AppSimpleJdbc3Errors app;

	@Before
	public void setUp() throws SQLException {
		super.setUp();
		options.setFpcServiceOptions("noboundaries");
		app = new AppSimpleJdbc3Errors(variant);
		setUpTestData();
	}

	@After
	public void tearDown() throws SQLException {
		super.tearDown();
		app.close();
	}

	public void setUpTestData() {
		app.executeUpdateNative(new String[] { "drop table if exists test",
				"create table test(id int not null, num int not null, text varchar(16))",
				"insert into test(id,num,text) values(1,99,'xyz')" });
	}

	private void assertExceptionMessage(String expected, String actual) {
		// To make easier comparison and compatible with .net uses ignorecase and partial match (from the beginning)
		int count = expected.length();
		actual = actual.replace("\r","");
		String compareTo = actual.length() >= count ? JavaCs.substring(actual, 0, count) : actual;
		if (!contains(compareTo.toLowerCase(), expected.toLowerCase())) {
			if (new Variability().isJava())
				assertEquals("Expected not included in actual.", expected, actual);
			else
				assertEquals(expected, actual, "Expected not included in actual.");
		}
	}
	private boolean contains(String text, String substring) {
		return text.indexOf(substring) != -1;
	}

	@Test
	public void testFaultConnectingService() throws SQLException {
		options.setFpcServiceUrl("http://giis.uniovi.es/noexiste.xml")
			.setCacheRulesLocation(""); // disable cache to run the actual service
		rs=app.executeQuery("select id,num,text from test where num<9");
		assertExceptionMessage(new Variability().isJava()
				? "Error at Get query table names: ApiException"
				: "Error at Get query table names: Giis.Tdrules.Openapi.Client.ApiException: Error calling QueryEntitiesPost"
				, StoreService.getLast().getLastGenStatus());
	}
	@Test
	public void testFaultUnexpectedException() throws SQLException {
		QueryStatement.setFaultInjector(new FaultInjector().setUnexpectedException("Injected unexpected exception"));
		rs = app.executeQuery("select id,num,text from test where num<9");
		assertExceptionMessage("Error at : giis.qacover.portable.QaCoverException: Injected unexpected exception",
				StoreService.getLast().getLastGenStatus());
	}
				
	@Test
	public void testFaultJdbcSqlSyntax() throws SQLException {
		try {
			rs = app.executeQuery("select id,num,text from test where num lt 9");
			fail("se esperaba excepcion");
		} catch (Exception e) {
			assertExceptionMessage(variant.isJava()
					? (variant.isJava8() ? "org.sqlite.SQLiteException: [SQLITE_ERROR] SQL error or missing database (near \"lt\": syntax error)"
									   : "org.h2.jdbc.JdbcSQLException: Syntax error in SQL statement")
					: "System.Data.SQLite.SQLiteException: SQL logic error\nnear \"lt\": syntax error"
					//: "Microsoft.Data.Sqlite.SqliteException: SQLite Error 1: 'near \"lt\": syntax error'."
					, QaCoverException.getString(e));
		}
	}

	@Test
	public void testFaultJdbcTableNotExists() throws SQLException {
		try {
			rs = app.executeQuery("select id,num,text from noexiste where num<9");
			fail("se esperaba excepcion");
		} catch (Exception e) {
			assertExceptionMessage(variant.isJava()
					? (variant.isJava8() ? "org.sqlite.SQLiteException: [SQLITE_ERROR] SQL error or missing database (no such table: noexiste)"
							   			: "org.h2.jdbc.JdbcSQLException: Table NOEXISTE not found")
					: "System.Data.SQLite.SQLiteException: SQL logic error\nno such table: noexiste"
					//: "Microsoft.Data.Sqlite.SqliteException: SQLite Error 1: 'no such table: noexiste'."
					, QaCoverException.getString(e));
		}
	}

	@Test
	public void testFaultServiceInferParameters() throws SQLException {
		options.setInferQueryParameters(true);
		QueryStatement.setFaultInjector(new FaultInjector().setInferFaulty("Injected fault at infer"));
		rs = app.executeQuery("select id,num,text from test where num < 9");
		assertExceptionMessage("Error at Infer query parameters: giis.qacover.portable.QaCoverException: Injected fault at infer", 
				StoreService.getLast().getLastGenStatus());
	}
	
	@Test
	public void testFaultServiceGetTables() throws SQLException {
		QueryStatement.setFaultInjector(new FaultInjector().setTablesFaulty("Injected fault at get tables"));
		rs = app.executeQuery("select id,num,text from test where num < 9");
		assertExceptionMessage("Error at Get query table names: giis.qacover.portable.QaCoverException: Injected fault at get tables", 
				StoreService.getLast().getLastGenStatus());
	}
	
	@Test
	public void testFaultServiceGetSchema() throws SQLException {
		QueryStatement.setFaultInjector(new FaultInjector().setSchemaFaulty("select id,num,text from noexiste where num < 9"));
		rs = app.executeQuery("select id,num,text from test where num < 9");
		String msg="Error at Get database schema: giis.tdrules.store.rdb.SchemaException: SchemaReaderJdbc.setTableType: Can't find table or view: noexiste";
		if (new Variability().isNetCore()) //en .net el namespace se renombra para evitar conflictos on otros paquetes
			msg=msg.replace("giis.util", "giis.qacover.util");
		assertExceptionMessage(msg, 
				StoreService.getLast().getLastGenStatus());
	}
	
	@Test
	public void testFaultServiceGetRules() throws SQLException {
		QueryStatement.setFaultInjector(new FaultInjector().setRulesFaulty("Injected fault at rules"));
		rs = app.executeQuery("select id,num,text from test where num < 9");
		assertExceptionMessage("Error at Generate SQLFpc coverage rules: giis.qacover.portable.QaCoverException: Injected fault at rules", 
				StoreService.getLast().getLastGenStatus());
	}
	
	@Test
	public void testFaultServiceRunRules() throws SQLException {
		QueryStatement.setFaultInjector(new FaultInjector().setSingleRuleFaulty("selectar id,num,text from test where num < 9"));
		rs = app.executeQuery("select id,num,text from test where num < 9");
		rs.close();
		// Result is success because a single rule has failed, not the query
		assertExceptionMessage("success", StoreService.getLast().getLastGenStatus());
		// More checks
		QueryModel model=StoreService.getLast().getQueryModel(StoreService.getLast().getLastSavedQueryKey());
		assertEquals("count=2,dead=0,error=1\n" + 
				"count=1,dead=0,error=1\n" + 
				"count=1,dead=0", model.getTextSummary());
		// Rule with fault is the first one, checks its message
		List<RuleModel> rules=model.getRules(); //NOSONAR no using typed names for compatibility with downgrade to jdk 1.4
		String errorString=rules.get(0).getErrorString();
		assertExceptionMessage(variant.isJava()
				? (variant.isJava8() ? "giis.qacover.portable.QaCoverException: SpyStatementAdapter.hasRows. Caused by: org.sqlite.SQLiteException: [SQLITE_ERROR] SQL error or missing database (near \"selectar\": syntax error)"
						   : "giis.qacover.portable.QaCoverException: SpyStatementAdapter.hasRows. Caused by: org.h2.jdbc.JdbcSQLException: Syntax error in SQL statement")
				: "Giis.Qacover.Portable.QaCoverException: SpyStatementAdapter.hasRows. Caused by: System.Data.SQLite.SQLiteException: SQL logic error\nnear \"selectar\": syntax error"
				//: "giis.Qacover.Portable.QaCoverException: SpyStatementAdapter.hasRows. Caused by: Microsoft.Data.Sqlite.SqliteException: SQLite Error 1: 'near \"selectar\": syntax error'."
				, errorString);
	}
	
	// Multiple execution scenarios
    //  - same query with fault: same fault / different fault
    //  - same rule with fault: same fault / different fault
	@Test
	public void testMultipleFaultsQuery() throws SQLException {
		//primer error en la query, comprueba tanto los cotnadores como el texto de error
		QueryStatement.setFaultInjector(new FaultInjector().setTablesFaulty("Injected fault at get tables"));
		rs = app.queryMultipleErrors();
		rs.close();
		assertMultipleFaults(true,"qerror=1,count=0,dead=0","Error at Get query table names: giis.qacover.portable.QaCoverException: Injected fault at get tables");
		//segundo error del mismo texto, nada cambia
		QueryStatement.setFaultInjector(new FaultInjector().setTablesFaulty("Injected fault at get tables"));
		rs = app.queryMultipleErrors();
		rs.close();
		assertMultipleFaults(true,"qerror=1,count=0,dead=0","Error at Get query table names: giis.qacover.portable.QaCoverException: Injected fault at get tables");
		//otro error pero con diferente texto, remplaza al anterior
		QueryStatement.setFaultInjector(new FaultInjector().setUnexpectedException("Injected unexpected exception"));
		rs = app.queryMultipleErrors();
		rs.close();
		assertMultipleFaults(true,"qerror=1,count=0,dead=0","Error at : giis.qacover.portable.QaCoverException: Injected unexpected exception");
	}
	String exceptionInvalidSql=new Variability().isJava()
			? (new Variability().isJava8()
						? "giis.qacover.portable.QaCoverException: SpyStatementAdapter.hasRows. Caused by: org.sqlite.SQLiteException: [SQLITE_ERROR] SQL error or missing database (near \"selectar\": syntax error)"
						: "giis.qacover.portable.QaCoverException: SpyStatementAdapter.hasRows. Caused by: org.h2.jdbc.JdbcSQLException: Syntax error in SQL statement")
			: "Giis.Qacover.Portable.QaCoverException: SpyStatementAdapter.hasRows. Caused by: System.Data.SQLite.SQLiteException: SQL logic error\nnear \"selectar\": syntax error"
			//: "giis.Qacover.Portable.QaCoverException: SpyStatementAdapter.hasRows. Caused by: Microsoft.Data.Sqlite.SqliteException: SQLite Error 1: 'near \"selectar\": syntax error'."
			;

	@Test
	public void testMultipleFaultsRule() throws SQLException {
		//primer error en la query, comprueba tanto los contadores como el texto de error
		QueryStatement.setFaultInjector(new FaultInjector().setSingleRuleFaulty("selectar id,num,text from test where num < 9"));
		rs = app.queryMultipleErrors();
		rs.close();
		assertMultipleFaults(false,"count=2,dead=0,error=1\n" + 
				"count=1,dead=0,error=1\n" + 
				"count=1,dead=0",
				exceptionInvalidSql);
		//mismo error, no cambia el mensaje, aunque si los contadores de las reglas individuales
		QueryStatement.setFaultInjector(new FaultInjector().setSingleRuleFaulty("selectar id,num,text from test where num < 9"));
		rs = app.queryMultipleErrors();
		rs.close();
		assertMultipleFaults(false,"count=2,dead=0,error=1\n" + 
				"count=2,dead=0,error=2\n" + 
				"count=2,dead=0",
				exceptionInvalidSql);
		//otro error differente, ademas de incrementar contadores, anyade el mensaje
		QueryStatement.setFaultInjector(new FaultInjector().setSingleRuleFaulty("select id,num,text from notable where num < 9"));
		rs = app.queryMultipleErrors();
		rs.close();
		assertMultipleFaults(false,"count=2,dead=0,error=1\n" + 
				"count=3,dead=0,error=3\n" + 
				"count=3,dead=0",
				exceptionInvalidSql);
		//esto no impide que otras reglas se cubran, modifico la bd para que se cubra la segunda, esta seguira fallando pues hemos modificado el sql de la regla
		QueryStatement.setFaultInjector(new FaultInjector().setSingleRuleFaulty("select id,num,text from notable where num < 9"));
		app.executeUpdateNative(new String[] { "update test set num=5 where id=1" });
		rs = app.queryMultipleErrors();
		rs.close();
		assertMultipleFaults(false,"count=2,dead=1,error=1\n" + 
				"count=4,dead=0,error=4\n" + 
				"count=4,dead=1",
				exceptionInvalidSql);
	}
	
	private void assertMultipleFaults(boolean checkErrorsAtQuery, String expectedSummary, String expectedErrorString) {
		//Obiene las regas del almacenamiento y comprueba el resumen
		QueryModel model=StoreService.getLast().getQueryModel(StoreService.getLast().getLastSavedQueryKey());
		assertEquals(expectedSummary, model.getTextSummary());
		//texto detallado de los errores
		if (checkErrorsAtQuery) {
			String errorString=model.getErrorString();
			assertExceptionMessage(expectedErrorString,errorString);
		} else {
			//la que tiene fallo inyectado es la primera, comprueba el mensaje
			List<RuleModel> rules=model.getRules();
			String errorString=rules.get(0).getErrorString();
			assertExceptionMessage(expectedErrorString,errorString);
		}
	}

}
