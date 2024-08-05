package giis.qacover.reader;

import java.util.ArrayList;
import java.util.List;

public class SourceCodeLine {
	// If source code is available, this will contain this line of source code
	private String source = null;
	// If a query was run at this line, this contains the QueryReader (allows more
	// than one)
	private List<QueryReader> queries = new ArrayList<>();
	
	public String getSource() {
		return source;
	}
	
	public List<QueryReader> getQueries() {
		return queries;
	}
	
	public int getCount() {
		int count=0;
		for (QueryReader query: queries) {
			count+=query.getModel().getCount();
		}
		return count;
	}
	public int getDead() {
		int dead=0;
		for (QueryReader query: queries) {
			dead+=query.getModel().getDead();
		}
		return dead;
	}
}
