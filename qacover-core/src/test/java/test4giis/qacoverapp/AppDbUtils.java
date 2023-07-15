package test4giis.qacoverapp;
import java.sql.*;
import java.util.List;

import org.apache.commons.dbutils.DbUtils;
import org.apache.commons.dbutils.QueryRunner;
import org.apache.commons.dbutils.handlers.BeanListHandler;

import giis.qacover.model.Variability;

/**
 * Different implementation of the base class when testing queries executed with apache commons dbutils
 */
public class AppDbUtils extends AppBase {
	public AppDbUtils(Variability targetVariant) throws SQLException {
		super(targetVariant);
	}

	public List<SimpleEntity> queryDbUtils(Integer param2, String param1) throws SQLException {
		List<SimpleEntity> pojoList; //lista de objetos que seran devueltos por la query
		BeanListHandler<SimpleEntity> beanListHandler=new BeanListHandler<SimpleEntity>(SimpleEntity.class);
		QueryRunner runner=new QueryRunner();
		String sql="select id,num,text from test where text=? and num>?";
		pojoList=runner.query(conn, sql, beanListHandler, param1, param2);
		DbUtils.closeQuietly(conn);
		return pojoList;
	}

}
