/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using Giis.Qacover.Core.Services;
using Giis.Qacover.Model;
using Java.Sql;


namespace Test4giis.Qacoverapp
{
	/// <summary>Simple mock application</summary>
	public class AppSimpleJdbc : AppBase
	{
		/// <exception cref="Java.Sql.SQLException"/>
		public AppSimpleJdbc(Variability targetVariant)
			: base(targetVariant)
		{
		}

		//para pruebas de stacktrace
		public virtual StackLocator MyGetStackTraceTargetMethod()
		{
			return MyGetStackTraceIgnoredMethod();
		}

		/// <exception cref="Java.Sql.SQLException"/>
		public virtual ResultSet QueryNoParameters1Condition(int param1)
		{
			return ExecuteQuery("select id,num,text from test where num>=" + param1);
		}

		/// <exception cref="Java.Sql.SQLException"/>
		public virtual ResultSet QueryNoParameters2Condition(int param1, string param2)
		{
			return ExecuteQuery("select id,num,text from test where num>" + param1 + " and text='" + param2 + "'");
		}

		/// <exception cref="Java.Sql.SQLException"/>
		public override ResultSet QueryParameters(int param1, string param2)
		{
			return ExecuteQuery("select id,num,text from test where num>? and text=?", param1, param2);
		}

		//multiples queries en un metodo (misma y distinta linea)
		/// <exception cref="Java.Sql.SQLException"/>
		public virtual void QueryDifferentSingleLine(bool run1, int param1, bool run2, string param2)
		{
			//dos queries en misma linea con distintas reglas
			if (run1)
			{
				rs = ExecuteQuery("select * from test where num=" + param1);
				rs.Close();
			}
			if (run2)
			{
				rs = ExecuteQuery("select * from test where text=" + param2);
				rs.Close();
			}
		}

		/// <exception cref="Java.Sql.SQLException"/>
		public virtual void QueryEqualSingleLine(string param1, string param2)
		{
			rs = ExecuteQuery("select * from test where text=" + param1);
			rs.Close();
			ExecuteQuery("select * from test where text=" + param2);
		}

		/// <exception cref="Java.Sql.SQLException"/>
		public virtual void QueryEqualDifferentLine(string param1, string param2)
		{
			rs = ExecuteQuery("select * from test where text=" + param1);
			rs.Close();
			//la misma en otra linea
			rs = ExecuteQuery("select * from test where text=" + param2);
		}

		/// <exception cref="Java.Sql.SQLException"/>
		public virtual ResultSet QueryNoConditions()
		{
			return ExecuteQuery("select id,num,text from test");
		}

		/// <exception cref="Java.Sql.SQLException"/>
		public virtual ResultSet QueryNoParametersQuotes(string param1, bool bracketAtTable, bool bracketAtColumn)
		{
			return ExecuteQuery("select id,num,text from " + QuoteIdentifier("test", bracketAtTable) + " where " + QuoteIdentifier("text", bracketAtColumn) + "='" + param1 + "'");
		}

		/// <exception cref="Java.Sql.SQLException"/>
		public virtual ResultSet QueryNoParametersBrackets(string param1, bool bracketAtTable, bool bracketAtColumn)
		{
			return ExecuteQuery("select id,num,text from " + (bracketAtTable ? "[test]" : "test") + " where " + (bracketAtColumn ? "[text]" : "text") + "='" + param1 + "'");
		}

		private string QuoteIdentifier(string name, bool doQuote)
		{
			if (doQuote)
			{
				//oracle ademas siempre pasa todo a mayusculas, luego debe estar mayusculas al poner quotes
				return "\"" + (variant.IsOracle() || variant.IsH2() ? name.ToUpper() : name) + "\"";
			}
			else
			{
				return name;
			}
		}

		/// <exception cref="Java.Sql.SQLException"/>
		public virtual ResultSet QueryParametersNamed(int param1, int param2, string param3)
		{
			return ExecuteQuery("/* params=?1?,?1?,?2? */ select id,num,text from test where id=? or num=? or text=?", param1, param2, param3);
		}

		// This is for testing mutants
		/// <exception cref="Java.Sql.SQLException"/>
		public virtual ResultSet QueryMutParameters(string param1)
		{
			return ExecuteQueryMut("select id,txt from test where txt=?", param1);
		}
	}
}
