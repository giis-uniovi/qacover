package giis.qacover.reader;

import java.util.SortedMap;
import java.util.TreeMap;

/**
 * Collects all info about source code and coverage of queries executed at each
 * line. Indexed by line number, each can contain source code or queries or both
 */
public class SourceCodeCollection {

	private TreeMap<Integer, SourceCodeLine> lines = new TreeMap<>();
	
	public SortedMap<Integer, SourceCodeLine> getLines() {
		return lines;
	}

	// Gets the indicated Line object, creates a new if not exists
	private SourceCodeLine getLine(int lineNumber) {
		if (lines.containsKey(lineNumber))
			return lines.get(lineNumber);
		SourceCodeLine newLine = new SourceCodeLine();
		lines.put(lineNumber, newLine);
		return newLine;
	}

	/**
	 * Adds a all queries to this collection. Each will be inserted at the line that
	 * corresponds with the line where the query was run
	 */
	public void addQueries(QueryCollection queries) {
		for (int i = 0; i < queries.size(); i++)
			addQuery(queries.get(i));
	}

	/**
	 * Adds a query to this collection. This will be inserted at the line that
	 * corresponds with the line where the query was run
	 */
	public void addQuery(QueryReader query) {
		int lineNumber = Integer.parseInt(query.getKey().getClassLine());
		SourceCodeLine line = getLine(lineNumber);
		line.getQueries().add(query);
	}

}
