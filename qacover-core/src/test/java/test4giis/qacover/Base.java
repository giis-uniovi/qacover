package test4giis.qacover;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.fail;

import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.Locale;

import org.junit.After;
import org.junit.Before;
import org.junit.Rule;
import org.junit.rules.TestName;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import giis.qacover.core.QueryStatement;
import giis.qacover.core.services.FaultInjector;
import giis.qacover.core.services.Configuration;
import giis.qacover.core.services.StoreService;
import giis.qacover.model.Variability;
import test4giis.qacoverapp.AppBase;

/**
 * Base class for all tests
 * Includes the setup for the objects that are used by tests:
 * - Variability to consider the platform and SGBD
 * - Setup of the mock test application (qacoverapp package) that
 *   executes queries under the QACover control
 * - Store object for rules
 * - Configuration
 * 
 * Implementation of tests is under the qacover package (with Sqlite) that extends this class.
 * Test for other platforms are in subpackages, inherit the Sqlite tests 
 * and customize the appropriate variables to reuse tests from their superclass.
 * 
 * Most of tests are at the integration level with a DBMS, the jdbcdriver, p6spy and the rules generated.
 * Integration tests according the maven convention (IT) are those that
 * execute outside of maven using the generated jars.
 */
public class Base {
	private static final Logger log = LoggerFactory.getLogger(Base.class);
	protected Variability variant;
	protected ResultSet rs; // the result of the execution of the mock test application
	protected Configuration options;
	protected FaultInjector faultInjector;
	private final static int MAX_PARAMS = 3;
	
	// PLACEHOLDER FOR .NET STATIC INITIALIZATION - DO NOT CHANGE

	@Rule
	public TestName testName = new TestName();

	@Before
	public void setUp() throws SQLException {
		log.info("*** CURRENT TEST - " + testName.getMethodName());
		variant = getVariant();
		log.info("*** CURRENT DBMS - " + variant.getSgbdName());
		rs = null;
		options = Configuration.getInstance().reset().setName("qacovertest");
		new StoreService(options).dropRules().dropLast();
		QueryStatement.setFaultInjector(null);
		Locale.setDefault(new Locale("en", "US"));
	}

	@After
	public void tearDown() throws SQLException {
		if (rs != null)
			rs.close();
	}

	protected Variability getVariant() {
		if (new Variability().isJava4())
			return new Variability("h2");
		else
			return new Variability("sqlite");
	}

	protected void assertEqualsIgnoreCase(String expected, String actual) {
		assertEquals(expected.toLowerCase(), actual.toLowerCase());
	}

	//for compatibility with .NET
	protected void assertEqualsCs(String expected, String actual) {
		if (new Variability().isNetCore())
			assertEqualsIgnoreCase(expected, actual);
		else
			assertEquals(expected, actual);
	}

	protected void assertContains(String expected, String actual) {
		if (!actual.contains(expected))
			fail("Expected not contained in actual: " + actual);
	}

	/**
	 * Custom asserts to check the results of evaluation of rules
	 */
	protected void assertEvalResults(String expSql, String expOutput, String actOutput, String expRule) {
		assertEvalResults(expSql, expOutput, actOutput, expRule, null, false, false);
	}

	protected void assertEvalResults(String expSql, String expOutput, String actOutput, String expRule, String expParams) {
		assertEvalResults(expSql, expOutput, actOutput, expRule, expParams, false, false);
	}

	protected void assertEvalResults(String expSql, String expOutput, String actOutput, String expRule,
			String expParams, boolean removeQuotesAndLines, boolean convertNetParams) {
		assertEvalResults(true, expSql, expOutput, actOutput, expRule, expParams, removeQuotesAndLines, convertNetParams);
	}

	protected void assertEvalResults(boolean expSuccess, String expSql, String expOutput, String actOutput,
			String expRule, String expParams, boolean removeQuotesAndLines, boolean convertNetParams) {
		StoreService store = StoreService.getLast();
		assertEquals(expSuccess ? "success" : "", store.getLastGenStatus());

		String sql = store.getLastSqlRun();
		// needed to test Entity Framework
		sql = removeQuotesAndLines ? sql.replace("\"", "").replace("\n", " ").replace("\r", "") : sql;
		// to compare parametrized queries in ADO.NET
		expSql = convertNetParams ? AppBase.jdbcParamsToAssert(expSql, MAX_PARAMS) : expSql;
		log.debug("Tested sql: " + sql);
		assertEquals(expSql, sql);

		assertEquals(expOutput, actOutput);
		String rules = removeQuotesAndLines ? store.getLastRulesLog().replace("\"", "") : store.getLastRulesLog();
		assertEquals(expRule, rules);

		if (expParams != null) {
			expParams = convertNetParams ? AppBase.ruleParamsToAssert(expParams, MAX_PARAMS) : expParams;
			assertEquals(expParams, store.getLastParametersRun());
		}
	}

}
