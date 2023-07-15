package giis.qacover.reader;

import java.util.ArrayList;
import java.util.List;

/**
 * A collections of QueryCollection to organize the summarizing and display,
 * with an on-demand global summary of all QueryCollection, similar
 * structure than QueryCollection and and lazy access to the details.
 */
public class CoverageCollection {
	private List<QueryCollection> queries = new ArrayList<>();
	private CoverageSummary summary;

	public void add(QueryCollection queryCol) {
		queries.add(queryCol);
	}

	public int size() {
		return queries.size();
	}
	public QueryCollection get(int position) {
		return queries.get(position);
	}
	public CoverageSummary getSummary() {
		if (summary == null) {
			summary = new CoverageSummary();
			for (QueryCollection rules : queries) {
				CoverageSummary partial = rules.getSummary();
				summary.addQueryCounters(partial.getQcount(), partial.getQrun(), partial.getQerror());
				summary.addRuleCounters(partial.getCount(), partial.getDead(), partial.getError());
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
		sb.append("CoverageCollection:").append(includeSummary ? " " + this.getSummary() : "");
		for (int i = 0; i < this.size(); i++) {
			sb.append("\n").append(this.get(i).toString(includeSummary, includeLineNumbers, includeFiles));
		}
		return sb.toString();
	}
}
