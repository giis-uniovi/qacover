package test4giis.qacoversample;

import org.apache.commons.dbutils.DbUtils;
import org.junit.After;
import org.junit.Before;
import org.junit.BeforeClass;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import giis.qacover.core.services.Configuration;
import giis.qacover.core.services.StoreService;

import java.sql.*;

/**
 * A sample of using QACover to evaluate the query coverage and using the
 * coverage information to improve the fault detection capability of the tests.
 * 
 * Base class with the setup
 */
public class BaseTest {
	private static final Logger log = LoggerFactory.getLogger(BaseTest.class);
	protected static final String DRIVER = "org.sqlite.JDBC";
	protected static final String URL = "jdbc:p6spy:sqlite:TestDB.db";
	protected Connection conn = null;

	@Before
	public void setUp() throws SQLException {
		conn = DriverManager.getConnection(URL);
		setUpTestDatabase();
	}

	private void setUpTestDatabase() {
		executeUpdateNative(new String[] {
				"drop table if exists Assignment",
				"drop table if exists Project",
				"drop table if exists Employee",
				"create table Employee(idEmp int not null primary key, basesalary double not null)",
				"create table Project(idProj int not null primary key, bonus double)",
				"create table Assignment(idEmp int not null, idProj int not null, hours int not null, primary key(idEmp,idProj)"
						+ ", foreign key(idEmp) references Employee(idEmp), foreign key(idProj) references Project(idProj) )" });
	}

	@After
	public void tearDown() throws SQLException {
		DbUtils.closeQuietly(conn);
	}

	// Reset to run each test class independently from the others and get
	// independent coverage result
	@BeforeClass
	public static void classSetUp() {
		log.info("Reset coverage parameters");
		Configuration options = Configuration.getInstance().reset().setName("qacoversample")
				.setFpcServiceOptions("noboundaries notautology");
		// instantiates an store to reset rules
		new StoreService(options).dropRules().dropLast();
	}

	/**
	 * Execute queries against the native connection, used in test setup
	 */
	protected void executeUpdateNative(String[] sqlArray) {
		Connection cn = null;
		Statement stmt = null;
		try { // no usa try with resources para compatibilidad con java 1.6
			cn = DriverManager.getConnection(URL.replace(":p6spy:", ":")); // usa el driver nativo
			stmt = cn.createStatement();
			for (String sql : sqlArray) {
				log.info("Load test data: " + sql);
				stmt.executeUpdate(sql);
			}
		} catch (SQLException e) {
			throw new RuntimeException(e); // NOSONAR
		} finally {
			try {
				if (stmt != null)
					stmt.close();
			} catch (Exception e) {
			}
			;
			try {
				if (cn != null)
					cn.close();
			} catch (Exception e) {
			}
			;
		}
	}

}
