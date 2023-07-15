package test4giis.qacoverapp;

import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.SQLException;
import java.sql.Statement;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import giis.portable.util.FileUtil;
import giis.portable.util.Parameters;
import giis.qacover.model.Variability;
import giis.tdrules.store.rdb.JdbcProperties;

/**
 * Creation of connections (intercepted or native) for all platforms.
 */
public class ConnectionFactory {
	private static final Logger log=LoggerFactory.getLogger(ConnectionFactory.class);

	private static final String SETUP_PATH = FileUtil.getPath(Parameters.getProjectRoot(), "..", "setup");
	private static final String ENVIRONMENT_PROPERTIES = FileUtil.getPath(SETUP_PATH, "environment.properties");
	private static final String DATABASE_PROPERTIES = FileUtil.getPath(SETUP_PATH, "database.properties");

	private Variability variant;
	private String driver;
	private String url;
	private String user;
	private String password;

	public ConnectionFactory(Variability dbmsVariant) {
		variant = dbmsVariant;
		String propPrefix = "qacover." + variant.getPlatformName() + ".qacoverdb." + variant.getSgbdName();
		// En java14 el driver no es el de la bd, sino el de p4spy
		if (variant.isJava4())
			driver = new JdbcProperties().getProp(DATABASE_PROPERTIES, "qacover.java4.p6spy.driver");
		else
			driver = new JdbcProperties().getProp(DATABASE_PROPERTIES, propPrefix + ".driver");
		url = new JdbcProperties().getProp(DATABASE_PROPERTIES, propPrefix + ".url");
		user = new JdbcProperties().getProp(DATABASE_PROPERTIES, propPrefix + ".user");
		password = new JdbcProperties().getEnvVar(ENVIRONMENT_PROPERTIES,
				"TEST_" + variant.getSgbdName().toUpperCase() + "_PWD");
		log.trace("Create Connection: Driver=" + driver + " Url=" + url + " User=" + user);
	}

	/**
	 * Gets a connection for tests (intercepted)
	 */
	public Connection getConnection() throws SQLException {
		Connection conn;
		if (variant.isJava4()) {
			// jdk 1.4 requires register the driver class
			registerDriverClass();
			// also this connection must be done using the native url!!!
			conn = DriverManager.getConnection(getNativeUrl(url), user, password);
		} else {
			conn = DriverManager.getConnection(url, user, password);
		}
		if (variant.isOracle()) {
			// Oracle requires configure date format to match the specified in spy.properties
			Statement stmt = conn.createStatement();
			stmt.execute("ALTER SESSION SET NLS_DATE_FORMAT = 'yyyy-MM-dd'");
			stmt.close();
		}
		return conn;
	}

	/**
	 * Gets a native connection to use in test data setup
	 */
	public Connection getNativeConnection() throws SQLException {
		return DriverManager.getConnection(getNativeUrl(url), user, password);
	}

	// Urls in the database setup always contain the p6spy substring, remove for the native connection
	private String getNativeUrl(String targetUrl) {
		return targetUrl.replaceAll(":p6spy:", ":");
	}

	private void registerDriverClass() {
		try {
			Class.forName(driver);
		} catch (ClassNotFoundException e) {
			throw new RuntimeException(e);
		}
	}

}
