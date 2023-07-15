/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using Giis.Qacover.Model;
using Java.Sql;


namespace Test4giis.Qacoverapp
{
	/// <summary>Simple mock application to test errors</summary>
	public class AppSimpleJdbc3Errors : AppBase
	{
		/// <exception cref="Java.Sql.SQLException"/>
		public AppSimpleJdbc3Errors(Variability targetVariant)
			: base(targetVariant)
		{
		}

		/// <exception cref="Java.Sql.SQLException"/>
		public virtual ResultSet Query0Errors()
		{
			return ExecuteQuery("select id,num,text from test where num<9");
		}

		/// <exception cref="Java.Sql.SQLException"/>
		public virtual ResultSet Query1ErrorAtQuery()
		{
			return ExecuteQuery("select id,num,text from test where num<10");
		}

		/// <exception cref="Java.Sql.SQLException"/>
		public virtual ResultSet Query1ErrorAtRule()
		{
			return ExecuteQuery("select id,num,text from test where num < 11");
		}

		/// <exception cref="Java.Sql.SQLException"/>
		public virtual ResultSet QueryMultipleErrors()
		{
			return ExecuteQuery("select id,num,text from test where num<9");
		}
	}
}
