using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////

namespace Giis.Qacover.Reader
{
    /// <summary>
    /// A collections of QueryCollection to organize the summarizing and display,
    /// with an on-demand global summary of all QueryCollection, similar
    /// structure than QueryCollection and and lazy access to the details.
    /// </summary>
    public class CoverageCollection
    {
        private IList<QueryCollection> queries = new List<QueryCollection>();
        private CoverageSummary summary;
        public virtual void Add(QueryCollection queryCol)
        {
            queries.Add(queryCol);
        }

        public virtual int GetSize()
        {
            return queries.Count;
        }

        public virtual QueryCollection GetItem(int position)
        {
            return queries[position];
        }

        public virtual CoverageSummary GetSummary()
        {
            if (summary == null)
            {
                summary = new CoverageSummary();
                foreach (QueryCollection rules in queries)
                {
                    CoverageSummary partial = rules.GetSummary();
                    summary.AddQueryCounters(partial.GetQcount(), partial.GetQrun(), partial.GetQerror());
                    summary.AddRuleCounters(partial.GetCount(), partial.GetDead(), partial.GetError());
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
            sb.Append("CoverageCollection:").Append(includeSummary ? " " + this.GetSummary() : "");
            for (int i = 0; i < this.GetSize(); i++)
            {
                sb.Append("\n").Append(this.GetItem(i).ToString(includeSummary, includeLineNumbers, includeFiles));
            }

            return sb.ToString();
        }
    }
}