package giis.qacover.reader;

import java.util.ArrayList;
import java.util.List;

import giis.qacover.model.QueryModel;
import giis.qacover.storage.LocalStore;

/**
 * Allows accesing to a collection of QueryModels via the stored list of
 * QueryReaders, with an on-demand generated summary of all items.
 * Lazy access to the QueryModel data from the stored QueryReader
 */
public class QueryCollection {
	private List<QueryReader> items = new ArrayList<QueryReader>();
	private String folder; // store folder
	private String name; // a name given to differentiate from other collections
	private CoverageSummary summary = null; // optional, created on demand

	public QueryCollection(String rulesFolder, String collectionName) {
		this.folder = rulesFolder;
		this.name = collectionName;
	}

	public void add(QueryReader item) {
		items.add(item);
	}
	
	public String getName() {
		return name;
	}
	/**
	 * Number of QueryReaders stored in this instance
	 */
	public int getSize() {
		return items.size();
	}
	/**
	 * Gets the QueryReader at the indicated position
	 */
	public QueryReader getItem(int position) {
		return items.get(position);
	}
	
	/**
	 * Gets a coverage summary of all rules in all queries in this collection
	 */
	public CoverageSummary getSummary() {
		if (summary == null) { // lazy access
			summary = new CoverageSummary();
			for (QueryReader key : items) {
				QueryModel rules = new LocalStore(folder).getQueryModel(key.getKey().toString());
				summary.addQueryCounters(1, rules.getQrun(), rules.getQerror());
				summary.addRuleCounters(rules.getCount(), rules.getDead(), rules.getError());
			}
		}
		return summary;
	}
	
	@Override
	public String toString() {
		return toString(false, false, false);
	}

	public String toString(boolean includeSummary, boolean includeLineNumbers, boolean includeFiles) {
		StringBuilder sb = new StringBuilder();
		sb.append("QueryCollection: ").append(this.getName())
			.append(includeSummary ? " " + this.getSummary() : "");
		for (int i = 0; i < this.getSize(); i++) {
			sb.append("\n  ").append(this.getItem(i).getKey().getMethodName(includeLineNumbers))
				.append(includeFiles ? " " + this.items.get(i).getKey() : "");
		}
		return sb.toString();
	}
	
}
