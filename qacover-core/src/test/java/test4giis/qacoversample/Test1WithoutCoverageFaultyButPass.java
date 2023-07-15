package test4giis.qacoversample;

import org.apache.commons.dbutils.QueryRunner;
import org.apache.commons.dbutils.handlers.MapListHandler;
import org.junit.AfterClass;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TestName;

import giis.qacover.report.ReportManager;

import static org.junit.Assert.assertEquals;

import java.sql.*;
import java.util.List;
import java.util.Map;

/**
 * A sample of using QACover to evaluate the query coverage and using the
 * coverage information to improve the fault detection capability of the tests.
 * 
 * First scenario with a test that passes but does not reveal hidden defects
 */
public class Test1WithoutCoverageFaultyButPass extends BaseTest {

	/**
	 * This query and the method simulates a business process that gets the salary of an employee as:
	 *  - the base salary
	 *  - plus the average of bonus from projects in whith the employee has an assignment of at least 100 hours
	 */
	protected String query = "SELECT E.idEmp, basesalary, avg(Bonus) as bonus " +
				"FROM Employee E INNER JOIN Assignment A ON E.idEmp = A.idEmp " +
				"INNER JOIN Project P ON A.idProj = P.idProj " +
				"WHERE E.idEmp = ? AND A.hours >= 100 " +
				"GROUP BY E.idEmp";

	protected double getSalaryWithBonus(Connection conn, int idEmp) throws SQLException {
		List<Map<String, Object>> ret = new QueryRunner().query(conn, query, new MapListHandler(), idEmp);
		double basesalary = (Double) ret.get(0).get("basesalary");
		double bonus = (Double) ret.get(0).get("bonus");
		return basesalary + bonus;
	}

	@Before
	public void setUp() throws SQLException {
		super.setUp();
		setUpTestData();
	}

	// Test situations for the first test (determined manually)
	// For the employee to include in the test (13)
	// - there are other employees with bonus
	// - has any assignment with less than 100 hours (must be ignore)
	// - different bonus values with equal and different values to check the average calculation
	public void setUpTestData() throws SQLException {
		executeUpdateNative(new String[] {
				"insert into Employee(idEmp,baseSalary) values (12, 20000)",
				"insert into Employee(idEmp,baseSalary) values (13, 30000)",
				"insert into Project(idProj,bonus) values (21, 1000)",
				"insert into Project(idProj,bonus) values (22, 1000)",
				"insert into Project(idProj,bonus) values (23, 2000)",
				"insert into Project(idProj,bonus) values (24, 1400)",
				"insert into Assignment(idEmp,idProj,hours) values (12,21,111)",
				"insert into Assignment(idEmp,idProj,hours) values (13,21,100)",
				"insert into Assignment(idEmp,idProj,hours) values (13,22,101)",
				"insert into Assignment(idEmp,idProj,hours) values (13,23,102)",
				"insert into Assignment(idEmp,idProj,hours) values (13,24,99)",
		});
	}

	@Rule
	public TestName name = new TestName();

	@Test
	public void testInitial() throws SQLException {
		assertEquals(31333.33, getSalaryWithBonus(conn, 13), 0.009);
	}

	@AfterClass
	public static void generateReport() {
		new ReportManager().run("target/qacover/rules", "target/qacover/demo-reports1");
	}
	
}
