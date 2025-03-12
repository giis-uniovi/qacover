using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////

namespace Giis.Qacover.Reader
{
    public class SourceCodeLine
    {
        // If source code is available, this will contain this line of source code
        private string source = null;
        // If a query was run at this line, this contains the QueryReader (allows more
        // than one)
        private IList<QueryReader> queries = new List<QueryReader>();
        public virtual string GetSource()
        {
            return source;
        }

        public virtual void SetSource(string source)
        {
            this.source = source;
        }

        public virtual IList<QueryReader> GetQueries()
        {
            return queries;
        }

        public virtual int GetCount()
        {
            int count = 0;
            foreach (QueryReader query in queries)
            {
                count += query.GetModel().GetCount();
            }

            return count;
        }

        public virtual int GetDead()
        {
            int dead = 0;
            foreach (QueryReader query in queries)
            {
                dead += query.GetModel().GetDead();
            }

            return dead;
        }
    }
}