package test4giis.qacover;

import static org.junit.Assert.assertEquals;

import java.sql.*;

import org.junit.After;
import org.junit.Before;
import org.junit.Test;

import giis.qacover.core.services.Configuration;
import giis.qacover.core.services.StoreService;
import giis.qacover.model.QueryModel;
import giis.qacover.model.Variability;
import test4giis.qacoverapp.AppSimpleJdbc;
/**
 * Rule generation and evaluation with jdbc connections created with DriverManager
 * and using the executeQuery methods (other execution methods are in TestExecute)
 *
 * Situations
 *  - type of query: no parameters / with parameters / using Apache DbUtils (in separate class)
 *  - number of parameters: one / more
 *  - datatype of parameters: int / string
 *  - number of conditions: one / two / none
 *  - additional FPC options: with / without
 *  - number of rules covered: one / two /none
 *  - positoin of rules covered: boundaries
 *  - Identifiers: combine table /column with double quote / brackets(sqlserver)
 *  - Named jdbc parameters: can be replaced / duplicated value error
 */
public class TestEvaluation extends Base {
	private AppSimpleJdbc app;

	@Before
	public void setUp() throws SQLException {
		super.setUp();
		app=new AppSimpleJdbc(variant);
		setUpTestData();
	}

	@After
	public void tearDown() throws SQLException{
		super.tearDown();
		app.close();
	}

	public void setUpTestData() {
		app.dropTable("test");
		app.executeUpdateNative(new String[] {
				"create table test(id int not null, num int not null, text varchar(16))", 
				"insert into test(id,num,text) values(1,0,'abc')",
				"insert into test(id,num,text) values(2,99,'xyz')",
				"insert into test(id,num,text) values(3,99,null)"
		});
	}
	
	@Test
	public void testEvalNoParameters() throws SQLException {
		rs = app.queryNoParameters1Condition(-1);
		assertEvalResults("select id,num,text from test where num>=-1", 
				"1 0 abc\n2 99 xyz\n3 99 NULL", SqlUtil.resultSet2csv(rs," "), 
				"COVERED   SELECT id , num , text FROM test WHERE (num = 0)\n" + 
				"UNCOVERED SELECT id , num , text FROM test WHERE (num = -1)\n" + 
				"UNCOVERED SELECT id , num , text FROM test WHERE (num = -2)",
				"{}");
	}
	
	@Test
	public void testEvalInferParameters() throws SQLException {
		// no prepared statement, parameters replaced in the string
		Configuration.getInstance().setInferQueryParameters(true);
		rs = app.queryNoParameters2Condition(98,"abc");
		// sql has been parsed and parameters are inferend
		assertEvalResults("SELECT id , num , text FROM test WHERE num > ?1? AND text = ?2?", 
				"", SqlUtil.resultSet2csv(rs," "), 
				"UNCOVERED SELECT id , num , text FROM test WHERE (num = 98 + 1) AND (text = 'abc')\n" + 
				"UNCOVERED SELECT id , num , text FROM test WHERE (num = 98) AND (text = 'abc')\n" + 
				"UNCOVERED SELECT id , num , text FROM test WHERE (num = 98 - 1) AND (text = 'abc')\n" + 
				"UNCOVERED SELECT id , num , text FROM test WHERE (text = 'abc') AND (num > 98)\n" + 
				"COVERED   SELECT id , num , text FROM test WHERE NOT(text = 'abc') AND (num > 98)\n" + 
				"COVERED   SELECT id , num , text FROM test WHERE (text IS NULL) AND (num > 98)");
	}
	
	@Test
	public void testEvalInferParametersNegative() throws SQLException {
		Configuration.getInstance().setInferQueryParameters(true);
		rs = app.queryNoParameters1Condition(-1);
		assertEvalResults("SELECT id , num , text FROM test WHERE num >= ?1?", 
				"1 0 abc\n2 99 xyz\n3 99 NULL", SqlUtil.resultSet2csv(rs," "), 
				"COVERED   SELECT id , num , text FROM test WHERE (num = -1 + 1)\n" + 
				"UNCOVERED SELECT id , num , text FROM test WHERE (num = -1)\n" + 
				"UNCOVERED SELECT id , num , text FROM test WHERE (num = -1 - 1)");
	}
	
	@Test
	public void testEvalFpcOptions() throws SQLException {
		configureTestOptions().setRuleOptions("noboundaries");
		rs = app.queryNoParameters1Condition(-1);
		assertEvalResults("select id,num,text from test where num>=-1", 
				"1 0 abc\n2 99 xyz\n3 99 NULL", SqlUtil.resultSet2csv(rs," "), 
				"UNCOVERED SELECT id , num , text FROM test WHERE NOT(num >= -1)\n" + 
				"COVERED   SELECT id , num , text FROM test WHERE (num >= -1)");
	}
	
	@Test
	public void testEvalParameters() throws SQLException {
		Configuration.getInstance().setInferQueryParameters(false);
		rs = app.queryParameters(98,"abc");
		assertEvalResults("select id,num,text from test where num>? and text=?", 
				"", SqlUtil.resultSet2csv(rs," "), 
				"UNCOVERED SELECT id , num , text FROM test WHERE (num = 98 + 1) AND (text = 'abc')\n" + 
				"UNCOVERED SELECT id , num , text FROM test WHERE (num = 98) AND (text = 'abc')\n" + 
				"UNCOVERED SELECT id , num , text FROM test WHERE (num = 98 - 1) AND (text = 'abc')\n" + 
				"UNCOVERED SELECT id , num , text FROM test WHERE (text = 'abc') AND (num > 98)\n" + 
				"COVERED   SELECT id , num , text FROM test WHERE NOT(text = 'abc') AND (num > 98)\n" + 
				"COVERED   SELECT id , num , text FROM test WHERE (text IS NULL) AND (num > 98)",
				"{?1?=98, ?2?='abc'}", false, new Variability().isNetCore());
	}
	
	@Test
	public void testEvalParametersNamed() throws SQLException {
		if (new Variability().isJava4() || new Variability().isNetCore())
			return;
		Configuration.getInstance().setInferQueryParameters(false).setRuleOptions("noboundaries");
		rs = app.queryParametersNamed(1,1,"abc");
		assertEvalResults("/* params=?1?,?1?,?2? */ select id,num,text from test where id=? or num=? or text=?", 
				"1 0 abc", SqlUtil.resultSet2csv(rs," "), 
				"COVERED   /*params=1,1,'abc'*/ SELECT id , num , text FROM test WHERE NOT(id = 1) AND NOT(num = 1) AND NOT(text = 'abc')\n" + 
				"UNCOVERED /*params=1,1,'abc'*/ SELECT id , num , text FROM test WHERE (id = 1) AND NOT(num = 1) AND NOT(text = 'abc')\n" + 
				"UNCOVERED /*params=1,1,'abc'*/ SELECT id , num , text FROM test WHERE (num = 1) AND NOT(id = 1) AND NOT(text = 'abc')\n" + 
				"UNCOVERED /*params=1,1,'abc'*/ SELECT id , num , text FROM test WHERE (text = 'abc') AND NOT(id = 1) AND NOT(num = 1)\n" + 
				"COVERED   /*params=1,1,'abc'*/ SELECT id , num , text FROM test WHERE (text IS NULL) AND NOT(id = 1) AND NOT(num = 1)");
		QueryModel model=StoreService.getLast().getQueryModel(StoreService.getLast().getLastSavedQueryKey());
		String sql = model.getSql();
		assertEquals("/*params=?1?,?1?,?2?*/ SELECT id , num , text FROM test WHERE id = ?1? OR num = ?1? OR text = ?2?",sql);

	}
	
	@Test
	public void testEvalParametersNamedInconsistency() throws SQLException {
		if (new Variability().isJava4() || new Variability().isNetCore())
			return;
		Configuration.getInstance().setInferQueryParameters(false).setRuleOptions("noboundaries");
		rs = app.queryParametersNamed(1,2,"abc");
		assertEquals("Error at : giis.qacover.portable.QaCoverException: StatementAdapter: Parameter ?1? had been assigned to 1. Can't be assigned to a new value 2",
				StoreService.getLast().getLastGenStatus());
	}
	
	@Test
	public void testEvalNoConditions() throws SQLException {
		rs = app.queryNoConditions();
		assertEvalResults("select id,num,text from test", "1 0 abc\n2 99 xyz\n3 99 NULL", SqlUtil.resultSet2csv(rs," "), "");
	}
	
	@Test
	public void testEvalNoConditionsInferParams() throws SQLException {
		Base.configureTestOptions().setInferQueryParameters(true);
		rs = app.queryNoConditions();
		assertEvalResults("SELECT id , num , text FROM test", "1 0 abc\n2 99 xyz\n3 99 NULL", SqlUtil.resultSet2csv(rs," "), "");
	}
	
	@Test
	public void testEvalIdentifiersWithQuotes() throws SQLException {
		if (new Variability().isJava4() || new Variability().isNetCore())
			return; // ignore in java 1.4, uses h2 that causes error when text has double quotes
		String table = "test";
		// Oracle and H2: quoted identifier is case sensitive
		String column = variant.isOracle() || variant.isH2() ? "\"TEXT\"" : "\"text\"";
		rs = app.queryNoParametersQuotes("xyz", false, true); //quotes solo en columna
		assertEvalResults("select id,num,text from test where " + column + "='xyz'", "2 99 xyz", SqlUtil.resultSet2csv(rs," "), 
				"COVERED   SELECT id , num , text FROM test WHERE NOT(" + column + " = 'xyz')\n" + 
				"COVERED   SELECT id , num , text FROM test WHERE (" + column + " = 'xyz')\n" + 
				"COVERED   SELECT id , num , text FROM test WHERE (" + column + " IS NULL)");
		rs = app.queryNoParametersQuotes("xyz", true, true); //quotes en columna y tabla
		table = variant.isOracle() || variant.isH2() ? "\"TEST\"" : "\"test\"";
		assertEvalResults("select id,num,text from " + table + " where " + column + "='xyz'", "2 99 xyz", SqlUtil.resultSet2csv(rs," "), 
				"COVERED   SELECT id , num , text FROM " + table + " WHERE NOT(" + column + " = 'xyz')\n" + 
				"COVERED   SELECT id , num , text FROM " + table + " WHERE (" + column + " = 'xyz')\n" + 
				"COVERED   SELECT id , num , text FROM " + table + " WHERE (" + column + " IS NULL)");
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
