using Giis.Qacover.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////

namespace Giis.Qacover.Reader
{
    /// <summary>
    /// Stores the run history to give access to the series parameters used to run each query
    /// </summary>
    public class HistoryReader
    {
        private IList<HistoryModel> history;
        public HistoryReader(IList<HistoryModel> history)
        {
            this.history = history;
        }

        public virtual IList<HistoryModel> GetItems()
        {
            return this.history;
        }

        /// <summary>
        /// Returns an history reader that contains only those items that correspond
        /// to the executions of a query (given by his key)
        /// </summary>
        public virtual HistoryReader GetHistoryAtQuery(QueryKey target)
        {
            IList<HistoryModel> selection = new List<HistoryModel>();
            foreach (HistoryModel item in history)
            {
                QueryKey key = new QueryKey(item.GetKey());
                if (target.GetKey().Equals(key.GetKey()))
                    selection.Add(item);
            }

            return new HistoryReader(selection);
        }
    }
}