using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////

namespace Giis.Qacover.Core
{
    /// <summary>
    /// Provides read access to the data stored in the database to evaluate the rules
    /// </summary>
    public interface IQueryStatementReader
    {
        /// <summary>
        /// Determines if the execution of a query returns at least one row
        /// (to evaluate the fpc coverage)
        /// </summary>
        bool HasRows();
        /// <summary>
        /// Gets all rows returned by a sql statement
        /// </summary>
        IList<String[]> GetRows();
        /// <summary>
        /// Determines if the list of rows is the same than the returned by a sql statement
        /// </summary>
        bool EqualRows(IList<String[]> expected);
    }
}