package test4giis.qacover;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertNull;

import java.sql.SQLException;

import org.junit.After;
import org.junit.Before;
import org.junit.Test;

import giis.qacover.core.services.Configuration;
import giis.qacover.core.services.StoreService;
import test4giis.qacoverapp.AppSimpleJdbc;

/**
 * Extension of TestEvaluation to use methods different than executeQuery
 * Situations, multiple combination of:
 *  - method: executeUpdate, execute with a selectc, execute con an update
 *  - Statement typed: prepared, no prepared
 */
public class TestExecute extends Base {
	private AppSimpleJdbc app;

	@Before
	public void setUp() throws SQLException {
		super.setUp();
		app = new AppSimpleJdbc(variant); // aplicacion con los metodos a probar
		setUpTestData();
		Configuration.getInstance().setFpcServiceOptions("noboundaries");
	}

	@After
	public void tearDown() throws SQLException {
		super.tearDown();
		app.close(); // para la conexion abierta que mantiene la aplicacion
	}

	public void setUpTestData() { // misma estructura que en TestEvaluation
		app.dropTable("test");
		app.executeUpdateNative(new String[] { "create table test(id int not null, num int not null, text varchar(16))",
				"insert into test(id,num,text) values(1,0,'abc')",
				"insert into test(id,num,text) values(2,99,'xyz')" });
	}

	// executeUpdate does not generate rules, but must check that the query execution is not filtered

	@Test
	public void testExecuteUpdateNoParameters() throws SQLException {
		app.executeUpdate("update test set text='def' where id=?", 2, false);
		assertNull(StoreService.getLast()); // no rules, then a store was not instantiated
		String actual = app.executeSelectNative("select id,num,text from test order by id");
		assertEquals("1 0 abc\n2 99 def", actual); // but updte was executed
		assertNull(StoreService.getLast()); // and no rules have been evaluated
	}

	@Test
	public void testExecuteUpdateParameters() throws SQLException {
		app.executeUpdate("update test set text='def' where id=?", 2, true);
		assertNull(StoreService.getLast());
		String actual = app.executeSelectNative("select id,num,text from test order by id");
		assertEquals("1 0 abc\n2 99 def", actual);
		assertNull(StoreService.getLast());
	}

	// Generic execute must have same behaviour if sql is update

	@Test
	public void testExecuteGenericUpdateNoParameters() throws SQLException {
		app.executeGeneric("Update test set text='def' where id=?", 2, false, false);
		assertNull(StoreService.getLast());
		String actual = app.executeSelectNative("select id,num,text from test order by id");
		assertEquals("1 0 abc\n2 99 def", actual);
		assertNull(StoreService.getLast());
	}

	@Test
	public void testExecuteGenericUpdateParameters() throws SQLException {
		app.executeGeneric("Update test set text='def' where id=?", 2, true, false);
		assertNull(StoreService.getLast());
		String actual = app.executeSelectNative("select id,num,text from test order by id");
		assertEquals("1 0 abc\n2 99 def", actual);
		assertNull(StoreService.getLast());
	}

	// Generic execute must evaluate rules if sql is select

	public void testExecuteGenericSelectNoParameters() throws SQLException {
		rs = app.executeGeneric("Select id,num,text from test where num<?", 100, false, true);
		assertEvalResults("Select id,num,text from test where num<100", 
				"1 0 abc\n2 99 xyz", SqlUtil.resultSet2csv(rs, " "), 
				"UNCOVERED SELECT id , num , text FROM test WHERE NOT(num < 100)\n" + 
				"COVERED   SELECT id , num , text FROM test WHERE (num < 100)");
	}

	public void testExecuteGenericSelectParameters() throws SQLException {
		rs = app.executeGeneric("Select id,num,text from test where num<?", 100, true, true);
		assertEvalResults("Select id,num,text from test where num<?", 
				"1 0 abc\n2 99 xyz", SqlUtil.resultSet2csv(rs, " "), 
				"UNCOVERED SELECT id , num , text FROM test WHERE NOT(num < 100)\n" +
				"COVERED   SELECT id , num , text FROM test WHERE (num < 100)");
	}

}
