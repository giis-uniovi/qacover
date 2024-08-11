/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using System.Text;
using Giis.Portable.Util;
using Giis.Qacover.Core.Services;
using Giis.Qacover.Model;
using NLog;


namespace Giis.Qacover.Core
{
	/// <summary>
	/// Manages the generation and execution of the rules for a query
	/// and keeps track of the coverage results
	/// </summary>
	public class CoverageManager
	{
		private bool abortedStatus = false;

		private static readonly Logger log = NLogUtil.GetLogger(typeof(Giis.Qacover.Core.CoverageManager));

		private QueryModel model;

		private SchemaModel schema;

		private RuleDriver ruleDriver;

		private ResultVector result = new ResultVector(0);

		public CoverageManager(RuleDriver ruleDriver)
		{
			// generation process interrupted for any reason
			// a delegate to get and evaluate the rules
			// to avoid null if fails before generating rules
			// Different constructors for new rules, existing and errors
			this.ruleDriver = ruleDriver;
		}

		public CoverageManager(RuleDriver ruleDriver, QueryModel model)
		{
			this.model = model;
			this.ruleDriver = ruleDriver;
		}

		public CoverageManager(RuleDriver ruleDriver, string sql, string error)
		{
			this.model = new QueryModel(sql, error);
			this.ruleDriver = ruleDriver;
		}

		public virtual QueryModel GetModel()
		{
			return this.model;
		}

		/// <summary>
		/// Gets the model of the schema used to generate the rules, note that this is only present
		/// the first execution (when the rules are generated), the rest of executions
		/// retrieve the rules from the store and set the schema as null
		/// </summary>
		public virtual SchemaModel GetSchemaAtRuleGeneration()
		{
			return this.schema;
		}

		public virtual ResultVector GetResult()
		{
			return result;
		}

		public virtual bool IsAborted()
		{
			return abortedStatus;
		}

		/// <summary>Generates a new set of coverage rules for a given query, that are kept in a QueryModel</summary>
		public virtual void Generate(RuleServices svc, StoreService store, QueryStatement stmt, string sql, Configuration config)
		{
			FaultInjector faultInjector = stmt.GetFaultInjector();
			// To better handling large schemas first gets the tables involved in the query
			log.Debug("Getting table names");
			store.SetLastGeneratedSql(sql);
			if (faultInjector != null && faultInjector.IsSchemaFaulty())
			{
				sql = stmt.GetFaultInjector().GetSchemaFault();
			}
			string[] tables = svc.GetAllTableNames(sql, config.GetDbStoretype());
			log.Debug("Table names: " + JavaCs.DeepToString(tables));
			// Check table name exclusions
			foreach (string table in tables)
			{
				if (Configuration.ValueInProperty(table, config.GetTableExclusionsExact(), false))
				{
					log.Warn("Table " + table + " found in table exclusions property, skip generation");
					abortedStatus = true;
					return;
				}
			}
			// Now, gets the schema with only the tables involved in teh query
			log.Debug("Getting database schema");
			this.schema = svc.GetSchemaModel(stmt.GetConnection(), string.Empty, string.Empty, tables);
			// Gets the rules, always numbering jdbc parameters for further substitution
			log.Debug("Getting sql coverage rules");
			string clientVersion = new Variability().GetVersion();
			string fpcOptions = "clientname=" + config.GetName() + new Variability().GetVariantId() + " clientversion=" + clientVersion + " numberjdbcparam" + ("mutation".Equals(config.GetRuleServiceType()) ? " getparsedquery" : string.Empty) + " " + config.GetFpcServiceOptions();
			// required to evaluate query
			store.SetLastGeneratedInRules(svc.GetRulesInput(sql, this.schema, fpcOptions));
			model = ruleDriver.GetRules(svc, sql, this.schema, fpcOptions);
			// The DBMS is stored too to manage its variability in further actions
			model.SetDbms(this.schema.GetDbms());
		}

		/// <summary>Executes the rules that must be previously generated</summary>
		public virtual void Run(RuleServices svc, StoreService store, QueryStatement stmt, Configuration options)
		{
			svc.SetErrorContext("Run SQLFpc coverage rules");
			store.SetLastSqlRun(stmt.GetSql());
			StringBuilder logsb = new StringBuilder();
			IList<RuleModel> rules = model.GetRules();
			FaultInjector injector = stmt.GetFaultInjector();
			if (injector != null && injector.IsSingleRuleFaulty())
			{
				rules[0].SetSql(injector.GetSingleRuleFault());
			}
			model.Reset();
			result = new ResultVector(rules.Count);
			stmt.SetVariant(new Variability(model.GetDbms()));
			// Mutation requires a previous step to read query results
			// note that it requires generate the rules to get the parsed query
			// that includes numbers in the parameters
			ruleDriver.PrepareEvaluation(stmt, model.GetModel().GetParsedquery());
			// Executes and annotates the coverage/status of each rule
			for (int i = 0; i < rules.Count; i++)
			{
				RuleModel ruleModel = rules[i];
				string res;
				if (options.GetOptimizeRuleEvaluation() && ruleModel.GetDead() > 0)
				{
					res = ResultVector.AlreadyCovered;
				}
				else
				{
					res = ruleDriver.EvaluateRule(ruleModel, stmt);
				}
				// Results for logging
				string logString = ruleDriver.GetLogString(ruleModel, stmt, res);
				logsb.Append((i == 0 ? string.Empty : "\n") + logString);
				log.Debug(logString);
				// store results
				result.SetResult(i, res);
				if (ruleModel.GetDead() > 0)
				{
					model.AddDead(1);
				}
				if (ruleModel.GetError() > 0)
				{
					model.AddError(1);
				}
			}
			model.SetCount(rules.Count);
			model.AddQrun(1);
			log.Info(" SUMMARY: Covered " + model.GetDead() + " out of " + model.GetCount());
			store.SetLastRulesLog(logsb.ToString());
		}
	}
}
