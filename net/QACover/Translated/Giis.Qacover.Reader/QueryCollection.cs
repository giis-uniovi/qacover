using Giis.Qacover.Model;
using Giis.Qacover.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////

namespace Giis.Qacover.Reader
{
    /// <summary>
    /// Allows accesing to a collection of QueryModels via the stored list of
    /// QueryReaders, with an on-demand generated summary of all items.
    /// Lazy access to the QueryModel data from the stored QueryReader
    /// </summary>
    public class QueryCollection
    {
        private IList<QueryReader> items = new List<QueryReader>();
        private string folder; // store folder
        private string name; // a name given to differentiate from other collections
        private CoverageSummary summary = null; // optional, created on demand
        public QueryCollection(string rulesFolder, string collectionName)
        {
            this.folder = rulesFolder;
            this.name = collectionName;
        }

        public virtual void Add(QueryReader item)
        {
            items.Add(item);
        }

        public virtual string GetName()
        {
            return name;
        }

        /// <summary>
        /// Number of QueryReaders stored in this instance
        /// </summary>
        public virtual int GetSize()
        {
            return items.Count;
        }

        /// <summary>
        /// Gets the QueryReader at the indicated position
        /// </summary>
        public virtual QueryReader GetItem(int position)
        {
            return items[position];
        }

        /// <summary>
        /// Gets a coverage summary of all rules in all queries in this collection
        /// </summary>
        public virtual CoverageSummary GetSummary()
        {
            if (summary == null)
            {

                // lazy access
                summary = new CoverageSummary();
                foreach (QueryReader key in items)
                {
                    QueryModel rules = new LocalStore(folder).GetQueryModel(key.GetKey().ToString());
                    summary.AddQueryCounters(1, rules.GetQrun(), rules.GetQerror());
                    summary.AddRuleCounters(rules.GetCount(), rules.GetDead(), rules.GetError());
                }
            }

            return summary;
        }

        public override string ToString()
        {
            return ToString(false, false, false);
        }

        public virtual string ToString(bool includeSummary, bool includeLineNumbers, bool includeFiles)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("QueryCollection: ").Append(this.GetName()).Append(includeSummary ? " " + this.GetSummary() : "");
            for (int i = 0; i < this.GetSize(); i++)
            {
                sb.Append("\n  ").Append(this.GetItem(i).GetKey().GetMethodName(includeLineNumbers)).Append(includeFiles ? " " + this.items[i].GetKey() : "");
            }

            return sb.ToString();
        }
    }
}