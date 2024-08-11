package test4giis.qacover;

import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertTrue;

import java.sql.SQLException;
import java.util.ArrayList;
import java.util.List;

import org.junit.After;
import org.junit.Before;
import org.junit.Test;

import giis.qacover.driver.QueryStatementReader;
import giis.qacover.model.Variability;
import test4giis.qacoverapp.AppSimpleJdbc;

public class TestMutation extends Base {
	private AppSimpleJdbc app;

	@Before
	@Override
	public void setUp() throws SQLException {
		super.setUp();
		app = new AppSimpleJdbc(variant);
		setUpTestData();
	}

	@After
	@Override
	public void tearDown() throws SQLException {
		super.tearDown();
		app.close();
	}

	public void setUpTestData() {
		app.dropTable("test");
		app.executeUpdateNative(new String[] { 
				"create table test(id int, txt varchar(16))",
				"insert into test(id,txt) values(1,'abc')", 
				"insert into test(id,txt) values(2,null)" 
				});
	}

	@Test
	public void testEvalMutants() throws SQLException {
		configureTestOptions().setRuleServiceType("mutation")
			.setFpcServiceOptions("nomutate=equivalent,ir,abs,uoi,lcr,nl");
		rs = app.queryMutParameters("abc");
		assertEvalResults("select id,txt from test where txt=?", "1 abc", SqlUtil.resultSet2csv(rs, " "),
				"UNCOVERED SELECT DISTINCT id , txt FROM test WHERE txt = 'abc'\n"
						+ "COVERED   SELECT id , txt FROM test WHERE txt <> 'abc'\n"
						+ "COVERED   SELECT id , txt FROM test WHERE txt > 'abc'\n"
						+ "COVERED   SELECT id , txt FROM test WHERE txt < 'abc'\n"
						+ "UNCOVERED SELECT id , txt FROM test WHERE txt >= 'abc'\n"
						+ "UNCOVERED SELECT id , txt FROM test WHERE txt <= 'abc'\n"
						+ "COVERED   SELECT id , txt FROM test WHERE (1=1)\n"
						+ "COVERED   SELECT id , txt FROM test WHERE (1=0)",
				"{?1?='abc'}", false, new Variability().isNetCore());
	}

	// Detailed evaluation of differences
	@Test
	public void testDataCompare() throws SQLException {
		QueryStatementReader reader = new QueryStatementReader(app.getConnectionNative(), "select id,txt from test order by id");

		// equality
		List<String[]> rows = reader.getRows();
		assertTrue(reader.equalRows(getList()));
		assertTrue(reader.equalRows(rows));

		// differences in row sizes
		rows = getList();
		rows.remove(1);
		assertFalse(reader.equalRows(rows));
		rows = getList();
		rows.add(new String[] { "3", "zzz" });
		assertFalse(reader.equalRows(rows));

		// differences in col sizes
		rows = getList();
		rows.set(0, new String[] { "1", "abc", "def" });
		assertFalse(reader.equalRows(rows));
		rows = getList();
		rows.set(1, new String[] { "2" });
		assertFalse(reader.equalRows(rows));

		// Difference in values not null, null and not null, not null and null
		rows = getList();
		rows.get(0)[1] = "abcd";
		assertFalse(reader.equalRows(rows));
		rows = getList();
		rows.get(1)[1] = "xyz";
		assertFalse(reader.equalRows(rows));
		rows = getList();
		rows.get(0)[0] = null;
		assertFalse(reader.equalRows(rows));
	}

	private List<String[]> getList() {
		List<String[]> lst = new ArrayList<>();
		lst.add(new String[] { "1", "abc" });
		lst.add(new String[] { "2", null });
		return lst;
	}

}
