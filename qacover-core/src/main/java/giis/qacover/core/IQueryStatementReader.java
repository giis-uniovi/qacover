package giis.qacover.core;

import java.util.List;

/**
 * Provides read access to the data stored in the database to evaluate the rules
 */
public interface IQueryStatementReader {
	
	/**
	 * Determines if the execution of a query returns at least one row
	 * (to evaluate the fpc coverage)
	 */
	public boolean hasRows();

	/**
	 * Gets all rows returned by a sql statement
	 */
	public List<String[]> getRows();
	
	/**
	 * Determines if the list of rows is the same than the returned by a sql statement
	 */
	public boolean equalRows(List<String[]> expected);

}
