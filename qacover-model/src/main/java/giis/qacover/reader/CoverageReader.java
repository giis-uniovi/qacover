package giis.qacover.reader;

import java.util.List;
import java.util.Map;
import java.util.TreeMap;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import giis.qacover.model.HistoryModel;
import giis.qacover.model.QueryKey;
import giis.qacover.storage.LocalStore;

/**
 * This is the API to access to all information about the evaluation, grouped by
 * different criteria. Creates collections (CoverageCollection or QueryCollection)
 * to lazy access the stored information.
 */
public class CoverageReader {
	private static final Logger log = LoggerFactory.getLogger(CoverageReader.class);
	private String rulesFolder;

	/**
	 * Instantiation from the store location
	 */
	public CoverageReader(String rulesFolder) {
		this.rulesFolder = rulesFolder;
	}

	/**
	 * Gets a CoverageCollection with an item for each class,
	 * the QueryCollections will have all queries in this class
	 */
	public CoverageCollection getByClass() {
		log.trace("CoverageReader.GetByClass, Processing files in folder: " + rulesFolder);
		List<String> files = new LocalStore(rulesFolder).getRuleItems();
		// saves the query collection for each class
		Map<String, QueryCollection> index = new TreeMap<String, QueryCollection>(); 
		CoverageCollection target = new CoverageCollection(); // collection to return
		for (String fileName : files) {
			log.trace("Processing file: " + fileName);
			String className = new QueryKey(fileName).getClassName();
			QueryCollection current;
			if (index.containsKey(className)) { // already indexed
				current = index.get(className);
			} else { // new, adds to map of query collections
				current = new QueryCollection(rulesFolder, className);
				index.put(className, current);
				target.add(current);
			}
			current.add(new QueryReader(rulesFolder, fileName));
		}
		return target;
	}

	/**
	 * Gets a QueryCollection with all data of the executions in time order
	 */
	public QueryCollection getByRunOrder() {
		log.trace("CoverageReader.getByRunOrder, Processing history for folder: " + rulesFolder);
		LocalStore storage = new LocalStore(rulesFolder);
		List<HistoryModel> hitems = storage.getHistoryItems();
		QueryCollection target = new QueryCollection(rulesFolder, "all-executions");
		for (HistoryModel hitem : hitems) {
			QueryReader item = new QueryReader(rulesFolder, hitem.getKey());
			item.setTimestamp(hitem.getTimestampString());
			item.setParams(hitem.getParams());
			target.add(item);
		}
		return target;
	}

}
