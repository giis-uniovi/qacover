package test4giis.qacover;

import java.sql.SQLException;

import org.junit.After;
import org.junit.Before;
import org.junit.Test;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import giis.qacover.model.Variability;
import test4giis.qacoverapp.AppDbUtils;
import test4giis.qacoverapp.AppSimpleJdbc;

/**
 * Extension of TestEvaluation for queries created with Apache DbUtils
 */
public class TestEvaluationDbUtils extends Base {
	private static final Logger log=LoggerFactory.getLogger(TestEvaluationDbUtils.class);
	private AppSimpleJdbc app;

	@Before
	@Override
	public void setUp() throws SQLException {
		super.setUp();
		app=new AppSimpleJdbc(variant);
		setUpTestData();
	}
	@After
	@Override
	public void tearDown() throws SQLException{
		super.tearDown();
		app.close();
	}

	public void setUpTestData() {
		app.dropTable("test");
		app.executeUpdateNative(new String[] {
				"create table test(id int not null, num int not null, text varchar(16))", 
				"insert into test(id,num,text) values(1,0,'abc')",
				"insert into test(id,num,text) values(2,99,'xyz')",
				"insert into test(id,num,text) values(3,99,null)"
		});
	}
	
	@Test
	public void testEvalDbUtilsParameters() throws SQLException {
		AppDbUtils app=new AppDbUtils(variant); // Need a different mock application to use DbUtils
		if (new Variability().isNetCore()) //
			return;
		app.queryDbUtils(98,"abc");
		log.debug("Output of native resultset");
		assertEvalResults("select id,num,text from test where text=? and num>?", 
				"", "", 
				"UNCOVERED SELECT id , num , text FROM test WHERE (text = 'abc') AND (num > 98)\n" + 
				"COVERED   SELECT id , num , text FROM test WHERE NOT(text = 'abc') AND (num > 98)\n" + 
				"COVERED   SELECT id , num , text FROM test WHERE (text IS NULL) AND (num > 98)\n" + 
				"UNCOVERED SELECT id , num , text FROM test WHERE (num = 98 + 1) AND (text = 'abc')\n" + 
				"UNCOVERED SELECT id , num , text FROM test WHERE (num = 98) AND (text = 'abc')\n" + 
				"UNCOVERED SELECT id , num , text FROM test WHERE (num = 98 - 1) AND (text = 'abc')");
	}
}
