package test4giis.qacoversample;

import org.apache.commons.dbutils.QueryRunner;
import org.apache.commons.dbutils.handlers.MapListHandler;
import org.apache.commons.dbutils.handlers.ScalarHandler;
import org.junit.AfterClass;
import org.junit.Test;

import giis.qacover.report.ReportManager;

import java.sql.*;
import java.util.List;
import java.util.Map;

/**
 * A sample of using QACover to evaluate the query coverage and using the
 * coverage information to improve the fault detection capability of the tests.
 * 
 * Third scenario: After detecting two hidden faults in the second scenario,
 * both query and the java method are fixed, no all tests pass.
 */
public class Test3WithCoverageFixedAndPass extends Test2WithCoverageFaultyAndFails {
	
	/**
	 * Fixing the query: changes the way in which average is calculated
	 * and checks the number of returned rows to avoid exceptions.
	 */
	@Override
	protected double getSalaryWithBonus(Connection conn, int idEmp) throws SQLException {
		String sql = "SELECT E.idEmp, basesalary, sum(Bonus)/count(*) as bonus " +
				"FROM Employee E INNER JOIN Assignment A ON E.idEmp = A.idEmp " +
				"INNER JOIN Project P ON A.idProj = P.idProj " +
				"WHERE E.idEmp = ? AND A.hours >= 100 " +
				"GROUP BY E.idEmp";
		List<Map<String, Object>> ret = new QueryRunner().query(conn, sql, new MapListHandler(), idEmp);
		if (ret.size() == 0) {
			return new QueryRunner().query(conn, "SELECT basesalary from Employee WHERE idEmp=?", new ScalarHandler<Double>(), idEmp);
		}
		double basesalary = (Double) ret.get(0).get("basesalary");
		double bonus = (Double) ret.get(0).get("bonus");
		return basesalary + bonus;
	}
	
	@Override
	@Test
	public void testProjectWithoutBonus() throws SQLException {
		super.testProjectWithoutBonus();
	}

	@Override
	@Test
	public void testEmployeeWithoutProjects() throws SQLException {
		super.testEmployeeWithoutProjects();
	}

	@AfterClass
	public static void generateReport() {
		new ReportManager().run("target/qacover/rules", "target/qacover/demo-reports3");
	}
	
}
