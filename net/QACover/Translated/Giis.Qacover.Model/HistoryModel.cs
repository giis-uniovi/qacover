/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Giis.Portable.Util;


namespace Giis.Qacover.Model
{
	/// <summary>
	/// The history model holds secuentially the reference to each query evaluation:
	/// (1) datetime
	/// (2) QueryKey
	/// (3) Parameters used in the evaluation
	/// The string representation of each item is a csv with | as separator
	/// </summary>
	public class HistoryModel
	{
		private DateTime timestamp;

		private string key = string.Empty;

		private string @params = string.Empty;

		public HistoryModel(DateTime timestamp, string key, string @params)
		{
			this.timestamp = timestamp;
			this.key = key;
			this.@params = @params;
		}

		public HistoryModel(string item)
		{
			string[] splitted = JavaCs.SplitByBar(item);
			// should have 3 at least
			timestamp = JavaCs.ParseIsoDate(splitted[0]);
			key = splitted[1];
			// If more than 3, joins the remaining items
			for (int i = 2; i < splitted.Length; i++)
			{
				@params = GetParams() + (i == 2 ? string.Empty : "|") + splitted[i];
			}
		}

		// NOSONAR
		public virtual DateTime GetTimestamp()
		{
			return timestamp;
		}

		public virtual string GetTimestampString()
		{
			return JavaCs.GetIsoDate(timestamp);
		}

		public virtual string GetKey()
		{
			return key;
		}

		public virtual string GetParams()
		{
			return @params;
		}

		public override string ToString()
		{
			return JavaCs.GetIsoDate(GetTimestamp()) + "|" + GetKey() + "|" + GetParams();
		}
	}
}
