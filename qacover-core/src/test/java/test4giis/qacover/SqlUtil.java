package test4giis.qacover;

import java.sql.ResultSet;
import java.sql.SQLException;

import giis.qacover.portable.Jdk14StringBuilder;
import giis.qacover.portable.QaCoverException;

public class SqlUtil {
	public static String resultSet2csv(ResultSet rs, String separator) {
		try {
			Jdk14StringBuilder s = new Jdk14StringBuilder();
			int colCount = getColumnCount(rs);
			while (rs.next()) {
				if (s.length() != 0)
					s.append("\n");
				for (int i = 0; i < colCount; i++) {
					String val = rs.getString(i + 1);
					if (rs.wasNull())
						val = "NULL";
					s.append((i == 0 ? "" : separator) + val);
				}
			}
			rs.close();
			return s.toString();
		} catch (SQLException e) {
			throw new QaCoverException(e);
		}
	}

	public static int getColumnCount(ResultSet rs) throws SQLException {
		return rs.getMetaData().getColumnCount();
	}

}
