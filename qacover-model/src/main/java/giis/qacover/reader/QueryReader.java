package giis.qacover.reader;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import giis.qacover.model.QueryKey;
import giis.qacover.model.QueryModel;
import giis.qacover.model.SchemaModel;
import giis.qacover.storage.LocalStore;

/**
 * Represents all information to access to a query and its coverage rules given a QueryKey.
 * It is instantiated from the store folder and gets the rest of info by demand.
 * Used to create and manage collections of queries with a lazy access the details.
 * Additionally it can store the timestamp and parameters sent to the query
 * (this allows to use this object for reading the current state or an execution)
 */
public class QueryReader {
	private static final Logger log = LoggerFactory.getLogger(QueryReader.class);
	private String folder; // store folder with the rules
	private QueryKey queryKey;
	// Next data is optional
	private String timestamp = ""; // fecha formato iso
	private String params = ""; // parametros en xml

	public QueryReader(String rulesFolder, String stringKey) {
		queryKey = new QueryKey(stringKey);
		folder = rulesFolder;
	}

	public void setTimestamp(String value) {
		timestamp = value;
	}
	public void setParams(String value) {
		params = value;
	}
	public QueryKey getKey() {
		return queryKey;
	}
	
	/**
	 * Reads the QueryModel from the store
	 */
	public QueryModel getModel() {
		return new LocalStore(folder).getQueryModel(queryKey.toString());
	}
	/**
	 * Reads the QueryModel from the stored rules
	 */
	public String getSql() {
		return this.getModel().getSql();
	}
	public String getTimestamp() {
		return timestamp;
	}
	public SchemaModel getSchema() {
		try {
			return new LocalStore(folder).getSchema(queryKey.toString());
		} catch (RuntimeException e) {
			// A QueryModel may have no schema (if the query execution failed)
			log.warn("Schema not found for rule " + queryKey.toString());
			return new SchemaModel();
		}
	}
	public String getParametersXml() {
		return params;
	}

}
