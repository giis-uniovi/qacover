using NLog;
using Giis.Qacover.Core.Services;
using Giis.Qacover.Model;
using Giis.Qacover.Portable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////

namespace Giis.Qacover.Core
{
    /// <summary>
    /// When p6spy finds an elegible statement to evaluate the coverage,
    /// creates a new controller instancia and calls it to manage the process.
    /// 
    /// This class manages the storage of the rules, generation of new or use of
    /// existing rules, calling the CoverageManager to get the coverage and top level
    /// errors.
    /// </summary>
    public class Controller
    {
        private static readonly Logger log = Giis.Portable.Util.NLogUtil.GetLogger(typeof(Controller));
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
                    throw stmt.GetException();
                rm = MainProcessSql(svc, store, stack, stmt, options); // ejecuta reglas de la query
                if (rm.IsAborted())
                    return;
            }
            catch (Exception e)
            {
                rm = MainProcessError(svc, store, stmt, e);
            }

            log.Debug("Saving evaluation results");
            rm.GetModel().SetLocation(stack.GetClassName(), stack.GetMethodName(), stack.GetLineNumber(), stack.GetFileName(), stack.GetSourceFileName());
            store.Put(stack, stmt.GetSql(), stmt.GetParameters(), rm.GetModel(), rm.GetSchemaAtRuleGeneration(), rm.GetResult());
        }

        private CoverageManager MainProcessError(RuleServices svc, StoreService store, QueryStatement stmt, Exception e)
        {
            string errorMsg = "Error at " + svc.GetErrorContext() + ": " + QaCoverException.GetString(e);
            string sql = stmt != null ? stmt.GetSql() : "";
            RuleDriver rd = new RuleDriverFactory().GetDriver();
            CoverageManager rm = new CoverageManager(rd, sql, errorMsg); // construye error en el formato xml de las reglas
            store.SetLastGenStatus(errorMsg);
            log.Error("  ERROR: " + errorMsg, e);
            return rm;
        }

        private CoverageManager MainProcessSql(RuleServices svc, StoreService store, StackLocator stack, QueryStatement stmt, Configuration options)
        {

            // Set the configuration of data store type if not already set
            if (options.GetDbStoretype() == null)
            {
                SchemaModel schema = svc.GetSchemaModel(stmt.GetConnection(), "", "", new string[] { });
                options.SetDbStoretype(schema.GetDbms());
            }


            // Optional parameter inference may lead to a change in the sql and parameters
            if (options.GetInferQueryParameters() && stmt.GetParameters().GetSize() == 0)
                stmt.InferParameters(svc, options.GetDbStoretype());
            log.Info("  SQL: " + stmt.GetSql());
            log.Info("  PARAMS: " + stmt.GetParameters().ToString());
            store.SetLastParametersRun(stmt.GetParameters().ToString());

            // CoverageManager is constructed from rules generated in a previous query
            // or by generating a fresh set of rules
            RuleDriver rd = new RuleDriverFactory().GetDriver(); // delegate to get and evaluate the rules
            CoverageManager rm = GetCoverageManager(rd, store, stack, stmt, options.GetRuleCriterion());
            if (rm == null)
            {
                log.Debug("Generating new coverage rules for this query");
                rm = new CoverageManager(rd);
                rm.Generate(svc, store, stmt, stmt.GetSql(), options);
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

        private CoverageManager GetCoverageManager(RuleDriver ruleDriver, StoreService store, StackLocator stack, QueryStatement stmt, string currentCriterion)
        {
            QueryModel model = store.Get(stack.GetClassName(), stack.GetMethodName(), stack.GetLineNumber(), stmt.GetSql());
            if (model == null)
                return null; // to signal the need to create a new model

            // If a previous run generated a model with a different criterion, 
            // a new coverage manager must be created to overwrite the existing model
            string ruleCriterion = model.GetModel().GetRulesClass();
            if (!ruleCriterion.Equals(currentCriterion))
            {
                log.Warn("Current {} coverage criterion does not match with the stored rule {} criterion." + " Existing rule will be overwritten", currentCriterion, ruleCriterion);
                return null; // to signal the need to create a new model that will overwrite the stored rule
            }


            // new empty rule
            return new CoverageManager(ruleDriver, model);
        }
    }
}