/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using System.Text;
using Java.Sql;

using Test4giis.Qacoverapp;

namespace Test4giis.Qacover
{
	/// <summary>
	/// Utility to validate the handling of different datatypes in a SGBD:
	/// Stores a list different types, and values to create
	/// statements that check these data types can be created and viewed
	/// </summary>
	public class DataTypeAssert : Base
	{
		public IList<string[]> columns = new List<string[]>();

		// Ads a column of a given datatype to generate an insertion 
		// of inValue and a select of outValue
		public virtual void Add(string type, string inValue, string outValue)
		{
			columns.Add(new string[] { type, inValue, outValue });
		}

		public virtual string GetSqlCreate()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("create table dbmstypes(");
			for (int i = 0; i < columns.Count; i++)
			{
				sb.Append((i == 0 ? string.Empty : ", ") + "col" + i + " " + columns[i][0]);
			}
			sb.Append(")");
			return sb.ToString();
		}

		public virtual string GetSqlInsert(bool firstSerial)
		{
			int first = firstSerial ? 1 : 0;
			// si el primero es serial se salta el valor a insert
			StringBuilder sb = new StringBuilder();
			sb.Append("insert into dbmstypes(");
			for (int i = first; i < columns.Count; i++)
			{
				sb.Append((i == first ? string.Empty : ",") + "col" + i);
			}
			sb.Append(") values (");
			for (int i_1 = first; i_1 < columns.Count; i_1++)
			{
				sb.Append((i_1 == first ? string.Empty : ",") + columns[i_1][1]);
			}
			sb.Append(")");
			return sb.ToString();
		}

		public virtual string GetSqlColumns()
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < columns.Count; i++)
			{
				sb.Append((i == 0 ? string.Empty : " , ") + "col" + i);
			}
			return sb.ToString();
		}

		public virtual string GetOutputs()
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < columns.Count; i++)
			{
				sb.Append((i == 0 ? string.Empty : " ") + columns[i][2]);
			}
			return sb.ToString();
		}

		// Main assertion: create table, execute insertion and then select with the configured types and values
		/// <exception cref="Java.Sql.SQLException"/>
		public virtual void AssertAll(bool firstSerial, AppSimpleJdbc app, ResultSet rs)
		{
			app.DropTable("dbmstypes");
			app.ExecuteUpdateNative(new string[] { GetSqlCreate(), GetSqlInsert(firstSerial) });
			string sql = "select " + GetSqlColumns() + " from dbmstypes where col0<10";
			rs = app.ExecuteQuery(sql);
			// Another assert to check that datatype are handled when evaluating rules
			AssertEvalResults(sql, GetOutputs(), SqlUtil.ResultSet2csv(rs, " "), "UNCOVERED SELECT " + GetSqlColumns() + " FROM dbmstypes WHERE NOT(col0 < 10)\n" + "COVERED   SELECT " + GetSqlColumns() + " FROM dbmstypes WHERE (col0 < 10)");
		}
	}
}
