/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Giis.Portable.Util;
using Giis.Qacover.Model;
using Giis.Qacover.Storage;
using NLog;


namespace Giis.Qacover.Core.Services
{
	/// <summary>Manages the persistence of queries an rules that are evaluated</summary>
	public class StoreService
	{
		private static readonly Logger log = NLogUtil.GetLogger(typeof(Giis.Qacover.Core.Services.StoreService));

		private static Giis.Qacover.Core.Services.StoreService lastInstance;

		private string lastGeneratedSql = null;

		private string lastGeneratedInRules = null;

		private string lastSavedQueryKey = string.Empty;

		private string lastSqlRun = string.Empty;

		private string lastParametersRun = string.Empty;

		private string lastRulesLog = string.Empty;

		private string lastGenStatus = string.Empty;

		private LocalStore storage;

		public StoreService(Configuration options)
		{
			// To allow tests inspect the status of rules
			// To facilitate debugging other intermediate values are stored too
			// Also for testing, some values that will be used by the assertions
			// identificacion del ultimo fichero guardado
			// ultima query de la que se evaluo la cobertura
			// ultimo conjunto de parametros con los que se evaluo la cobertura
			// ultimos resultados de la evaluacion en formato string
			// estado de la ultima generacion de reglas (success o mensaje de error)
			storage = new LocalStore(options.GetStoreRulesLocation());
			storage.EnsureStoreFolders();
			lastInstance = this;
		}

		// NOSONAR needed to allow inspecting from test
		public virtual string GetStoreLocation()
		{
			return storage.GetStoreLocation();
		}

		public static Giis.Qacover.Core.Services.StoreService GetLast()
		{
			return lastInstance;
		}

		public virtual void DropLast()
		{
			lastInstance = null;
		}

		// NOSONAR non static because is used in fluent calls
		/// <summary>Remove all data related to execution of rules</summary>
		public virtual Giis.Qacover.Core.Services.StoreService DropRules()
		{
			storage.DeleteRules();
			return this;
		}

		/// <summary>Saves the set of rules for a query in the persistent store</summary>
		public virtual void Put(StackLocator stack, string sql, QueryParameters @params, QueryModel queryModel, SchemaModel schemaModel)
		{
			string className = stack.GetClassName();
			string methodName = stack.GetMethodName();
			int lineNumber = stack.GetLineNumber();
			// timestamp indicates the time of rule saving (a little bit more than its execution begin
			DateTime lastTimestamp = JavaCs.GetCurrentDate();
			string queryKey = new QueryKey(className, methodName, lineNumber, sql).ToString();
			log.Debug("Save file to store: " + queryKey);
			storage.PutQueryModel(queryKey, queryModel, @params, schemaModel, lastTimestamp);
			// Additional info for testing and debugging purposes
			SetLastSavedQueryKey(queryKey);
			if (this.lastGeneratedSql != null)
			{
				storage.PutSql(queryKey, lastGeneratedSql);
			}
			if (this.lastGeneratedSql != null && stack != null)
			{
				storage.PutStack(queryKey, stack.ToString());
			}
			if (this.lastGeneratedInRules != null)
			{
				storage.PutGeneratedInRules(queryKey, lastGeneratedInRules);
			}
			// To decide if add some parameter to exclude this log (it includes parameters, maybe
			// some confidential data could be stored, e.g. passwords)
			string logRuns = "---- " + lastTimestamp.ToString() + "\nGENERATION: " + this.lastGenStatus + "\nSQL: " + this.lastSqlRun + "\nPARAMS: " + @params.ToString() + "\n" + this.lastRulesLog + "\n";
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
