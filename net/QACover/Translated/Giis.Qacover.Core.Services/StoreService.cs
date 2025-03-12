using Java.Util;
using NLog;
using Giis.Portable.Util;
using Giis.Qacover.Model;
using Giis.Qacover.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////

namespace Giis.Qacover.Core.Services
{
    /// <summary>
    /// Manages the persistence of queries an rules that are evaluated
    /// </summary>
    public class StoreService
    {
        private static readonly Logger log = Giis.Portable.Util.NLogUtil.GetLogger(typeof(StoreService));
        // To allow tests inspect the status of rules
        private static StoreService lastInstance;
        // To facilitate debugging other intermediate values are stored too
        private string lastGeneratedSql = null;
        private string lastGeneratedInRules = null;
        // Also for testing, some values that will be used by the assertions
        private string lastSavedQueryKey = ""; // identificacion del ultimo fichero guardado
        private string lastSqlRun = ""; // ultima query de la que se evaluo la cobertura
        private string lastParametersRun = ""; // ultimo conjunto de parametros con los que se evaluo la cobertura
        private string lastRulesLog = ""; // ultimos resultados de la evaluacion en formato string
        private string lastGenStatus = ""; // estado de la ultima generacion de reglas (success o mensaje de error)
        private LocalStore storage;
        public StoreService(Configuration options)
        {
            storage = new LocalStore(options.GetStoreRulesLocation());
            storage.EnsureStoreFolders();
            lastInstance = this; // NOSONAR needed to allow inspecting from test
        }

        public virtual string GetStoreLocation()
        {
            return storage.GetStoreLocation();
        }

        public static StoreService GetLast()
        {
            return lastInstance;
        }

        public virtual void DropLast()
        {
            lastInstance = null; // NOSONAR non static because is used in fluent calls
        }

        /// <summary>
        /// Remove all data related to execution of rules
        /// </summary>
        public virtual StoreService DropRules()
        {
            storage.DeleteRules();
            return this;
        }

        /// <summary>
        /// Saves the set of rules for a query in the persistent store
        /// </summary>
        public virtual void Put(StackLocator stack, string sql, QueryParameters @params, QueryModel queryModel, SchemaModel schemaModel, ResultVector resultVector)
        {
            string className = stack.GetClassName();
            string methodName = stack.GetMethodName();
            int lineNumber = stack.GetLineNumber();

            // timestamp indicates the time of rule saving (a little bit more than its execution begin
            DateTime timestampLast = JavaCs.GetCurrentDate();
            string queryKey = new QueryKey(className, methodName, lineNumber, sql).ToString();
            log.Debug("Save file to store: " + queryKey);
            storage.PutQueryModel(queryKey, queryModel, @params, schemaModel, timestampLast, resultVector);

            // Additional info for testing and debugging purposes
            SetLastSavedQueryKey(queryKey);
            if (this.lastGeneratedSql != null)
                storage.PutSql(queryKey, lastGeneratedSql);
            if (this.lastGeneratedSql != null && stack != null)
                storage.PutStack(queryKey, stack.ToString());
            if (this.lastGeneratedInRules != null)
                storage.PutGeneratedInRules(queryKey, lastGeneratedInRules);

            // To decide if add some parameter to exclude this log (it includes parameters, maybe
            // some confidential data could be stored, e.g. passwords)
            string logRuns = "---- " + timestampLast.ToString() + "\nGENERATION: " + this.lastGenStatus + "\nSQL: " + this.lastSqlRun + "\nPARAMS: " + @params.ToString() + "\n" + this.lastRulesLog + "\n";
            storage.AppendLogRun(queryKey, logRuns);
        }

        /// <summary>
        /// Retrieves a CoverageManager from the local store given the query coordinates,
        /// null if does not exists
        /// </summary>
        public virtual QueryModel Get(string className, string methodName, int lineNumber, string sql)
        {
            string queryKey = new QueryKey(className, methodName, lineNumber, sql).ToString();
            return this.GetQueryModel(queryKey);
        }

        /// <summary>
        /// Retrieves a QueryModel from the local store given the query key,
        /// null if it does not exist
        /// </summary>
        public virtual QueryModel GetQueryModel(string queryKey)
        {
            return storage.GetQueryModel(queryKey);
        }

        public virtual void SetLastGeneratedSql(string sql)
        {
            this.lastGeneratedSql = sql;
        }

        public virtual void SetLastGeneratedInRules(string xml)
        {
            this.lastGeneratedInRules = xml;
        }

        public virtual string GetLastSqlRun()
        {
            return lastSqlRun;
        }

        public virtual void SetLastSqlRun(string lastSqlRun)
        {
            this.lastSqlRun = lastSqlRun;
        }

        public virtual void SetLastParametersRun(string @params)
        {
            this.lastParametersRun = @params;
        }

        public virtual string GetLastParametersRun()
        {
            return lastParametersRun;
        }

        public virtual string GetLastRulesLog()
        {
            return lastRulesLog;
        }

        public virtual void SetLastRulesLog(string lastRulesLog)
        {
            this.lastRulesLog = lastRulesLog;
        }

        public virtual string GetLastSavedQueryKey()
        {
            return lastSavedQueryKey;
        }

        private void SetLastSavedQueryKey(string lastSavedQueryKey)
        {
            this.lastSavedQueryKey = lastSavedQueryKey;
        }

        public virtual string GetLastGenStatus()
        {
            return lastGenStatus;
        }

        public virtual void SetLastGenStatus(string lastGenStatus)
        {
            this.lastGenStatus = lastGenStatus;
        }
    }
}