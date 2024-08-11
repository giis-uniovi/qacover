/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;


namespace Giis.Qacover.Core
{
	/// <summary>Provides read access to the data stored in the database to evaluate the rules</summary>
	public interface IQueryStatementReader
	{
		/// <summary>
		/// Determines if the execution of a query returns at least one row
		/// (to evaluate the fpc coverage)
		/// </summary>
		bool HasRows();

		/// <summary>Gets all rows returned by a sql statement</summary>
		IList<string[]> GetRows();

		/// <summary>Determines if the list of rows is the same than the returned by a sql statement</summary>
		bool EqualRows(IList<string[]> expected);
	}
}
