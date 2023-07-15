/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using Giis.Qacover.Model;
using Java.Sql;


namespace Test4giis.Qacoverapp
{
	/// <summary>Simple mock application only to test evaluation in different queries</summary>
	public class AppSimpleJdbc2 : AppBase
	{
		/// <exception cref="Java.Sql.SQLException"/>
		public AppSimpleJdbc2(Variability targetVariant)
			: base(targetVariant)
		{
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
	}
}
