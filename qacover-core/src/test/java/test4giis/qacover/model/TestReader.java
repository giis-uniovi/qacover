package test4giis.qacover.model;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertNotNull;
import static org.junit.Assert.assertNull;
import static org.junit.Assert.fail;

import java.sql.SQLException;
import java.text.ParseException;
import java.util.List;

import org.junit.After;
import org.junit.Before;
import org.junit.Test;

import giis.portable.util.FileUtil;
import giis.qacover.core.services.Configuration;
import giis.qacover.core.services.StoreService;
import giis.qacover.model.HistoryModel;
import giis.qacover.model.QueryKey;
import giis.qacover.model.RuleModel;
import giis.qacover.model.SchemaModel;
import giis.qacover.model.Variability;
import giis.qacover.reader.CoverageCollection;
import giis.qacover.reader.CoverageReader;
import giis.qacover.reader.HistoryReader;
import giis.qacover.reader.QueryCollection;
import giis.qacover.reader.QueryReader;
import giis.qacover.reader.SourceCodeCollection;
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

	// Collect the history items to access to the list of parameters used to run
	// each query using a V1 history store format
	@Test
	public void testHistoryReaderV1() throws SQLException, ParseException {
		//Creates a coverage reader from the already saved v1 files
		CoverageReader reader = new CoverageReader(new giis.qacover.model.Variability().isJava()
				? "src/test/resources/historyV1"
				: "../../../../../qacover-core/src/test/resources/historyV1");
		HistoryReader all = reader.getHistory();

		// Reads all classes to get the query keys used to select queries in the history
		CoverageCollection classes = reader.getByClass();
		assertEquals(1, classes.size());
		QueryCollection query = classes.get(0);
		assertEqualsCs("test4giis.qacoverapp.appsimplejdbc", query.getName().toLowerCase());
		assertEquals(2, query.size()); // two methods

		// First query has only one execution
		assertEquals("querynoparameters1condition", query.get(0).getKey().getMethodName().toLowerCase());
		HistoryReader history = all.getHistoryAtQuery(query.get(0).getKey());
		List<HistoryModel> model = history.getItems();
		assertEquals(1, model.size());
		assertEquals("<parameters></parameters>", model.get(0).getParamsXml());

		// Second query has two executions
		assertEquals("queryparameters", query.get(1).getKey().getMethodName().toLowerCase());
		history = all.getHistoryAtQuery(query.get(1).getKey());
		model = history.getItems();
		assertEquals(2, model.size());
		assertEquals("<parameters><parameter name=\"?1?\" value=\"98\" /><parameter name=\"?2?\" value=\"'abc'\" /></parameters>",
				model.get(0).getParamsXml());
		assertEquals("<parameters><parameter name=\"?1?\" value=\"98\" /><parameter name=\"?2?\" value=\"'a|c'\" /></parameters>",
				model.get(1).getParamsXml());

		// Invalid query, creates a query key by changing the class name of an existing key
		QueryKey invalid = new QueryKey(query.get(0).getKey().getKey().replace("AppSimpleJdbc", "InvalidClass"));
		history = all.getHistoryAtQuery(invalid);
		model = history.getItems();
		assertEquals(0, model.size());
	}

	// Collect the history items to access to the list of parameters used to run
	// each query
	@Test
	public void testHistoryReader() throws SQLException, ParseException {
		CoverageReader reader = getCoverageReader();
		HistoryReader all = reader.getHistory();

		// Reads all classes to get the query keys used to select queries in the history
		CoverageCollection classes = reader.getByClass();
		assertEquals(1, classes.size());
		QueryCollection query = classes.get(0);
		assertEqualsCs("test4giis.qacoverapp.appsimplejdbc", query.getName().toLowerCase());
		assertEquals(2, query.size()); // two methods

		// First query has only one execution
		assertEquals("querynoparameters1condition", query.get(0).getKey().getMethodName().toLowerCase());
		HistoryReader history = all.getHistoryAtQuery(query.get(0).getKey());
		List<HistoryModel> model = history.getItems();
		assertEquals(1, model.size());
		assertEquals("[]", model.get(0).getParamsJson());
		assertEquals("#oo", model.get(0).getResult());

		// Second query has two executions
		assertEquals("queryparameters", query.get(1).getKey().getMethodName().toLowerCase());
		history = all.getHistoryAtQuery(query.get(1).getKey());
		model = history.getItems();
		assertEquals(2, model.size());
		assertEquals(javacsparm("[{\"name\":\"?1?\",\"value\":\"98\"},{\"name\":\"?2?\",\"value\":\"'abc'\"}]"),
				model.get(0).getParamsJson());
		assertEquals("oooooo", model.get(0).getResult());
		assertEquals(javacsparm("[{\"name\":\"?1?\",\"value\":\"98\"},{\"name\":\"?2?\",\"value\":\"'a|c'\"}]"),
				model.get(1).getParamsJson());
		assertEquals("oooooo", model.get(1).getResult());

		// Invalid query, creates a query key by changing the class name of an existing key
		QueryKey invalid = new QueryKey(query.get(0).getKey().getKey().replace("AppSimpleJdbc", "InvalidClass"));
		history = all.getHistoryAtQuery(invalid);
		model = history.getItems();
		assertEquals(0, model.size());
	}
	private String javacsparm(String params) {
		return new Variability().isJava() ? params : params.replace("?1?", "@param1").replace("?2?", "@param2");
	}
	
	@Test
	public void testReaderEmptyIfIndexNotExist() {
		CoverageReader reader = new CoverageReader("."); // folder exists, but no index, do not throw exception
		HistoryReader history = reader.getHistory();
		assertEquals(0, history.getItems().size());
	}

	// Collect the source code lines with coverage of queries
	// Basic test of main situations: 
	// - only queries, with source, source not found
	// - path with leading/railing spaces
	// - sorting by line
	// - multiple queries in class, multiple queries in line (tested in TestReport)
	// Integrated test in TestReport
	@Test
	public void testSourceCodeCollection() throws SQLException {
		boolean isJava = new Variability().isJava();
		QueryCollection queries = getCoverageReader().getByClass().get(0);
		
		// (1) Only queries, no source code
		SourceCodeCollection sources = new SourceCodeCollection();
		sources.addQueries(queries);
		assertEquals(2, sources.getLines().size());

		// Position of each query and basic content
		List<Integer> keys = sources.getLineNumbers();
		int key0 = keys.get(0);
		int key1 = keys.get(1);
		assertEquals(1, sources.getLines().get(key0).getQueries().size());
		assertEquals(1, sources.getLines().get(key1).getQueries().size());
		assertNull(sources.getLines().get(key0).getSource());
		assertNull(sources.getLines().get(key1).getSource());
		
		// Order of execution: queryParameters queryNoParameters1Condition queryParameters
		// but the second is in a line before the first, it appear at the first position
		assertEquals("select id,num,text from test where num>=-1", 
				sources.getLines().get(key0).getQueries().get(0).getSql());
		assertEquals(isJava
				? "SELECT id , num , text FROM test WHERE num > ?1? AND text = ?2?"
				: "select id,num,text from test where num>@param1 and text=@param2", 
				sources.getLines().get(key1).getQueries().get(0).getSql());
		
		// Test location of source code (net stores an absolute path, note that this test is run 4 levels below solution folder)
		assertEquals(isJava
				? "test4giis/qacoverapp/AppSimpleJdbc.java"
				: FileUtil.getFullPath("../../../../QACoverTest/Translated/Test4giis.Qacoverapp/AppSimpleJdbc.cs").replace("\\", "/"), 
				sources.getLines().get(key0).getQueries().get(0).getModel().getSourceLocation());
		
		// Source folder/files setup. In net the rules have an absolute path that requires a projectRoot to resolve
		String projectFolder = isJava ? "" : "../../../../../net"; // this solution root
		String sourceFolder = isJava
				? "src/test/java"
				: "../../../.."; // in this case sources are just under project root
		String noSourceFolder = isJava
				? "src/nosources"
				: "../../../../../otherproject/QACoverTest";
		
		// (2) Add source code (found in second path, that requires trim), now there is source content and coverage
		sources.addSources(queries.get(0), noSourceFolder + ", " + sourceFolder + " ", projectFolder);
		assertEquals(1, sources.getLines().get(key0).getQueries().size());
		assertEquals(1, sources.getLines().get(key1).getQueries().size());
		assertNotNull(sources.getLines().get(key0).getSource());
		assertNotNull(sources.getLines().get(key1).getSource());
		// Check first and last line, with source content, without coverage
		int numLines = sources.getLines().size();
		assertEquals(0, sources.getLines().get(1).getQueries().size());
		assertNotNull(sources.getLines().get(1));
		assertEquals(0, sources.getLines().get(numLines).getQueries().size());
		assertNotNull(sources.getLines().get(numLines));
		
		// (3) Source code can't be located at any file
		// Queries are already generated, reset the sources
		sources = new SourceCodeCollection();
		sources.addQueries(queries);
		sources.addSources(queries.get(0), noSourceFolder, projectFolder);
		// Same checks than at the beginning of this test
		assertEquals(2, sources.getLines().size());
		assertEquals(1, sources.getLines().get(key0).getQueries().size());
		assertEquals(1, sources.getLines().get(key1).getQueries().size());
		assertNull(sources.getLines().get(key0).getSource());
		assertNull(sources.getLines().get(key1).getSource());
	}
		
}
