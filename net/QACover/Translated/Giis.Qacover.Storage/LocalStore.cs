using Java.Util;
using NLog;
using Giis.Portable.Util;
using Giis.Qacover.Model;
using Giis.Tdrules.Model.IO;
using Giis.Tdrules.Openapi.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////

namespace Giis.Qacover.Storage
{
    /// <summary>
    /// Manages the persistence of the model objects (queries, rules and its
    /// evaluation). Currently there only exist a storage in local folders.
    /// 
    /// Three kinds of information are stored:
    /// (1) Query models: a file for each query with the rules and evaluation results,
    /// the name of the file is the QueryKey (full qualified method name, line, sql hash)
    /// (2) History: A single file with a line for each query evaluation, contains a csv
    /// of datetime, QueryKey and paramters used in the execution
    /// (3) Additional files for debugging to store the sql, schema, rules and
    /// textual details about the evaluation of each query
    /// </summary>
    public class LocalStore
    {
        private static readonly Logger log = Giis.Portable.Util.NLogUtil.GetLogger(typeof(LocalStore));
        private static readonly string HISTORY_FILE_NAME = "00HISTORY.log";
        // Main folder to store the rules
        protected string storeLocation;
        // Addicional folders for debug info (must have siblings to the storeLocation)
        protected string storeLocationSchema;
        protected string storeLocationSql;
        protected string storeLocationStack;
        protected string storeLocationInRules;
        protected string storeLocationRuns;
        public LocalStore(string location)
        {
            storeLocation = location;
            storeLocationSql = FileUtil.GetPath(storeLocation, "log-sql");
            storeLocationStack = FileUtil.GetPath(storeLocation, "log-stack");
            storeLocationSchema = FileUtil.GetPath(storeLocation, "log-schema");
            storeLocationInRules = FileUtil.GetPath(storeLocation, "log-inrules");
            storeLocationRuns = FileUtil.GetPath(storeLocation, "log-runs");
        }

        public virtual void EnsureStoreFolders()
        {
            FileUtil.CreateDirectory(storeLocation);
            FileUtil.CreateDirectory(storeLocationSchema);
            FileUtil.CreateDirectory(storeLocationSql);
            FileUtil.CreateDirectory(storeLocationStack);
            FileUtil.CreateDirectory(storeLocationInRules);
            FileUtil.CreateDirectory(storeLocationRuns);
        }

        public virtual string GetStoreLocation()
        {
            return storeLocation;
        }

        /// <summary>
        /// Stores the query model, overwrite if already exists
        /// </summary>
        public virtual void PutQueryModel(string queryKey, QueryModel queryModel, QueryParameters @params, SchemaModel schemaModel, DateTime timestamp, ResultVector resultVector)
        {
            FileUtil.FileWrite(storeLocation, queryKey + ".xml", new TdRulesXmlSerializer().Serialize(queryModel.GetModel()));
            AddHistoryItem(timestamp, queryKey, @params, resultVector);

            // The schema model is only available for new generated rules, second time that a rule is evaluated
            // it is read from the store and does not need the schema
            if (schemaModel != null)
                PutSchema(queryKey, schemaModel);
        }

        private void AddHistoryItem(DateTime timestamp, string queryKey, QueryParameters @params, ResultVector resultVector)
        {
            HistoryModel historyLog = new HistoryModel(timestamp, queryKey, @params, resultVector);
            FileUtil.FileAppend(storeLocation, HISTORY_FILE_NAME, historyLog.ToStringV2() + "\n");
        }

        private void PutSchema(string queryKey, SchemaModel schema)
        {
            FileUtil.FileWrite(storeLocationSchema, queryKey + ".xml", new TdSchemaXmlSerializer().Serialize(schema.GetModel()));
        }

        /// <summary>
        /// Retrieves the rule model that must be previously stored,
        /// null if does not exist
        /// </summary>
        public virtual QueryModel GetQueryModel(string queryKey)
        {
            string frules = FileUtil.FileRead(storeLocation, queryKey + ".xml", false);
            log.Trace("Try get QueryModel file from store: " + queryKey);
            if (frules == null)
            {
                log.Trace("QueryModel file has not been generated");
                return null;
            }

            log.Trace("QueryModel file found");
            TdRules srules = new TdRulesXmlSerializer().Deserialize(frules);
            return new QueryModel(srules);
        }

        /// <summary>
        /// Gests all records in the store history
        /// </summary>
        public virtual IList<HistoryModel> GetHistoryItems()
        {
            string logFile = FileUtil.GetPath(storeLocation, HISTORY_FILE_NAME);
            IList<string> lines = FileUtil.FileReadLines(logFile, false);
            IList<HistoryModel> items = new List<HistoryModel>();
            if (JavaCs.IsEmpty(lines))
                log.Warn("Storage of eval status and params can not be found or is empty: " + logFile);
            foreach (string line in lines)
                items.Add(new HistoryModel(line));
            return items;
        }

        /// <summary>
        /// Gets the file names of all stored query models
        /// </summary>
        public virtual IList<string> GetRuleItems()
        {
            IList<string> files = FileUtil.GetFileListInDirectory(storeLocation);
            for (int i = files.Count - 1; i >= 0; i--)
                if (!files[i].EndsWith(".xml"))
                    files.RemoveAt(i);
            ((List<string>)files).Sort(); // To make repetible
            return files;
        }

        public virtual SchemaModel GetSchema(string ruleKey)
        {
            string fschema = FileUtil.FileRead(storeLocationSchema, ruleKey + ".xml");
            TdSchema schema = new TdSchemaXmlSerializer().Deserialize(fschema);
            return new SchemaModel(schema);
        }

        /// <summary>
        /// Removes all stored query models, used to reset the execution
        /// </summary>
        public virtual void DeleteRules()
        {
            FileUtil.DeleteFilesInDirectory(storeLocation);
            FileUtil.DeleteFilesInDirectory(storeLocationSchema);
            FileUtil.DeleteFilesInDirectory(storeLocationSql);
            FileUtil.DeleteFilesInDirectory(storeLocationStack);
            FileUtil.DeleteFilesInDirectory(storeLocationInRules);
            FileUtil.DeleteFilesInDirectory(storeLocationRuns);
        }

        // Storing of additional information for debug
        public virtual void PutSql(string queryKey, string sql)
        {
            FileUtil.FileWrite(storeLocationSql, queryKey + ".sql", sql);
        }

        public virtual void PutStack(string queryKey, string stack)
        {
            FileUtil.FileWrite(storeLocationStack, queryKey + ".txt", queryKey + stack);
        }

        public virtual void PutGeneratedInRules(string queryKey, string lastGeneratedInRules)
        {
            FileUtil.FileWrite(storeLocationInRules, queryKey + ".xml", lastGeneratedInRules);
        }

        public virtual void AppendLogRun(string queryKey, string logRun)
        {
            FileUtil.FileAppend(storeLocationRuns, queryKey + ".log", logRun);
        }
    }
}