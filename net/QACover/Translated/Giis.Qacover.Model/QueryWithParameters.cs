/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////


namespace Giis.Qacover.Model
{
	/// <summary>DTO to store both a sql query and its parameters</summary>
	public class QueryWithParameters
	{
		private string sql;

		protected internal QueryParameters parameters = new QueryParameters();

		public virtual QueryParameters GetParams()
		{
			return parameters;
		}

		public virtual void PutParam(string name, string value)
		{
			parameters.Put(name, value);
		}

		public virtual string GetSql()
		{
			return sql;
		}

		public virtual void SetSql(string sql)
		{
			this.sql = sql;
		}
	}
}
