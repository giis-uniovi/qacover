package test4giis.qacoversample;

import org.apache.commons.dbutils.QueryRunner;
import org.apache.commons.dbutils.handlers.MapListHandler;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import giis.qacover.report.ReportManager;

import org.junit.AfterClass;
import org.junit.Ignore;
import org.junit.Test;
import static org.junit.Assert.assertEquals;

import java.sql.*;
import java.util.List;
import java.util.Map;

/**
 * A sample of using QACover to evaluate the query coverage and using the
 * coverage information to improve the fault detection capability of the tests.
 * 
 * Second scenario: After executing the first scenario, the information of the covered rules
 * gives us the clues to include more test data and an additional test to cover all rules.
 * Now the tests are able to reveal two failures.
 * NOTE: Tests have the ignore annotation to run in the CI environment. Remove to check the failures
 */
public class Test2WithCoverageFaultyAndFails extends Test1WithoutCoverageFaultyButPass {
	private static final Logger log=LoggerFactory.getLogger(Test2WithCoverageFaultyAndFails.class);

	/**
	 * The test process is the same, inherits the query to test.
	 * (test code is duplicated to allow QACover locate the interaction point in this class)
	 */
	@Override
	protected double getSalaryWithBonus(Connection conn, int idEmp) throws SQLException {
		List<Map<String, Object>> ret = new QueryRunner().query(conn, query, new MapListHandler(), idEmp);
		double basesalary = (Double) ret.get(0).get("basesalary");
		double bonus = (Double) ret.get(0).get("bonus");
		return basesalary + bonus;
	}

	/**
	 * Based on the uncovered rules adds more test data (a project without bonus) to cover an uncovered rule; 
	 * test uses the same employee.
	 * However, the test fails because now the average takes into account the new project without assignment.
	 */
	@Ignore
	@Test
	public void testProjectWithoutBonus() throws SQLException {
		log.info("Run test method: " + name.getMethodName());
		executeUpdateNative(new String[] {
				"insert into Project(idProj,bonus) values (25, null)",
				"insert into Assignment(idEmp,idProj,hours) values (13, 25, 123)"
		});
		assertEquals(31000.0, getSalaryWithBonus(conn, 13), 0.009);
	}
	
	/**
	 * Based on the uncovered rules, adds a new test with more test data to cover the remaining uncovered rule;
	 * test uses a different employee without projects (15).
	 * However, the test fails with an exception (should return the base salary as there are no bonus)
	 */
	@Ignore
	@Test
	public void testEmployeeWithoutProjects() throws SQLException {
		log.info("Run test method: " + name.getMethodName());
		executeUpdateNative(new String[] { 
				"insert into Employee(idEmp,baseSalary) values (15, 50000)",
		});
		assertEquals(50000.0, getSalaryWithBonus(conn, 15), 0.001);
	}
	
	@AfterClass
	public static void generateReport() {
		new ReportManager().run("target/qacover/rules","target/qacover/demo-reports2");
	}
}
