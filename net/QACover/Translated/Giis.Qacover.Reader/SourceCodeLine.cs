/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;


namespace Giis.Qacover.Reader
{
	public class SourceCodeLine
	{
		private string source = null;

		private IList<QueryReader> queries = new List<QueryReader>();

		// If source code is available, this will contain this line of source code
		// If a query was run at this line, this contains the QueryReader (allows more
		// than one)
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
