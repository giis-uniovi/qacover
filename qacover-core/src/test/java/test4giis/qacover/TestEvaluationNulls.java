package test4giis.qacover;

import java.sql.*;

import org.junit.After;
import org.junit.Before;
import org.junit.Test;

import giis.qacover.core.services.Configuration;
import giis.qacover.model.Variability;
import test4giis.qacoverapp.AppSimpleJdbc;

/**
 * Handling of nulls in rule generation and evaluation
 */
public class TestEvaluationNulls extends Base {
	private AppSimpleJdbc app;

	@Before
	public void setUp() throws SQLException {
		super.setUp();
		app = new AppSimpleJdbc(variant);
		setUpTestData();
	}

	@After
	public void tearDown() throws SQLException {
		super.tearDown();
		app.close();
	}

	public void setUpTestData() {
		app.dropTable("test");
		app.executeUpdateNative(new String[] {
				"create table test(id int not null, num int not null, text varchar(16))", 
				"insert into test(id,num,text) values(1,0,'abc')",
				"insert into test(id,num,text) values(3,99,null)"
		});
	}
	
	@Test
	public void testParameterAsNullValue() throws SQLException {
		doParameterSetNull(false); // assigning null value
	}
	@Test
	public void testParameterAsSetNull() throws SQLException {
		doParameterSetNull(true); // using setNull
	}
	public void doParameterSetNull(boolean useSetNull) throws SQLException {
		Configuration.getInstance().setInferQueryParameters(false);
		String sql="select id,num,text from test where text=?";
		rs=app.executeQueryNulls(sql, null, useSetNull);
		assertEvalResults(sql, 
				"", SqlUtil.resultSet2csv(rs," "), 
				"UNCOVERED SELECT id , num , text FROM test WHERE NOT(text = NULL)\n" + 
				"UNCOVERED SELECT id , num , text FROM test WHERE (text = NULL)\n" + 
				"COVERED   SELECT id , num , text FROM test WHERE (text IS NULL)",
				"{?1?=NULL}", false, new Variability().isNetCore());
	}
	
	/**
	 * With parameter inference, query has been parsed, but the parameter is not shown in the result,
	 * that means the NULL value is not considered in the parameter inference
	 */
	@Test
	public void testParameterInferNull() throws SQLException {
		Configuration.getInstance().setInferQueryParameters(true);
		String sql="select id,num,text from test where text=NULL";
		rs=app.executeQuery(sql);
		assertEvalResults("SELECT id , num , text FROM test WHERE text = NULL", 
				"", SqlUtil.resultSet2csv(rs," "), 
				"UNCOVERED SELECT id , num , text FROM test WHERE NOT(text = NULL)\n" + 
				"UNCOVERED SELECT id , num , text FROM test WHERE (text = NULL)\n" + 
				"COVERED   SELECT id , num , text FROM test WHERE (text IS NULL)",
				"{}", false, new Variability().isNetCore());
	}
	
}
