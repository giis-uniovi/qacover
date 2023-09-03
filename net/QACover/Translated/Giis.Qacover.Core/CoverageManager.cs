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

		public CoverageManager()
			: base()
		{
		}

		public CoverageManager(QueryModel model)
		{
			// generation process interrupted for any reason
			// Different constructors for new rules, existing and errors
			this.model = model;
		}

		public CoverageManager(string sql, string error)
		{
			this.model = new QueryModel(sql, error);
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
			string fpcOptions = "clientname=" + config.GetName() + new Variability().GetVariantId() + " clientversion=" + clientVersion + " numberjdbcparam" + " " + config.GetFpcServiceOptions();
			store.SetLastGeneratedInRules(svc.GetRulesInput(sql, this.schema, fpcOptions));
			model = svc.GetRulesModel(sql, this.schema, fpcOptions);
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
			stmt.SetVariant(new Variability(model.GetDbms()));
			// Executes and annotates the coverage/status of each rule
			for (int i = 0; i < rules.Count; i++)
			{
				ManagedRule rule = new ManagedRule(rules[i]);
				string res;
				if (options.GetOptimizeRuleEvaluation() && rule.GetModel().GetDead() > 0)
				{
					res = ManagedRule.AlreadyCovered;
				}
				else
				{
					res = rule.Run(stmt);
				}
				// Restults for logging
				string logString = GetLogString(rule, stmt, res);
				logsb.Append((i == 0 ? string.Empty : "\n") + logString);
				log.Debug(logString);
				// store results
				if (rule.GetModel().GetDead() > 0)
				{
					model.AddDead(1);
				}
				if (rule.GetModel().GetError() > 0)
				{
					model.AddError(1);
				}
			}
			model.SetCount(rules.Count);
			model.AddQrun(1);
			log.Info(" SUMMARY: Covered " + model.GetDead() + " out of " + model.GetCount());
			store.SetLastRulesLog(logsb.ToString());
		}

		private string GetLogString(ManagedRule rule, QueryStatement stmt, string res)
		{
			string ruleWithSql = rule.GetSqlWithValues(stmt).Replace("\r", string.Empty).Replace("\n", " ").Trim();
			string logString = res + " " + (ManagedRule.Covered.Equals(res) ? "  " : string.Empty) + ruleWithSql;
			logString += ManagedRule.RuntimeError.Equals(res) ? "\n" + rule.GetModel().GetRuntimeError() : string.Empty;
			return logString;
		}
	}
}
