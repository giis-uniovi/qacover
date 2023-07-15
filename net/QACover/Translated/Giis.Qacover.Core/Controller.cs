/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Giis.Portable.Util;
using Giis.Qacover.Core.Services;
using Giis.Qacover.Model;
using Giis.Qacover.Portable;
using NLog;


namespace Giis.Qacover.Core
{
	/// <summary>
	/// When p6spy finds an elegible statement to evaluate the coverage,
	/// creates a new controller instancia and calls it to manage the process.
	/// </summary>
	/// <remarks>
	/// When p6spy finds an elegible statement to evaluate the coverage,
	/// creates a new controller instancia and calls it to manage the process.
	/// This class manages the storage of the rules, generation of new or use of
	/// existing rules, calling the CoverageManager to get the coverage and top level
	/// errors.
	/// </remarks>
	public class Controller
	{
		private static readonly Logger log = NLogUtil.GetLogger(typeof(Controller));

		public virtual void ProcessSql(QueryStatement stmt)
		{
			log.Debug("**** processSql");
			Configuration options = Configuration.GetInstance();
			StoreService store = new StoreService(options);
			StackLocator stack = new StackLocator();
			RuleServices svc = new RuleServices(stmt.GetFaultInjector());
			CoverageManager rm;
			log.Info("LOCATION: " + stack.GetLocationAsString());
			// Checking exclusion list
			if (Configuration.ValueInProperty(stack.GetClassName(), options.GetClassExclusions(), true))
			{
				log.Warn("Class " + stack.GetClassName() + " found in class exclusions property, skip");
				return;
			}
			// Processing and storing, controls successful/unsuccessful.
			try
			{
				if (stmt.GetException() != null)
				{
					throw stmt.GetException();
				}
				rm = MainProcessSql(svc, store, stack, stmt, options);
				// ejecuta reglas de la query
				if (rm.IsAborted())
				{
					// aborted, maybe due to of exclusion of queries/tables
					return;
				}
			}
			catch (Exception e)
			{
				rm = MainProcessError(svc, store, stmt, e);
			}
			log.Debug("Saving evaluation results");
			rm.GetModel().SetLocation(stack.GetClassName(), stack.GetMethodName(), stack.GetLineNumber(), stack.GetFileName());
			store.Put(stack, stmt.GetSql(), stmt.GetParameters(), rm.GetModel(), rm.GetSchemaAtRuleGeneration());
		}

		private CoverageManager MainProcessError(RuleServices svc, StoreService store, QueryStatement stmt, Exception e)
		{
			string errorMsg = "Error at " + svc.GetErrorContext() + ": " + QaCoverException.GetString(e);
			string sql = stmt != null ? stmt.GetSql() : string.Empty;
			CoverageManager rm = new CoverageManager(sql, errorMsg);
			// construye error en el formato xml de las reglas
			store.SetLastGenStatus(errorMsg);
			log.Error("  ERROR: " + errorMsg, e);
			return rm;
		}

		private CoverageManager MainProcessSql(RuleServices svc, StoreService store, StackLocator stack, QueryStatement stmt, Configuration options)
		{
			// Optional parameter inference may lead to a change in the sql and parameters
			if (options.GetInferQueryParameters() && stmt.GetParameters().Size() == 0)
			{
				stmt.InferParameters(svc);
			}
			log.Info("  SQL: " + stmt.GetSql());
			log.Info("  PARAMS: " + stmt.GetParameters().ToString());
			store.SetLastParametersRun(stmt.GetParameters().ToString());
			// CoverageManager is constructed from rules generated in a previous query
			// or by generating a fresh set of rules
			CoverageManager rm = GetCoverageManager(store, stack, stmt);
			if (rm == null)
			{
				log.Debug("Generating new coverage rules for this query");
				rm = new CoverageManager();
				rm.Generate(svc, store, stmt, stmt.GetSql());
			}
			else
			{
				log.Debug("Using existing coverage rules for this query");
			}
			if (rm.IsAborted())
			{
				log.Warn("SKIP Coverage rules evaluation");
				return rm;
			}
			if (rm.GetModel().GetQerror() > 0)
			{
				log.Warn("The set of coverage rules had previous errors, skip evaluation");
				return rm;
			}
			log.Debug("BEGIN Coverage rules evaluation");
			rm.Run(svc, store, stmt, options);
			store.SetLastGenStatus("success");
			log.Debug("END Coverage rules evaluation");
			return rm;
		}

		private CoverageManager GetCoverageManager(StoreService store, StackLocator stack, QueryStatement stmt)
		{
			QueryModel model = store.Get(stack.GetClassName(), stack.GetMethodName(), stack.GetLineNumber(), stmt.GetSql());
			return model == null ? null : new CoverageManager(model);
		}
	}
}
