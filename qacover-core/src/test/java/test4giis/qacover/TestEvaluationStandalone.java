package test4giis.qacover;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertThrows;
import static org.junit.Assert.assertTrue;

import java.sql.Connection;
import java.sql.SQLException;
import java.sql.Statement;

import org.junit.After;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TestName;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import giis.portable.util.JavaCs;
import giis.qacover.eval.StandaloneEvaluator;
import giis.qacover.eval.reader.JdbcQueryStatementReader;
import giis.qacover.model.QueryModel;
import giis.qacover.model.RuleModel;
import giis.qacover.model.Variability;
import giis.qacover.portable.QaCoverException;
import giis.tdrules.openapi.model.TdRule;
import giis.tdrules.openapi.model.TdRules;
import test4giis.qacoverapp.ConnectionFactory;

/**
 * Unlike most of the evaluation tests, the standalone evaluator tests do not use an app mock but specific
 * models for each test and a direct Jdbc connection against the database.
 */
public class TestEvaluationStandalone {
	private static final Logger log = LoggerFactory.getLogger(TestEvaluationStandalone.class);

	// Variables used along the tests, initialized in the setUp
	private Connection conn;
	private Statement stmt;
	Variability variant;
	StandaloneEvaluator evaluator;

	@Rule
	public TestName testName = new TestName();

	@Before
	public void setUp() throws SQLException {
		log.info("*** CURRENT TEST - " + testName.getMethodName());
		this.variant = getVariant();
		log.info("*** CURRENT DBMS - " + variant.getSgbdName());
		this.conn = new ConnectionFactory(variant).getNativeConnection(); // do not use p6spy
		this.stmt = conn.createStatement();
		this.evaluator = new StandaloneEvaluator(this.conn).setSkipIfCovered(false).setDebugSql(true);
		cleanUpTestData();
	}

	@After
	public void tearDown() throws SQLException {
		this.stmt.close();
		this.conn.close();
	}

	// to allow subclasses to test other SGBDs
	protected Variability getVariant() {
		return new Variability("sqlite");
	}

	protected void cleanUpTestData() throws SQLException {
		stmt.execute("drop table if exists test");
		stmt.execute("create table test(id int not null, num int not null, text varchar(16))");
	}

	protected void assertSummary(QueryModel model, String expected) {
		assertEquals(expected, model.getTextSummary(true).replace("\n", " ")); // to facilitate writing expected
	}

	protected TdRules getRules(String rulesClass, String query, String... ruleQueries) {
		TdRules rules = new TdRules().rulesClass(rulesClass).query(query);
		for (int i = 0; i < ruleQueries.length; i++)
			rules.addRulesItem(new TdRule().id(String.valueOf(i + 1)).query(ruleQueries[i]));
		return rules;
	}

	// Scenario to check the rule coverage evolution by adding rows at each step
	@Test
	public void testFpcMain() throws SQLException {
		TdRules rules = getRules("fpc", "select * from test where text='abc'", 
				"select * from test where num<=0",
				"select * from test where text='xyz'", 
				"select * from test where text is null");
		QueryModel model = new QueryModel(rules);

		// none covered, only count runs
		evaluator.evaluate(model);
		assertSummary(model, "count=3,dead=0,qrun=1 count=1,dead=0 count=1,dead=0 count=1,dead=0");

		// one covered, increment dead count in rule and query
		stmt.execute("insert into test(id,num,text) values(1, 0, 'abc')");
		evaluator.evaluate(model);
		assertSummary(model, "count=3,dead=1,qrun=2 count=2,dead=1 count=2,dead=0 count=2,dead=0");

		// rule already covered, increment dead in rule, but not dead in query
		stmt.execute("insert into test(id,num,text) values(2, -1, 'abc')");
		evaluator.evaluate(model);
		assertSummary(model, "count=3,dead=1,qrun=3 count=3,dead=2 count=3,dead=0 count=3,dead=0");

		// other rule covered, increment dead count in new rule, old rule and query
		stmt.execute("insert into test(id,num,text) values(3, 99, null)");
		evaluator.evaluate(model);
		assertSummary(model, "count=3,dead=2,qrun=4 count=4,dead=3 count=4,dead=0 count=4,dead=1");

		// Final check to verify each measure in query and a rule
		assertEquals(3, model.getCount());
		assertEquals(2, model.getDead());
		assertEquals(4, model.getQrun());
		assertEquals(0, model.getError());
		assertEquals("", model.getErrorString());

		assertEquals(4, model.getRules().get(0).getCount());
		assertEquals(3, model.getRules().get(0).getDead());
		assertEquals(0, model.getRules().get(0).getError());
		assertEquals("", model.getRules().get(0).getErrorString());
		
		// A simple log with all results (example in the readme)
		log.info("Covered {} rule(s) out of {}. Query evaluated {} time(s)", model.getDead(), model.getCount(), model.getQrun());
		for (RuleModel rule : model.getRules())
			log.info("Rule {}: covered {} time(s), executed {}, with error {}. Error messages: {}", 
					rule.getId(), rule.getDead(), rule.getCount(), rule.getError(), rule.getErrorString());
	}

	// Do not evaluate rules that are already covered
	// Using the same rules than previous test, but shorter scenario
	@Test
	public void testFpcSkipIfCovered() throws SQLException {
		TdRules rules = getRules("fpc", "select * from test where text='abc'", 
				"select * from test where num<=0",
				"select * from test where text='xyz'", 
				"select * from test where text is null");
		QueryModel model = new QueryModel(rules);
		evaluator.setSkipIfCovered(true);

		// one covered, increment dead count in rule and query
		stmt.execute("insert into test(id,num,text) values(1, 0, 'abc')");
		evaluator.evaluate(model);
		assertSummary(model, "count=3,dead=1,qrun=1 count=1,dead=1 count=1,dead=0 count=1,dead=0");

		// two covered, one already covered, DO NOT increment dead in rule already covered
		stmt.execute("insert into test(id,num,text) values(3, 99, null)");
		evaluator.evaluate(model);
		assertSummary(model, "count=3,dead=2,qrun=2 count=1,dead=1 count=2,dead=0 count=2,dead=1");
	}

	// General purpose database for next tests
	protected void setupTestData() throws SQLException {
		stmt.execute("insert into test(id,num,text) values(1, 0, 'abc')");
		stmt.execute("insert into test(id,num,text) values(2, 50, 'xyz')");
		stmt.execute("insert into test(id,num,text) values(3, 99, null)");
	}

	@Test
	public void testFpcParameters() throws SQLException {
		setupTestData();
		TdRules rules = getRules("fpc", "select * from test where num=?1? and text=?2?", 
				"select * from test where num=?1?",
				"select * from test where text=?2?");
		QueryModel model = new QueryModel(rules);

		// int parameter increases coverage
		evaluator.evaluate(model, "0", "'none'");
		assertSummary(model, "count=2,dead=1,qrun=1 count=1,dead=1 count=1,dead=0");

		// string parameter increases coverage
		evaluator.evaluate(model, "-1", "'xyz'");
		assertSummary(model, "count=2,dead=2,qrun=2 count=2,dead=1 count=2,dead=1");
	}
	
	@Test
	public void testDemo() throws SQLException {
		setupTestData();
		TdRules rules = getRules("fpc", "select * from test where num=?1? and text=?2?", 
				"select * from test where num=?1?",
				"select * from test where text=?2?");
		QueryModel model = new QueryModel(rules);
		
		evaluator.evaluate(model, "0", "'none'");
		evaluator.evaluate(model, "-1", "'xyz'");
		log.info("{} rule(s) covered out of {}", model.getDead(), model.getCount());
		for (RuleModel rule : model.getRules()) {
			log.info("Rule {}: covered {} times, executed {}, with error {}. Error messages: {}", 
					rule.getId(), rule.getDead(), rule.getCount(), rule.getError(), rule.getErrorString());
		}
		
	}

	@Test
	public void testFpcErrors() throws SQLException {
		setupTestData();
		// Manages different amounts of parameters to check error counts and messages
		TdRules rules = getRules("fpc", "select * from test where num=?1? and text=?2?", 
				"select * from test where num=?1?",
				"select * from test where text=?2?");
		QueryModel model = new QueryModel(rules);

		// extra parameters are just skipped
		evaluator.evaluate(model, "0", "'none'", "'skip'");
		assertSummary(model, "count=2,dead=1,qrun=1 count=1,dead=1 count=1,dead=0");

		// insufficient parameters cause error, rule that fails does not increment dead
		evaluator.evaluate(model, "-1");
		assertSummary(model, "count=2,dead=1,error=1,qrun=2 count=2,dead=1 count=2,dead=0,error=1");
		assertEquals(1, model.getError());
		assertEquals("", model.getErrorString());
		assertEquals(0, model.getRules().get(0).getError());
		assertEquals("", model.getRules().get(0).getErrorString());
		assertEquals(1, model.getRules().get(1).getError());
		// assert only part of the message to make the check compatible with other SGBDs
		String message = model.getRules().get(1).getErrorString();
		Base.assertContains("giis.qacover.portable.QaCoverException: IQueryStatementReader.hasRows", message);

		// errors (count and string) are cumulative (at the rule, not in the query), new error because wrong data type
		evaluator.evaluate(model, "-1", "unquoted");
		assertSummary(model, "count=2,dead=1,error=1,qrun=3 count=3,dead=1 count=3,dead=0,error=2");
		assertEquals(2, model.getRules().get(1).getError());
		message = model.getRules().get(1).getErrorString();
		Base.assertContains("giis.qacover.portable.QaCoverException: IQueryStatementReader.hasRows", message);
		// to check that there are two messages removes the first characters and assert again
		String message2 = JavaCs.substring(model.getRules().get(1).getErrorString(), 40, message.length());
		Base.assertContains("giis.qacover.portable.QaCoverException: IQueryStatementReader.hasRows", message2);
	}

	// Previous tests exercise the coverage measures and exceptions using fpc.
	// Here we concentrate into the evaluation of mutants, using fixed database and each query with a single rule
	@Test
	public void testMutMain() throws SQLException {
		setupTestData();
		TdRules rules = getRules("mutation", "select id,num,text from test where num=0 or text is null order by id",
				"select id,num,text from test where num<>50 order by id");
		QueryModel model = new QueryModel(rules);

		// live, query and rule match their results, using first and last row
		evaluator.evaluate(model);
		assertSummary(model, "count=1,dead=0,qrun=1 count=1,dead=0");

		// below tests change the sql of the rule to check dead

		// dead, query returns more rows than rule
		rules.getRules().get(0).query("select id,num,text from test where num=0 order by id");
		evaluator.evaluate(model);
		assertSummary(model, "count=1,dead=1,qrun=2 count=2,dead=1");

		// dead, query returns less rows than rule
		rules.getRules().get(0).query("select id,num,text from test where num>=0 order by id");
		evaluator.evaluate(model);
		assertSummary(model, "count=1,dead=1,qrun=3 count=3,dead=2");
	}

	// Scenario to check the rule coverage evolution and reset values
	@Test
	public void testResetMut() throws SQLException {
		setupTestData();
		TdRules rules = getRules("mutation", "select id,num,text from test where num=0 or text is null order by id",
				"select id,num,text from test where num<>50 order by id");
		QueryModel model = new QueryModel(rules);

		// none covered, only count runs
		evaluator.evaluate(model);
		assertSummary(model, "count=1,dead=0,qrun=1 count=1,dead=0");
			
		// reset, all counts to 0 again
		model.reset();
		assertSummary(model, "count=0,dead=0,qrun=0 count=0,dead=0");
		
		// dead, query returns more rows than rule
		rules.getRules().get(0).query("select id,num,text from test where num=0 order by id");
		evaluator.evaluate(model);
		assertSummary(model, "count=1,dead=1,qrun=1 count=1,dead=1");

		// reset again
		model.reset();
		assertSummary(model, "count=0,dead=0,qrun=0 count=0,dead=0");
			
		// dead, query returns less rows than rule
		rules.getRules().get(0).query("select id,num,text from test where num>=0 order by id");
		evaluator.evaluate(model);
		assertSummary(model, "count=1,dead=1,qrun=1 count=1,dead=1");

		// Final check to verify each measure in query and a rule
		assertEquals(1, model.getCount());
		assertEquals(1, model.getDead());
		assertEquals(1, model.getQrun());
		assertEquals(0, model.getError());
		assertEquals("", model.getErrorString());

		assertEquals(1, model.getRules().get(0).getCount());
		assertEquals(1, model.getRules().get(0).getDead());
		assertEquals(0, model.getRules().get(0).getError());
		assertEquals("", model.getRules().get(0).getErrorString());
	}

	@Test
	public void testMutEmptyRows() throws SQLException {
		setupTestData();
		TdRules rules = getRules("mutation", "select id,num,text from test where num<0 order by id",
				"select id,num,text from test where num=33 order by id");
		QueryModel model = new QueryModel(rules);

		// Live, query and rule are empty
		evaluator.evaluate(model);
		assertSummary(model, "count=1,dead=0,qrun=1 count=1,dead=0");

		// Dead, expected not empty
		rules.query("select id,num,text from test where num>=0 order by id");
		evaluator.evaluate(model);
		assertSummary(model, "count=1,dead=1,qrun=2 count=2,dead=1");

		// Dead, rule not empty
		rules.query("select id,num,text from test where num<0 order by id");
		rules.getRules().get(0).query("select id,num,text from test where num>=0 order by id");
		evaluator.evaluate(model);
		assertSummary(model, "count=1,dead=1,qrun=3 count=3,dead=2");
	}

	@Test
	public void testMutErrors() throws SQLException {
		setupTestData();
		TdRules rules = getRules("mutation", "select id,num,text from test where num>0 order by id",
				"errorselect id,num,text from test where num>0 order by id");
		QueryModel model = new QueryModel(rules);

		// Rule exception, wrong syntax
		evaluator.evaluate(model);
		assertSummary(model, "count=1,dead=0,error=1,qrun=1 count=1,dead=0,error=1");
		String message = model.getRules().get(0).getErrorString();
		Base.assertContains("giis.qacover.portable.QaCoverException: IQueryStatementReader.equalRows", message);

		// Rule exception, number of columns do not match
		rules.query("select id,num,text from test where num>0 order by id");
		rules.getRules().get(0).query("select id,num from test where num>0 order by id");
		evaluator.evaluate(model);
		assertSummary(model, "count=1,dead=1,error=1,qrun=2 count=2,dead=1,error=1");

		// Query exception. This is not handled (unlike when run from the QACover manager
		// that creates a model without rules and the error message)
		rules.query("selecterror id,num,text from test where num>0 order by id");
		rules.getRules().get(0).query("select id,num,text from test where num>0 order by id");
		QaCoverException e = assertThrows(QaCoverException.class, () -> {
			evaluator.evaluate(model);
		});
		assertEquals("giis.qacover.portable.QaCoverException: Schema Exception", e.toString());
	}

	// Additional attributes in the model that can be present if special parameters
	// are passed to the mutation service (parsedquery, ordercols)
	@Test
	public void testMutOrderCols() throws SQLException {
		setupTestData();
		TdRules rules = getRules("mutation", "errorselect id,num,text from test where num>0",
				"select id,num,text from test where num>0");
		rules.parsedquery("SELECT id, num, text FROM test WHERE num > 0");
		QueryModel model = new QueryModel(rules);

		// The query sql in this test would cause exception,
		// but because the model includes the parsedquery, there is no exception
		evaluator.evaluate(model);
		assertSummary(model, "count=1,dead=0,qrun=1 count=1,dead=0");

		// Additionally, if there is a summary attribute with the ordercols key, an order by is added 
		// to both query and rules when executed against the DB to have more predictable comparisons
		// MANUAL CHECK the logs of query and rule
		rules.putSummaryItem("ordercols", "1,2,3");
		evaluator.evaluate(model);
		assertSummary(model, "count=1,dead=0,qrun=2 count=2,dead=0");
	}
	
	// Unit test the row comparison (as java8 does not have native array comparison
	@Test
	public void testMutCompareRows() {
		assertTrue(JdbcQueryStatementReader.equalRow(new String[] {"a", "b"}, new String[] {"a", "b"}));
		assertFalse(JdbcQueryStatementReader.equalRow(new String[] {"a", "b"}, new String[] {"x", "b"}));
		assertFalse(JdbcQueryStatementReader.equalRow(new String[] {"a", "b"}, new String[] {"a", "x"}));
		
		assertTrue(JdbcQueryStatementReader.equalRow(new String[] {"a", null}, new String[] {"a", null}));
		assertFalse(JdbcQueryStatementReader.equalRow(new String[] {"a",null}, new String[] {"a", "b"}));
		assertFalse(JdbcQueryStatementReader.equalRow(new String[] {"a", "b"}, new String[] {"a", null}));
		
		assertFalse(JdbcQueryStatementReader.equalRow(new String[] {"a", "b", "c"}, new String[] {"a", "b"}));
		assertFalse(JdbcQueryStatementReader.equalRow(new String[] {"a", "b"}, new String[] {"a", "b", "c"}));
	}

}
