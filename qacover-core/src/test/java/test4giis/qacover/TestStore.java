package test4giis.qacover;

import org.junit.After;
import org.junit.Before;
import org.junit.Test;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import giis.qacover.core.services.Configuration;
import giis.qacover.core.services.StoreService;
import giis.qacover.model.Variability;
import test4giis.qacoverapp.AppSimpleJdbc;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertFalse;

import java.sql.*;
/**
 * Scenarios to check the storage of the rule evaluation
 * Situations (* estan cubiertas):
 *  - Evaluation (parameter values): *query with different parameters / *different queries
 *  - Evaluation (coverage increase): *no incremento, *incremento, *varios
 *  - Store persistence: *new instance preserves previous results (default) / *does not preserves (after drop)
 *  - Location of different queries: *same line / *same method / *different method and class
 *  - Location of equal queries: *same line / *same method / different method, different class (always different provided that they are in differen lines)
 *  - Query exclusion: by partial mach / all / not excluded
 *  - Table query exclusion: excluded total match / not excluded partial match
 */
public class TestStore extends Base {
	private static final Logger log = LoggerFactory.getLogger(TestStore.class);
	private AppSimpleJdbc app;

	@Before
	@Override
	public void setUp() throws SQLException {
		super.setUp();
		options.setRuleOptions("noboundaries");
		app = new AppSimpleJdbc(variant); // aplicacion con los metodos a probar
		setUpTestData();
	}

	@After
	@Override
	public void tearDown() throws SQLException {
		super.tearDown();
		app.close();
	}

	public void setUpTestData() {
		app.dropTable("test");
		app.executeUpdateNative(new String[] { 
				"create table test(id int not null, num int not null, text varchar(16))",
				"insert into test(id,num,text) values(1,99,'xyz')"
				});
	}

	@Test
	public void testScenarioParametrized() throws SQLException {
		// also covers different queries, different methods, more than an increment in a rule
		rs = app.queryParameters(90, "nnn");
		StoreService store = StoreService.getLast();
		String firstFile = store.getLastSavedQueryKey();
		log.debug("Saved to file: " + store.getLastSavedQueryKey());
		assertEquals("count=4,dead=1\ncount=1,dead=0\ncount=1,dead=0\ncount=1,dead=1\ncount=1,dead=0",
				store.getQueryModel(firstFile).getTextSummary());
		rs.close();
		rs = app.queryParameters(90, "nnn");
		store = StoreService.getLast();
		log.debug("Saved to file: " + store.getLastSavedQueryKey());
		assertEquals(firstFile, store.getLastSavedQueryKey());
		assertEquals("count=4,dead=1\ncount=2,dead=0\ncount=2,dead=0\ncount=2,dead=2\ncount=2,dead=0",
				store.getQueryModel(firstFile).getTextSummary());

		// increment a rule
		rs = app.queryParameters(90, "xyz");
		store = StoreService.getLast();
		log.debug("Saved to file: " + store.getLastSavedQueryKey());
		assertEquals(firstFile, store.getLastSavedQueryKey());
		assertEquals("count=4,dead=2\ncount=3,dead=1\ncount=3,dead=0\ncount=3,dead=2\ncount=3,dead=0",
				store.getQueryModel(firstFile).getTextSummary());
		rs.close();

		// different query, another summary an file
		rs = app.queryNoParameters1Condition(99);
		store = StoreService.getLast();
		log.debug("Saved to file: " + store.getLastSavedQueryKey());
		String secondFile = store.getLastSavedQueryKey();
		assertFalse(firstFile.equals(store.getLastSavedQueryKey()));
		assertEquals("count=2,dead=1\n" 
				+ "count=1,dead=0\ncount=1,dead=1",
				store.getQueryModel(secondFile).getTextSummary());
		// still can read the first file
		assertEquals("count=4,dead=2\n"
				+ "count=3,dead=1\ncount=3,dead=0\ncount=3,dead=2\ncount=3,dead=0",
				store.getQueryModel(firstFile).getTextSummary());
		rs.close();
	}

	@Test
	public void testScenarioNoParametrized() throws SQLException {
		// simplified version of the above without parameters
		Configuration.getInstance().setInferQueryParameters(true);
		rs = app.queryNoParameters2Condition(90, "nnn");
		StoreService store = StoreService.getLast();
		String firstFile = store.getLastSavedQueryKey();
		assertEquals("count=4,dead=1\ncount=1,dead=0\ncount=1,dead=0\ncount=1,dead=1\ncount=1,dead=0",
				store.getQueryModel(firstFile).getTextSummary());
		rs.close();
		rs = app.queryNoParameters2Condition(90, "nnn");
		store = StoreService.getLast();
		assertEquals(firstFile, store.getLastSavedQueryKey());
		assertEquals("count=4,dead=1\ncount=2,dead=0\ncount=2,dead=0\ncount=2,dead=2\ncount=2,dead=0",
				store.getQueryModel(firstFile).getTextSummary());
		rs = app.queryNoParameters2Condition(90, "xyz");
		store = StoreService.getLast();
		assertEquals(firstFile, store.getLastSavedQueryKey());
		assertEquals("count=4,dead=2\ncount=3,dead=1\ncount=3,dead=0\ncount=3,dead=2\ncount=3,dead=0",
				store.getQueryModel(firstFile).getTextSummary());
		rs.close();
	}

	// Based on the first step of testScenarioParametrized, with optimization to do not re-execute rules already covered
	@Test
	public void testScenarioOptimized() throws SQLException {
		Configuration.getInstance().setOptimizeRuleEvaluation(true);
		rs = app.queryParameters(90, "nnn"); // solo 3rd rule is covered from the beginning as in testScenarioParametrized
		rs.close();
		app.executeUpdateNative(new String[] { "insert into test(id,num,text) values(1,99,'nnn')" });
		rs = app.queryParameters(90, "nnn"); // this also covers 1st
		rs.close();
		rs = app.queryParameters(90, "nnn");
		rs.close();
		StoreService store = StoreService.getLast();
		String firstFile = store.getLastSavedQueryKey();
		// without optimization, the result would be:
		// count=4,dead=2\ncount=3,dead=2\ncount=3,dead=0\ncount=3,dead=3\ncount=3,dead=0
		assertEquals("count=4,dead=2\ncount=2,dead=1\ncount=3,dead=0\ncount=1,dead=1\ncount=3,dead=0",
				store.getQueryModel(firstFile).getTextSummary());
	}

	@Test
	public void testScenarioPersistence() throws SQLException {
		Configuration.getInstance().setInferQueryParameters(true);
		rs = app.queryNoParameters2Condition(90, "nnn");
		StoreService store = StoreService.getLast();
		String firstFile = store.getLastSavedQueryKey();
		assertEquals("count=4,dead=1\ncount=1,dead=0\ncount=1,dead=0\ncount=1,dead=1\ncount=1,dead=0",
				store.getQueryModel(firstFile).getTextSummary());
		rs.close(); // close here to allow a new rs
		
		// a new instance preserves previous coverage data
		store.dropLast();
		rs = app.queryNoParameters2Condition(90, "nnn");
		store = StoreService.getLast();
		assertEquals(firstFile, store.getLastSavedQueryKey());
		assertEquals("count=4,dead=1\ncount=2,dead=0\ncount=2,dead=0\ncount=2,dead=2\ncount=2,dead=0",
				store.getQueryModel(firstFile).getTextSummary());
		
		// but drop does not preserves any coverage data
		StoreService.getLast().dropRules();
		store.dropLast();
		rs = app.queryNoParameters2Condition(90, "nnn");
		store = StoreService.getLast();
		assertEquals(firstFile, store.getLastSavedQueryKey());
		assertEquals("count=4,dead=1\ncount=1,dead=0\ncount=1,dead=0\ncount=1,dead=1\ncount=1,dead=0",
				store.getQueryModel(firstFile).getTextSummary());
		StoreService.getLast().dropRules();
		store.dropLast();
		rs = app.queryNoParameters2Condition(90, "xyz");
		store = StoreService.getLast();
		assertEquals(firstFile, store.getLastSavedQueryKey());
		assertEquals("count=4,dead=1\ncount=1,dead=1\ncount=1,dead=0\ncount=1,dead=0\ncount=1,dead=0",
				store.getQueryModel(firstFile).getTextSummary());
		rs.close();
	}

	@Test
	public void testDifferentQueriesSameLine() throws SQLException {
		// runs first time only to get the file name for further comparison
		app.queryDifferentSingleLine(true, 99, false, "");
		StoreService store = StoreService.getLast();
		String firstFile = store.getLastSavedQueryKey();
		log.debug("Saved to file: " + store.getLastSavedQueryKey());
		assertEquals("count=2,dead=1\ncount=1,dead=0\ncount=1,dead=1",
				store.getQueryModel(firstFile).getTextSummary());

		app.queryDifferentSingleLine(true, 99, true, "'xyz'");
		store = StoreService.getLast();
		String secondFile = store.getLastSavedQueryKey();
		log.debug("Saved to file (second): " + store.getLastSavedQueryKey());
		assertEquals("count=2,dead=1\ncount=2,dead=0\ncount=2,dead=2",
				store.getQueryModel(firstFile).getTextSummary());
		assertEquals("count=3,dead=1\ncount=1,dead=0\ncount=1,dead=1\ncount=1,dead=0",
				store.getQueryModel(secondFile).getTextSummary());
	}

	@Test
	public void testSameQueriesSameLine() throws SQLException {
		if (new Variability().isNetCore()) // ignored for .net (sharpen sets statements in different lines)
			return;
		Configuration.getInstance().setInferQueryParameters(true); // to consider the same parametrized qeury
		app.queryEqualSingleLine("'xyz'", "'aaa'");
		StoreService store = StoreService.getLast();
		String firstFile = StoreService.getLast().getLastSavedQueryKey();
		log.debug("Saved to file: " + firstFile);
		assertEquals("count=3,dead=2\ncount=2,dead=1\ncount=2,dead=1\ncount=2,dead=0",
				store.getQueryModel(firstFile).getTextSummary());
	}

	@Test
	public void testDifferentQueriesDifferentLine() throws SQLException {
		app.queryEqualDifferentLine("'xyz'", "'aaa'");
		StoreService store = StoreService.getLast();
		// only compares the last
		assertEquals("count=3,dead=1\ncount=1,dead=1\ncount=1,dead=0\ncount=1,dead=0",
				store.getQueryModel(store.getLastSavedQueryKey()).getTextSummary());
	}

	@Test
	public void testSameQueriesDifferentLine() throws SQLException {
		Configuration.getInstance().setInferQueryParameters(true); // para que sean consideradas las dos queries como
																	// una misma con parametros
		app.queryEqualDifferentLine("'xyz'", "'aaa'");
		StoreService store = StoreService.getLast();
		// only compares the last
		assertEquals("count=3,dead=1\ncount=1,dead=1\ncount=1,dead=0\ncount=1,dead=0",
				store.getQueryModel(store.getLastSavedQueryKey()).getTextSummary());
	}

	@Test
	public void testAbortByTableExclusion() throws SQLException {
		// full match (implies an abort on the rule evaluation)
		Base.configureTestOptions().setRuleOptions("noboundaries").addTableExclusionExact("test");
		rs = app.queryNoParameters1Condition(-1);
		assertAbort("{}");
		// approximate match (no abort)
		Base.configureTestOptions().setRuleOptions("noboundaries").addTableExclusionExact("tes");
		rs = app.queryNoParameters1Condition(-1);
		assertNoAbort();
	}

	@Test
	public void testAbortByClassExclusion() throws SQLException {
		// full match (abort)
		Base.configureTestOptions().setRuleOptions("noboundaries")
				.addClassExclusion("test4giis.qacoverapp.AppSimpleJdbc");
		rs = app.queryNoParameters1Condition(-1);
		assertAbort("");
		// partial match
		Base.configureTestOptions().setRuleOptions("noboundaries")
				.addClassExclusion("test4giis.qacoverapp.AppSimpleJdb");
		rs = app.queryNoParameters1Condition(-1);
		assertAbort("");
		// no match, normal processing
		Base.configureTestOptions().setRuleOptions("noboundaries")
				.addClassExclusion("test4giis.qacoverapp.AppSimpleJdbx");
		rs = app.queryNoParameters1Condition(-1);
		assertNoAbort();
	}

	// Check if a query has been processed (evaluated)
	private void assertAbort(String expParams) {
		assertEvalResults(false, "", "1 99 xyz", SqlUtil.resultSet2csv(rs, " "), "", expParams, false, false);
		// falta por comprobar el ultimo fichero guardado
		StoreService store = StoreService.getLast();
		assertEquals("", store.getLastSavedQueryKey());
	}
	private void assertNoAbort() {
		assertEvalResults(true, "select id,num,text from test where num>=-1", "1 99 xyz",
				SqlUtil.resultSet2csv(rs, " "), 
				"UNCOVERED SELECT id , num , text FROM test WHERE NOT(num >= -1)\n" +
				"COVERED   SELECT id , num , text FROM test WHERE (num >= -1)",
				"{}", false, false);
		StoreService store = StoreService.getLast();
		String firstFile = store.getLastSavedQueryKey();
		log.debug("Saved to file: " + store.getLastSavedQueryKey());
		assertEquals("count=2,dead=1\ncount=1,dead=0\ncount=1,dead=1", store.getQueryModel(firstFile).getTextSummary());
	}

}
