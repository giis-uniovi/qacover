package test4giis.qacover.model;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.fail;

import java.sql.SQLException;
import java.text.ParseException;
import java.util.List;

import org.junit.After;
import org.junit.Before;
import org.junit.Test;

import giis.portable.util.FileUtil;
import giis.portable.util.JavaCs;
import giis.qacover.core.services.Configuration;
import giis.qacover.core.services.StoreService;
import giis.qacover.model.RuleModel;
import giis.qacover.model.SchemaModel;
import giis.qacover.model.Variability;
import giis.qacover.reader.CoverageCollection;
import giis.qacover.reader.CoverageReader;
import giis.qacover.reader.QueryCollection;
import giis.qacover.reader.QueryReader;
import test4giis.qacover.Base;
import test4giis.qacoverapp.AppSimpleJdbc;

/**
 * Unit tests for CoverageReader and dependent objects. More integrated tests in TestReport
 */
public class TestReader extends Base {
	private AppSimpleJdbc app;
	// line number of the interaction point used here (different on Java an .NET)(
	private String qline1;

	@Before
	public void setUp() throws SQLException {
		super.setUp();
		app = new AppSimpleJdbc(variant);
		setUpTestData();
		qline1 = new Variability().isNetCore() ? "29" : "23";
	}

	@After
	public void tearDown() throws SQLException {
		super.tearDown();
		app.close();
	}

	public void setUpTestData() {
		app.dropTable("test");
		app.executeUpdateNative(new String[] { "create table test(id int not null, num int not null, text varchar(16))",
				"insert into test(id,num,text) values(1,0,'abc')", });
	}

	// Query evaluation and Coverage reader instantiation
	private CoverageReader getCoverageReader() throws SQLException {
		Configuration.getInstance().setInferQueryParameters(false);
		// Situations: no parameters / parameters / parameters contains vertical bar / query evaluated twice
		rs = app.queryParameters(98, "abc");
		rs.close();
		rs = app.queryNoParameters1Condition(-1); // this must be before if ordering is alphabetic
		rs.close();
		rs = app.queryParameters(98, "a|c");
		StoreService store = StoreService.getLast();
		return new CoverageReader(store.getStoreLocation());
	}

	@Test
	public void testReaderByClass() throws SQLException {
		CoverageReader reader = getCoverageReader();
		// List of classes CoverageCollection
		CoverageCollection classes = reader.getByClass();
		assertEquals(1, classes.size());

		// Content of a class: QueryCollection
		QueryCollection query = classes.get(0);
		assertEqualsCs("test4giis.qacoverapp.AppSimpleJdbc", query.getName());
		assertEquals(2, query.size());

		// Each query: QueryReader
		QueryReader item = query.get(0);
		assertEqualsCs("test4giis.qacoverapp.AppSimpleJdbc", item.getKey().getClassName());
		assertEqualsCs("queryNoParameters1Condition", item.getKey().getMethodName());
		assertEquals(qline1, item.getKey().getClassLine());
		assertEqualsCs(
				"test4giis.qacoverapp.AppSimpleJdbc.queryNoParameters1Condition." + qline1
						+ ".63629c65b13acf17c46df6199346b2fa414b78edfddccf3ba7f875eca30393b3",
				item.getKey().toString());
		assertEquals("select id,num,text from test where num>=-1", item.getSql());
		SchemaModel schema = item.getSchema();
		assertEquals("test", schema.getModel().getEntities().get(0).getName());
		assertEquals("id", schema.getModel().getEntities().get(0).getAttributes().get(0).getName());

		// this view is static, no execution data
		assertEquals("", item.getTimestamp());
		assertEquals("", item.getParametersXml());

		// can access the QueryModel data
		assertEquals("select id,num,text from test where num>=-1", item.getModel().getSql());
		List<RuleModel> allRules = item.getModel().getRules();
		assertEquals(3, allRules.size());
		assertEquals("SELECT id , num , text FROM test WHERE (num = 0)", allRules.get(0).getSql());
		assertEquals("SELECT id , num , text FROM test WHERE (num = -1)", allRules.get(1).getSql());
		assertEquals("SELECT id , num , text FROM test WHERE (num = -2)", allRules.get(2).getSql());

		// basic comprobation of a second item
		item = query.get(1);
		assertEqualsCs("test4giis.qacoverapp.AppSimpleJdbc", item.getKey().getClassName());
		assertEqualsCs("queryParameters", item.getKey().getMethodName());

	}

	@Test
	public void testReaderByClassAsString() throws SQLException {
		CoverageReader reader = getCoverageReader();
		CoverageCollection classes = reader.getByClass();
		String classesStr = "CoverageCollection:\n" + "QueryCollection: test4giis.qacoverapp.AppSimpleJdbc\n"
				+ "  queryNoParameters1Condition\n" + "  queryParameters";
		assertEqualsCs(classesStr, classes.toString());
		assertEqualsCs(classesStr, classes.toString(false, false, false));
		String classesStrAll = "CoverageCollection: qcount=2,qerror=0,count=9,dead=1,error=0\n"
				+ "QueryCollection: test4giis.qacoverapp.AppSimpleJdbc qcount=2,qerror=0,count=9,dead=1,error=0\n"
				+ "  queryNoParameters1Condition:23 test4giis.qacoverapp.AppSimpleJdbc.queryNoParameters1Condition.23.63629c65b13acf17c46df6199346b2fa414b78edfddccf3ba7f875eca30393b3\n"
				+ "  queryParameters:29 test4giis.qacoverapp.AppSimpleJdbc.queryParameters.29.d4a43c80328b80e4552866634547537e7254a10aba076820f905c4617b60aff9";
		if (new Variability().isJava()) // ignores in .net (too many differences to compare)
			assertEqualsCs(classesStrAll, classes.toString(true, true, true));
		assertEquals("qcount=2,qerror=0,count=9,dead=1,error=0", classes.getSummary().toString());

		QueryCollection query = classes.get(0);
		assertEqualsCs("QueryCollection: test4giis.qacoverapp.AppSimpleJdbc\n" + "  queryNoParameters1Condition\n"
				+ "  queryParameters", query.toString());
		assertEquals("qcount=2,qerror=0,count=9,dead=1,error=0", query.getSummary().toString());
	}

	@Test
	public void testReaderByRunOrder() throws SQLException, ParseException {
		CoverageReader reader = getCoverageReader();
		// This should return the collection with an evaluation in each item
		QueryCollection query = reader.getByRunOrder();
		assertEquals(3, query.size());

		QueryReader item = query.get(0);
		assertEqualsCs("test4giis.qacoverapp.AppSimpleJdbc", item.getKey().getClassName());
		assertEqualsCs("queryParameters", item.getKey().getMethodName());
		assertEqualsCs(sqlCs("SELECT id , num , text FROM test WHERE num > ?1? AND text = ?2?"), item.getSql());

		JavaCs.parseIsoDate(item.getTimestamp());
		assertEquals(sqlCs(
				"<parameters><parameter name=\"?1?\" value=\"98\" /><parameter name=\"?2?\" value=\"'abc'\" /></parameters>"),
				item.getParametersXml());

		item = query.get(1);
		assertEqualsCs("test4giis.qacoverapp.AppSimpleJdbc", item.getKey().getClassName());
		assertEqualsCs("queryNoParameters1Condition", item.getKey().getMethodName());
		assertEquals("select id,num,text from test where num>=-1", item.getSql());
		JavaCs.parseIsoDate(item.getTimestamp());
		assertEquals("<parameters></parameters>", item.getParametersXml());

		item = query.get(2);
		assertEqualsCs("test4giis.qacoverapp.AppSimpleJdbc", item.getKey().getClassName());
		assertEqualsCs("queryParameters", item.getKey().getMethodName());
		assertEqualsCs(sqlCs("SELECT id , num , text FROM test WHERE num > ?1? AND text = ?2?"), item.getSql());
		JavaCs.parseIsoDate(item.getTimestamp());
		assertEquals(sqlCs(
				"<parameters><parameter name=\"?1?\" value=\"98\" /><parameter name=\"?2?\" value=\"'a|c'\" /></parameters>"),
				item.getParametersXml());
	}

	private String sqlCs(String sql) { // for compatibility between java and .net parameters
		if (new Variability().isNetCore())
			return sql.replace(" , ", ",").replace(" = ", "=").replace(" > ", ">").replace("?1?", "@param1")
					.replace("?2?", "@param2");
		else
			return sql;
	}

	// Main invalid situations

	@Test
	public void testReaderInvalidFolderNotExist() {
		try {
			CoverageReader reader = new CoverageReader("pathnotexist");
			reader.getByClass();
			fail("Deberia producirse una excepcion");
		} catch (RuntimeException e) {
			assertEquals("Can't browse directory at path pathnotexist", e.getMessage());
		}
	}

	@Test
	public void testReaderInvalidIndexNotExist() {
		try {
			CoverageReader reader = new CoverageReader("."); // folder exists, but no index
			reader.getByRunOrder();
			fail("Deberia producirse una excepcion");
		} catch (RuntimeException e) {
			assertContains("Error reading file", e.getMessage());
			assertContains("00HISTORY.log", e.getMessage());
		}
	}

	@Test
	public void testJavaCsFilePath() {
		// getPath uses apache commons to concatenate paths, but returns null
		// if first parameter is relative.
		// Check that patch to solve this works
		assertContains("aa/xx", FileUtil.getPath("aa", "xx").replace("\\", "/"));
		assertContains("aa/xx", FileUtil.getPath("./aa", "xx").replace("\\", "/"));
		assertContains("bb/aa/xx", FileUtil.getPath("../bb/aa", "xx").replace("\\", "/"));
		assertContains("bb/aa/xx", FileUtil.getPath("../../bb/aa", "xx").replace("\\", "/"));
	}
}
