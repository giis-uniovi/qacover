/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using Giis.Portable.Util;
using Giis.Qacover.Model;
using Giis.Qacover.Portable;
using Giis.Tdrules.Client;
using Giis.Tdrules.Client.Rdb;
using Giis.Tdrules.Model;
using Giis.Tdrules.Model.IO;
using Giis.Tdrules.Openapi.Model;
using Java.Sql;
using NLog;


namespace Giis.Qacover.Core.Services
{
	/// <summary>Operations to get coverage rules and schemas</summary>
	public class RuleServices
	{
		private static readonly Logger log = NLogUtil.GetLogger(typeof(Giis.Qacover.Core.Services.RuleServices));

		private string errorContext = string.Empty;

		private FaultInjector faultInjector;

		public RuleServices(FaultInjector injector)
		{
			// to give clues on where an exception is raised
			faultInjector = injector;
		}

		public virtual string GetErrorContext()
		{
			return errorContext;
		}

		public virtual void SetErrorContext(string message)
		{
			errorContext = message;
		}

		private TdRulesApi GetApi()
		{
			string uri = Configuration.GetInstance().GetFpcServiceUrl();
			log.Debug("Call service: " + uri);
			return new TdRulesApi(uri);
		}

		public virtual QueryModel GetRulesModel(string sql, SchemaModel schema, string fpcOptions)
		{
			this.SetErrorContext("Generate SQLFpc coverage rules");
			SqlRules model = GetApi().GetRules(schema.GetModel(), sql, fpcOptions);
			InjectFaultIfNeeded(model);
			if (!string.Empty.Equals(model.GetError()))
			{
				throw new QaCoverException(model.GetError());
			}
			return new QueryModel(model);
		}

		public virtual string GetRulesInput(string sql, SchemaModel schemaModel, string fpcOptions)
		{
			SqlRulesBody body = new SqlRulesBody();
			body.SetSql(sql);
			body.SetSchema(schemaModel.GetModel());
			body.SetOptions(fpcOptions);
			return new SqlRulesXmlSerializer().Serialize(body);
		}

		public virtual string[] GetAllTableNames(string sql)
		{
			this.SetErrorContext("Get query table names");
			SqlTableListBody model = GetApi().SqlTablesPost(sql);
			InjectFaultIfNeeded(model);
			if (!string.Empty.Equals(model.GetError()))
			{
				throw new QaCoverException(model.GetError());
			}
			return JavaCs.ToArray(ModelUtil.Safe(model.GetTables()));
		}

		public virtual QueryWithParameters InferQueryWithParameters(string sql)
		{
			this.SetErrorContext("Infer query parameters");
			SqlParametersBody model = GetApi().SqlParametersPost(sql);
			InjectFaultIfNeeded(model);
			if (!string.Empty.Equals(model.GetError()))
			{
				throw new QaCoverException(model.GetError());
			}
			// determines the QueryStatement parameters
			QueryWithParameters ret = new QueryWithParameters();
			ret.SetSql(model.GetParsedsql());
			foreach (SqlParam param in ModelUtil.Safe(model.GetParameters()))
			{
				ret.PutParam(param.GetName(), param.GetValue());
			}
			return ret;
		}

		public virtual SchemaModel GetSchemaModel(Connection conn, string catalog, string schema, string[] tables)
		{
			this.SetErrorContext("Get database schema");
			DbSchemaApi api = new DbSchemaApi(conn).SetCatalogAndSchema(catalog, schema);
			DbSchema model = api.GetDbSchema(JavaCs.ToList(tables));
			return new SchemaModel(model);
		}

		// Fault injection when needed (only for testing)
		private void InjectFaultIfNeeded(SqlRules model)
		{
			// NOSONAR false positive, method overloaded
			if (faultInjector != null && faultInjector.IsRulesFaulty())
			{
				model.SetError(faultInjector.GetRulesFault());
				model.SetRules(null);
			}
		}

		private void InjectFaultIfNeeded(SqlTableListBody model)
		{
			// NOSONAR false positive, method overloaded
			if (faultInjector != null && faultInjector.IsTablesFaulty())
			{
				model.SetError(faultInjector.GetTablesFault());
				model.SetTables(null);
			}
		}

		private void InjectFaultIfNeeded(SqlParametersBody model)
		{
			// NOSONAR false positive, method overloaded
			if (faultInjector != null && faultInjector.IsInferFaulty())
			{
				model.SetError(faultInjector.GetInferFault());
				model.SetParameters(null);
			}
		}
	}
}
