import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.sql.Statement;

public class TestUber {

	public static final String URL="jdbc:p6spy:sqlite:TestUber.db";
	//public static final String URL="jdbc:sqlite:TestUber.db";
	
	public static void createDb() throws SQLException {
		Connection cn=DriverManager.getConnection(URL);
		Statement stmt = cn.createStatement(); //NOSONAR
		stmt.executeUpdate("drop table if exists test");
		stmt.executeUpdate("create table test(id int not null, num int not null, text varchar(16))");
		stmt.executeUpdate("insert into test(id,num,text) values(1,0,'abc')");
		stmt.close();
		cn.close();
	}
	public static void runQuery1() throws SQLException {
		Connection cn=DriverManager.getConnection(URL);
		Statement stmt = cn.createStatement(); //NOSONAR
		ResultSet rs=stmt.executeQuery("select * from test where text = 'abc'");
		rs.close();
		stmt.close();
		rs.close();
	}
	public static void runQuery2() throws SQLException {
		Connection cn=DriverManager.getConnection(URL);
		Statement stmt = cn.createStatement(); //NOSONAR
		ResultSet rs=stmt.executeQuery("select * from test where text = 'xyz'");
		rs.close();
		stmt.close();
		rs.close();
	}
	
	public static void main(String args[]) {
		try {
			System.out.println("Create database");
			createDb();
			System.out.println("Query database");
			runQuery1();
			System.out.println("End");
		} catch (SQLException e) {
			throw new RuntimeException(e);
		}
	}

}
