package test4giis.qacover.model;

import test4giis.qacover.Base;
import test4giis.qacoverapp.AppSimpleJdbc;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertThrows;
import static org.junit.Assert.assertTrue;

import java.sql.SQLException;
import java.util.Properties;

import org.junit.After;
import org.junit.Before;
import org.junit.Test;

import giis.qacover.core.services.Configuration;
import giis.qacover.core.services.StackLocator;
import giis.qacover.model.Variability;
import giis.qacover.reader.CoverageReader;

public class TestUtil {
	// This does not inherits because there is no bd, but variability must be defned
	protected Variability variant = new Variability("sqlite");

	// ensures clean start without custom options
	@Before
	public void setUp() {
		Base.configureTestOptions();
	}

	@After
	public void tearDown() {
		Base.configureTestOptions();
	}

	@Test
	public void testStackTrace() throws SQLException {
		AppSimpleJdbc app = new AppSimpleJdbc(variant);
		StackLocator stack = app.myGetStackTraceTargetMethod();
		assertEquals("test4giis.qacoverapp.appsimplejdbc", stack.getClassName().toLowerCase());
		assertEquals("mygetstacktracetargetmethod", stack.getMethodName().toLowerCase());
		// approximate location of the line
		assertTrue(stack.getLineNumber() > 10);
		assertTrue(stack.getLineNumber() < 50);
	}

	@Test
	public void testStackTraceEmpty() throws SQLException {
		// excludes all packages to achieve an empty call stack
		Configuration.getInstance().addStackExclusion("test4giis").addStackExclusion("giis").addStackExclusion("java.")
				.addStackExclusion("junit.").addStackExclusion("sun.").addStackExclusion("org.")
				.addStackExclusion("microsoft.").addStackExclusion("nunit.");
		AppSimpleJdbc app = new AppSimpleJdbc(variant);
		StackLocator stack = app.myGetStackTraceTargetMethod();
		assertEquals("undefined", stack.getClassName());
		assertEquals("undefined", stack.getMethodName());
		assertEquals(0, stack.getLineNumber());
	}

	public void testClassSummaryFileOrder() {
	}

	// Accessing properties files
	@Test
	public void testProperties() {
		// qacover.properties in the project rootraiz del proyecto
		Configuration opt = Configuration.getInstance();
		Properties prop = opt.getProperties("qacover.properties");
		assertEquals("false", prop.getProperty("qacover.query.infer.parameters"));
		// fortest.properties in src/test/resources, java only
		if (new Variability().isJava()) {
			prop = opt.getProperties("fortest.properties");
			assertEquals("X Y Z", prop.getProperty("property.one"));
		}
		// Not existing
		assertThrows(RuntimeException.class, () -> {
			opt.getProperties("noexiste.properties");
		});
	}

}
