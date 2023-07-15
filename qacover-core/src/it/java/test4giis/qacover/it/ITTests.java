package test4giis.qacover.it;

import static org.junit.Assert.assertTrue;

import org.junit.After;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TestName;

import giis.portable.util.FileUtil;
import giis.visualassert.Framework;
import giis.visualassert.SoftVisualAssert;
import test4giis.qacover.model.TestReport;

/**
 * Las pruebas de integracion ejecutan las diferentes acciones desde fuera del contexto de maven
 * usando aplicaciones que enlazan con los jar desplegados.
 * En pre-integration-test se realiza esta ejecucion desde ant con el script definido en build.xml
 * y en integration-test se realizan los assert correspondientes a esta clase
 */
public class ITTests {
	private String bmk="src/test/resources";
	private SoftVisualAssert va;
	
	@Rule public TestName testName = new TestName();

	@Before
	public void setUp() {
		va = new SoftVisualAssert().setFramework(Framework.JUNIT4).setCallStackLength(3)
				.setBrightColors(true).setReportSubdir("target"); 
	}
	@After
	public void tearDown() {
		va.assertAll("diff-aggregated-" + testName.getMethodName() + ".html"); 
	}
	
	/**
	 * Compara los datos consultados usando qacover-model.jar como un api para acceder a los datos
	 * desde it/qacover-api-sample que han utilizado las reglas empleadas en TestReport en ut
	 */
	@Test
	public void testItQacoverApi() {
		String expFolder=FileUtil.getPath(bmk, "qacover-api-sample");
		String actFolder=FileUtil.getPath("..", "it", "qacover-api-sample", "target", "qacover", "reports");
		assertFiles(expFolder, actFolder, "by-class.txt");
		assertFiles(expFolder, actFolder, "by-run-order.txt", true);
	}
	
	/**
	 * Compara reglas generadas desde una aplicacion con qacover.jar por medio de los reports generados
	 * (usando spring petclinic)
	 */
	@Test
	public void testItQacoverSpring() {
		String expFolder=FileUtil.getPath(bmk, "spring-petclinic-main");
		String actFolder=FileUtil.getPath("..", "it", "spring-petclinic-main", "target", "qacover", "reports");
		assertFiles(expFolder, actFolder, "index.html", true);
		assertFiles(expFolder, actFolder, "org.springframework.samples.petclinic.PetclinicIntegrationTests.html");
	}

	/**
	 * Compara reglas generadas desde una aplicacion que utiliza el uber jar por medio de los reports generados
	 * (usando qacover-uber-main, programa simple)
	 */
	@Test
	public void testItQacoverUber() {
		String expFolder=FileUtil.getPath(bmk, "qacover-uber-main");
		String actFolder=FileUtil.getPath("..", "it", "qacover-uber-main", "target", "qacover", "reports");
		assertFiles(expFolder, actFolder, "index.html", true);
		assertFiles(expFolder, actFolder, "TestUber.html");
	}
	@Test
	public void testItQacoverUberLogs() {
		// Check the summary output produced by report execution
		String actFolder=FileUtil.getPath("..", "it", "qacover-uber-main", "target", "qacover", "reports");
		String actual=FileUtil.fileRead(FileUtil.getPath(actFolder, "report.log"));
		String expected="Report for class: TestUber qcount=1,qerror=0,count=3,dead=1,error=0";
		assertTrue("Expected not contained in actual: " + actual, actual.contains(expected));
		
		// Check the summary of the own application log
		actFolder=FileUtil.getPath("..", "it", "qacover-uber-main", "target");
		actual=FileUtil.fileRead(FileUtil.getPath(actFolder, "qacover-uber-log.log"));
		expected="INFO  giis.qacover.core.CoverageManager -  SUMMARY: Covered 1 out of 3";
		assertTrue("Expected not contained in actual: " + actual, actual.contains(expected));
	}

	//compara ficheros del mismo nombre pero localizados en diferentes folders
	private void assertFiles(String expectedFolder, String actualFolder, String fileName) {
		assertFiles(expectedFolder, actualFolder, fileName, false);
	}
	//compara ficheros previo remplazo de expresiones regulares en ambos ficheros
	private void assertFiles(String expectedFolder, String actualFolder, String fileName, boolean doRegex) {
		String expected=FileUtil.fileRead(FileUtil.getPath(expectedFolder, fileName)).replaceAll("\r", "");
		String actual=FileUtil.fileRead(FileUtil.getPath(actualFolder, fileName)).replaceAll("\r", "");
		expected=TestReport.reprocessReportForCompare(expected);
		actual=TestReport.reprocessReportForCompare(actual);
		va.assertEquals(expected, actual, "Differences comparing "+fileName, ("diff-it-"+expectedFolder+"-"+fileName+".html").replace("/","-").replace("\\","-"));
	}
}
