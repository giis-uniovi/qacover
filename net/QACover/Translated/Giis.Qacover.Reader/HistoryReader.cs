/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using Giis.Qacover.Model;


namespace Giis.Qacover.Reader
{
	/// <summary>Stores the run history to give access to the series parameters used to run each query</summary>
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
		public virtual Giis.Qacover.Reader.HistoryReader GetHistoryAtQuery(QueryKey target)
		{
			IList<HistoryModel> selection = new List<HistoryModel>();
			foreach (HistoryModel item in history)
			{
				QueryKey key = new QueryKey(item.GetKey());
				if (target.GetKey().Equals(key.GetKey()))
				{
					selection.Add(item);
				}
			}
			return new Giis.Qacover.Reader.HistoryReader(selection);
		}
	}
}
