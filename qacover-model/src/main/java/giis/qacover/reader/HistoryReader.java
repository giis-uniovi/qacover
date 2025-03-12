package giis.qacover.reader;

import java.util.ArrayList;
import java.util.List;

import giis.qacover.model.HistoryModel;
import giis.qacover.model.QueryKey;

/**
 * Stores the run history to give access to the series parameters used to run each query
 */
public class HistoryReader {

	private List<HistoryModel> history;

	public HistoryReader(List<HistoryModel> history) {
		this.history = history;
	}

	public List<HistoryModel> getItems() {
		return this.history;
	}

	/**
	 * Returns an history reader that contains only those items that correspond
	 * to the executions of a query (given by his key)
	 */
	public HistoryReader getHistoryAtQuery(QueryKey target) {
		List<HistoryModel> selection = new ArrayList<HistoryModel>();
		for (HistoryModel item : history) {
			QueryKey key = new QueryKey(item.getKey());
			if (target.getKey().equals(key.getKey()))
				selection.add(item);
		}
		return new HistoryReader(selection);
	}

}
