using Java.Sql;
using Giis.Qacover.Portable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////

namespace Test4giis.Qacover
{
    public class SqlUtil
    {
        public static string ResultSet2csv(ResultSet rs, string separator)
        {
            try
            {
                Jdk14StringBuilder s = new Jdk14StringBuilder();
                int colCount = GetColumnCount(rs);
                while (rs.Next())
                {
                    if (s.Length() != 0)
                        s.Append("\n");
                    for (int i = 0; i < colCount; i++)
                    {
                        string val = rs.GetString(i + 1);
                        if (rs.WasNull())
                            val = "NULL";
                        s.Append((i == 0 ? "" : separator) + val);
                    }
                }

                rs.Close();
                return s.ToString();
            }
            catch (SQLException e)
            {
                throw new QaCoverException(e);
            }
        }

        public static int GetColumnCount(ResultSet rs)
        {
            return rs.GetMetaData().GetColumnCount();
        }
    }
}