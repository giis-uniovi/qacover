/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using Giis.Portable.Util;
using Giis.Qacover.Model;
using Giis.Qacover.Portable;
using Giis.Tdrules.Client;
using Giis.Tdrules.Client.Rdb;
using Giis.Tdrules.Model.IO;
using Giis.Tdrules.Model.Shared;
using Giis.Tdrules.Openapi.Model;
using Java.Sql;
using NLog;


#pragma warning disable CS0436 // Type conflicts with imported type
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
			string uri = Configuration.GetInstance().GetRuleServiceUrl();
			string cacheLocation = Configuration.GetInstance().GetRuleCacheFolder();
			log.Debug("Call service: {}, using cache: {}", uri, cacheLocation);
			TdRulesApi api = new TdRulesApi(uri);
			if (!string.Empty.Equals(cacheLocation))
			{
				api.SetCache(cacheLocation);
			}
			return api;
		}

		public virtual QueryModel GetFpcRulesModel(string sql, SchemaModel schema, string fpcOptions)
		{
			this.SetErrorContext("Generate SQLFpc coverage rules");
			TdRules model = GetApi().GetRules(schema.GetModel(), sql, fpcOptions);
			InjectFaultIfNeeded(model);
			if (!string.Empty.Equals(model.GetError()))
			{
				throw new QaCoverException(model.GetError());
			}
			return new QueryModel(model);
		}

		public virtual QueryModel GetMutationRulesModel(string sql, SchemaModel schema, string fpcOptions)
		{
			this.SetErrorContext("Generate SQLMutation coverage rules");
			TdRules model = GetApi().GetMutants(schema.GetModel(), sql, fpcOptions);
			InjectFaultIfNeeded(model);
			if (!string.Empty.Equals(model.GetError()))
			{
				throw new QaCoverException(model.GetError());
			}
			return new QueryModel(model);
		}

		public virtual string GetRulesInput(string sql, SchemaModel schemaModel, string fpcOptions)
		{
			TdRulesBody body = new TdRulesBody();
			body.SetQuery(sql);
			body.SetSchema(schemaModel.GetModel());
			body.SetOptions(fpcOptions);
			return new TdRulesXmlSerializer().Serialize(body);
		}

		public virtual string[] GetAllTableNames(string sql, string storetype)
		{
			this.SetErrorContext("Get query table names");
			QueryEntitiesBody model = GetApi().GetEntities(sql, storetype);
			InjectFaultIfNeeded(model);
			if (!string.Empty.Equals(model.GetError()))
			{
				throw new QaCoverException(model.GetError());
			}
			return JavaCs.ToArray(ModelUtil.Safe(model.GetEntities()));
		}

		public virtual QueryWithParameters InferQueryWithParameters(string sql, string storetype)
		{
			this.SetErrorContext("Infer query parameters");
			QueryParametersBody model = GetApi().GetParameters(sql, storetype);
			InjectFaultIfNeeded(model);
			if (!string.Empty.Equals(model.GetError()))
			{
				throw new QaCoverException(model.GetError());
			}
			// determines the QueryStatement parameters
			QueryWithParameters ret = new QueryWithParameters();
			ret.SetSql(model.GetParsedquery());
			foreach (QueryParam param in ModelUtil.Safe(model.GetParameters()))
			{
				ret.PutParam(param.GetName(), param.GetValue());
			}
			return ret;
		}

		public virtual SchemaModel GetSchemaModel(Connection conn, string catalog, string schema, string[] tables)
		{
			this.SetErrorContext("Get database schema");
			DbSchemaApi api = new DbSchemaApi(conn).SetCatalogAndSchema(catalog, schema);
			TdSchema model = api.GetSchema(JavaCs.ToList(tables));
			return new SchemaModel(model);
		}

		// Fault injection when needed (only for testing)
		private void InjectFaultIfNeeded(TdRules model)
		{
			// NOSONAR false positive, method overloaded
			if (faultInjector != null && faultInjector.IsRulesFaulty())
			{
				model.SetError(faultInjector.GetRulesFault());
				model.SetRules(null);
			}
		}

		private void InjectFaultIfNeeded(QueryEntitiesBody model)
		{
			// NOSONAR false positive, method overloaded
			if (faultInjector != null && faultInjector.IsTablesFaulty())
			{
				model.SetError(faultInjector.GetTablesFault());
				model.SetEntities(null);
			}
		}

		private void InjectFaultIfNeeded(QueryParametersBody model)
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
