/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using Giis.Tdrules.Openapi.Model;


namespace Giis.Qacover.Core.Services
{
	/// <summary>Temporal implementation of the client api, to be moved to tdrules-client</summary>
	public class TdRulesApi : Giis.Tdrules.Client.TdRulesApi
	{
		private bool useCache;

		private string cacheLocation;

		public TdRulesApi(string uri)
			: base(uri)
		{
			//
			this.cacheLocation = Giis.Qacover.Core.Services.Configuration.GetInstance().GetCacheRulesLocation();
			this.useCache = !string.Empty.Equals(Coalesce(cacheLocation));
		}

		/// <summary>Gets the fpc rules for a query executed under the specified schema</summary>
		public SqlRules GetRules(DbSchema schema, string query, string options)
		{
			SqlRulesBody request = new SqlRulesBody();
			request.SetSchema(schema);
			request.SetSql(query);
			request.SetOptions(Coalesce(options));
			TdRulesCache cache = GetCache("rulesPost", request);
			if (UseCache() && cache.Hit())
			{
				return (SqlRules)cache.GetPayload(typeof(SqlRules));
			}
			SqlRules result = base.RulesPost(request);
			if (UseCache())
			{
				cache.PutPayload(result);
			}
			return result;
		}

		public virtual SqlTableListBody GetTables(string sql)
		{
			TdRulesCache cache = GetCache("sqlTablesPost", sql);
			if (UseCache() && cache.Hit())
			{
				return (SqlTableListBody)cache.GetPayload(typeof(SqlTableListBody));
			}
			SqlTableListBody result = base.SqlTablesPost(sql);
			if (UseCache())
			{
				cache.PutPayload(result);
			}
			return result;
		}

		public virtual SqlParametersBody GetParameters(string sql)
		{
			TdRulesCache cache = GetCache("sqlParametersPost", sql);
			if (UseCache() && cache.Hit())
			{
				return (SqlParametersBody)cache.GetPayload(typeof(SqlParametersBody));
			}
			SqlParametersBody result = base.SqlParametersPost(sql);
			if (UseCache())
			{
				cache.PutPayload(result);
			}
			return result;
		}

		// Cache management
		private bool UseCache()
		{
			return this.useCache;
		}

		private TdRulesCache GetCache(string endpoint, object request)
		{
			return UseCache() ? new TdRulesCache(this.cacheLocation, endpoint, request) : null;
		}

		private string Coalesce(string value)
		{
			return value == null ? string.Empty : value;
		}
	}
}
