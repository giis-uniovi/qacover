package giis.qacover.storage;

import java.util.ArrayList;
import java.util.Collections;
import java.util.Date;
import java.util.List;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import giis.portable.util.FileUtil;
import giis.qacover.model.HistoryModel;
import giis.qacover.model.QueryModel;
import giis.qacover.model.QueryParameters;
import giis.qacover.model.ResultVector;
import giis.qacover.model.SchemaModel;
import giis.tdrules.model.io.TdRulesXmlSerializer;
import giis.tdrules.model.io.TdSchemaXmlSerializer;
import giis.tdrules.openapi.model.TdRules;
import giis.tdrules.openapi.model.TdSchema;

/**
 * Manages the persistence of the model objects (queries, rules and its
 * evaluation). Currently there only exist a storage in local folders.
 * 
 * Three kinds of information are stored:
 * (1) Query models: a file for each query with the rules and evaluation results,
 * the name of the file is the QueryKey (full qualified method name, line, sql hash)
 * (2) History: A single file with a line for each query evaluation, contains a csv
 * of datetime, QueryKey and paramters used in the execution
 * (3) Additional files for debugging to store the sql, schema, rules and
 * textual details about the evaluation of each query
 */
public class LocalStore {
	private static final Logger log = LoggerFactory.getLogger(LocalStore.class);
	
	private static final String HISTORY_FILE_NAME = "00HISTORY.log";
	// Main folder to store the rules
	protected String storeLocation;
	// Addicional folders for debug info (must have siblings to the storeLocation)
	protected String storeLocationSchema;
	protected String storeLocationSql;
	protected String storeLocationStack;
	protected String storeLocationInRules;
	protected String storeLocationRuns;

	public LocalStore(String location) {
		storeLocation = location;
		storeLocationSql = FileUtil.getPath(storeLocation, "log-sql");
		storeLocationStack = FileUtil.getPath(storeLocation, "log-stack");
		storeLocationSchema = FileUtil.getPath(storeLocation, "log-schema");
		storeLocationInRules = FileUtil.getPath(storeLocation, "log-inrules");
		storeLocationRuns = FileUtil.getPath(storeLocation, "log-runs");
	}

	public void ensureStoreFolders() {
		FileUtil.createDirectory(storeLocation);
		FileUtil.createDirectory(storeLocationSchema);
		FileUtil.createDirectory(storeLocationSql);
		FileUtil.createDirectory(storeLocationStack);
		FileUtil.createDirectory(storeLocationInRules);
		FileUtil.createDirectory(storeLocationRuns);
	}

	public String getStoreLocation() {
		return storeLocation;
	}

	/**
	 * Stores the query model, overwrite if already exists
	 */
	public void putQueryModel(String queryKey, QueryModel queryModel, QueryParameters params, SchemaModel schemaModel, Date timestamp, ResultVector resultVector) {
		FileUtil.fileWrite(storeLocation, queryKey + ".xml",
				new TdRulesXmlSerializer().serialize(queryModel.getModel()));
		addHistoryItem(timestamp, queryKey, params, resultVector);
		// The schema model is only available for new generated rules, second time that a rule is evaluated
		// it is read from the store and does not need the schema
		if (schemaModel != null)
			putSchema(queryKey, schemaModel);
	}
	
	private void addHistoryItem(Date timestamp, String queryKey, QueryParameters params, ResultVector resultVector) {
		HistoryModel historyLog = new HistoryModel(timestamp, queryKey, params, resultVector);
		FileUtil.fileAppend(storeLocation, HISTORY_FILE_NAME, historyLog.toStringV2() + "\n");
	}
	
	private void putSchema(String queryKey, SchemaModel schema) {
		FileUtil.fileWrite(storeLocationSchema, queryKey + ".xml",
				new TdSchemaXmlSerializer().serialize(schema.getModel()));
	}
	
	/**
	 * Retrieves the rule model that must be previously stored,
	 * null if does not exist
	 */
	public QueryModel getQueryModel(String queryKey) {
		String frules = FileUtil.fileRead(storeLocation, queryKey + ".xml", false);
		log.trace("Try get QueryModel file from store: " + queryKey);
		if (frules == null) {
			log.trace("QueryModel file has not been generated");
			return null;
		}
		log.trace("QueryModel file found");
		TdRules srules = new TdRulesXmlSerializer().deserialize(frules);
		return new QueryModel(srules);
	}
	
	/**
	 * Gests all records in the store history
	 */
	public List<HistoryModel> getHistoryItems() {
		String logFile = FileUtil.getPath(storeLocation, HISTORY_FILE_NAME);
		List<String> lines = FileUtil.fileReadLines(logFile);
		List<HistoryModel> items = new ArrayList<>();
		for (String line : lines)
			items.add(new HistoryModel(line));
		return items;
	}

	/**
	 * Gets the file names of all stored query models
	 */
	public List<String> getRuleItems() {
		List<String> files = FileUtil.getFileListInDirectory(storeLocation);
		for (int i = files.size() - 1; i >= 0; i--)
			if (!files.get(i).endsWith(".xml")) // only .xml (e.g. exclude history)
				files.remove(i);
		Collections.sort(files); // To make repetible
		return files;
	}

	public SchemaModel getSchema(String ruleKey) {
		String fschema = FileUtil.fileRead(storeLocationSchema, ruleKey + ".xml");
		TdSchema schema = new TdSchemaXmlSerializer().deserialize(fschema);
		return new SchemaModel(schema);
	}

	/**
	 * Removes all stored query models, used to reset the execution
	 */
	public void deleteRules() {
		FileUtil.deleteFilesInDirectory(storeLocation);
		FileUtil.deleteFilesInDirectory(storeLocationSchema);
		FileUtil.deleteFilesInDirectory(storeLocationSql);
		FileUtil.deleteFilesInDirectory(storeLocationStack);
		FileUtil.deleteFilesInDirectory(storeLocationInRules);
		FileUtil.deleteFilesInDirectory(storeLocationRuns);
	}
	
	// Storing of additional information for debug

	public void putSql(String queryKey, String sql) {
		FileUtil.fileWrite(storeLocationSql, queryKey + ".sql", sql);
	}

	public void putStack(String queryKey, String stack) {
		FileUtil.fileWrite(storeLocationStack, queryKey + ".txt", queryKey + stack);
	}

	public void putGeneratedInRules(String queryKey, String lastGeneratedInRules) {
		FileUtil.fileWrite(storeLocationInRules, queryKey + ".xml", lastGeneratedInRules);
	}

	public void appendLogRun(String queryKey, String logRun) {
		FileUtil.fileAppend(storeLocationRuns, queryKey + ".log", logRun);
	}

}
