package giis.qacover.core.services;

import java.util.Date;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import giis.portable.util.JavaCs;
import giis.qacover.model.QueryKey;
import giis.qacover.model.QueryModel;
import giis.qacover.model.QueryParameters;
import giis.qacover.model.ResultVector;
import giis.qacover.model.SchemaModel;
import giis.qacover.storage.LocalStore;

/**
 * Manages the persistence of queries an rules that are evaluated
 */
public class StoreService {
	private static final Logger log = LoggerFactory.getLogger(StoreService.class);

	// To allow tests inspect the status of rules
	private static StoreService lastInstance;
	// To facilitate debugging other intermediate values are stored too
	private String lastGeneratedSql = null;
	private String lastGeneratedInRules = null;
	// Also for testing, some values that will be used by the assertions
	private String lastSavedQueryKey = ""; // identificacion del ultimo fichero guardado
	private String lastSqlRun = ""; // ultima query de la que se evaluo la cobertura
	private String lastParametersRun = ""; // ultimo conjunto de parametros con los que se evaluo la cobertura
	private String lastRulesLog = ""; // ultimos resultados de la evaluacion en formato string
	private String lastGenStatus = ""; // estado de la ultima generacion de reglas (success o mensaje de error)
	private LocalStore storage;

	public StoreService(Configuration options) {
		storage = new LocalStore(options.getStoreRulesLocation());
		storage.ensureStoreFolders();
		lastInstance = this; // NOSONAR needed to allow inspecting from test
	}

	public String getStoreLocation() {
		return storage.getStoreLocation();
	}

	public static StoreService getLast() {
		return lastInstance;
	}
	public void dropLast() {
		lastInstance = null; // NOSONAR non static because is used in fluent calls
	}
	/**
	 * Remove all data related to execution of rules
	 */
	public StoreService dropRules() {
		storage.deleteRules();
		return this;
	}

	/**
	 * Saves the set of rules for a query in the persistent store
	 */
	public void put(StackLocator stack, String sql, QueryParameters params, QueryModel queryModel, SchemaModel schemaModel, ResultVector resultVector) {
		String className = stack.getClassName();
		String methodName = stack.getMethodName();
		int lineNumber = stack.getLineNumber();
		// timestamp indicates the time of rule saving (a little bit more than its execution begin
		Date timestampLast = JavaCs.getCurrentDate(); 
		String queryKey = new QueryKey(className, methodName, lineNumber, sql).toString();
		log.debug("Save file to store: " + queryKey);
		storage.putQueryModel(queryKey, queryModel, params, schemaModel, timestampLast, resultVector);
		
		// Additional info for testing and debugging purposes
		setLastSavedQueryKey(queryKey);
		if (this.lastGeneratedSql != null)
			storage.putSql(queryKey, lastGeneratedSql);
		if (this.lastGeneratedSql != null && stack != null)
			storage.putStack(queryKey, stack.toString());
		if (this.lastGeneratedInRules != null)
			storage.putGeneratedInRules(queryKey, lastGeneratedInRules);

		// To decide if add some parameter to exclude this log (it includes parameters, maybe
		// some confidential data could be stored, e.g. passwords)
		String logRuns = "---- " + timestampLast.toString() + "\nGENERATION: " + this.lastGenStatus + "\nSQL: "
				+ this.lastSqlRun + "\nPARAMS: " + params.toString() + "\n" + this.lastRulesLog + "\n";
		storage.appendLogRun(queryKey, logRuns);
	}

	/**
	 * Retrieves a CoverageManager from the local store given the query coordinates,
	 * null if does not exists
	 */
	public QueryModel get(String className, String methodName, int lineNumber, String sql) {
		String queryKey = new QueryKey(className, methodName, lineNumber, sql).toString();
		return this.getQueryModel(queryKey);
	}

	/**
	 * Retrieves a QueryModel from the local store given the query key,
	 * null if it does not exist
	 */
	public QueryModel getQueryModel(String queryKey) {
		return storage.getQueryModel(queryKey);
	}

	public void setLastGeneratedSql(String sql) {
		this.lastGeneratedSql = sql;
	}
	public void setLastGeneratedInRules(String xml) {
		this.lastGeneratedInRules = xml;
	}
	public String getLastSqlRun() {
		return lastSqlRun;
	}
	public void setLastSqlRun(String lastSqlRun) {
		this.lastSqlRun = lastSqlRun;
	}
	public void setLastParametersRun(String params) {
		this.lastParametersRun = params;
	}
	public String getLastParametersRun() {
		return lastParametersRun;
	}
	public String getLastRulesLog() {
		return lastRulesLog;
	}
	public void setLastRulesLog(String lastRulesLog) {
		this.lastRulesLog = lastRulesLog;
	}
	public String getLastSavedQueryKey() {
		return lastSavedQueryKey;
	}
	private void setLastSavedQueryKey(String lastSavedQueryKey) {
		this.lastSavedQueryKey = lastSavedQueryKey;
	}
	public String getLastGenStatus() {
		return lastGenStatus;
	}
	public void setLastGenStatus(String lastGenStatus) {
		this.lastGenStatus = lastGenStatus;
	}

}
