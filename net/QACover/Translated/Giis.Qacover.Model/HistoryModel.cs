/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Text;
using Giis.Portable.Util;
using Giis.Portable.Xml.Tiny;


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
		private HistoryDao dao;

		/// <summary>Creates an history model from a query that has been evaluated with the given parameters</summary>
		public HistoryModel(DateTime timestamp, string key, QueryParameters @params)
		{
			this.dao = new HistoryDao();
			this.dao.at = JavaCs.GetIsoDate(timestamp);
			this.dao.key = key;
			this.dao.@params = @params.ToDao();
		}

		/// <summary>Create an history model from a string read from the storage</summary>
		public HistoryModel(string item)
		{
			if (item.StartsWith("{"))
			{
				throw new Exception("V2 history still not implemented");
			}
			else
			{
				LoadHistoryItemV1(item);
			}
		}

		private void LoadHistoryItemV1(string item)
		{
			string[] splitted = JavaCs.SplitByBar(item);
			// should have 3 at least
			this.dao = new HistoryDao();
			this.dao.at = JavaCs.GetIsoDate(JavaCs.ParseIsoDate(splitted[0]));
			this.dao.key = splitted[1];
			// If more than 3, joins the remaining items
			string paramStr = string.Empty;
			for (int i = 2; i < splitted.Length; i++)
			{
				paramStr += (i == 2 ? string.Empty : "|") + splitted[i];
			}
			// NOSONAR
			this.dao.@params = ParamsFromXml(paramStr);
		}

		public virtual string GetTimestampString()
		{
			return dao.at;
		}

		public virtual string GetKey()
		{
			return dao.key;
		}

		public virtual string GetParamsXml()
		{
			return ParamsToXml(dao.@params);
		}

		private string ParamsToXml(IList<ParameterDao> @params)
		{
			StringBuilder sb = new StringBuilder();
			foreach (ParameterDao param in @params)
			{
				sb.Append("<parameter name=\"").Append(XNodeAbstract.EncodeAttribute(param.name)).Append("\" value=\"").Append(XNodeAbstract.EncodeAttribute(param.value)).Append("\" />");
			}
			return "<parameters>" + sb.ToString() + "</parameters>";
		}

		private IList<ParameterDao> ParamsFromXml(string paramXml)
		{
			IList<ParameterDao> dao = new List<ParameterDao>();
			IList<XNode> paramNodes = new XNode(paramXml).GetChildren("parameter");
			foreach (XNode paramNode in paramNodes)
			{
				dao.Add(new ParameterDao(XNodeAbstract.DecodeAttribute(paramNode.GetAttribute("name")), XNodeAbstract.DecodeAttribute(paramNode.GetAttribute("value"))));
			}
			return dao;
		}

		public virtual string ToStringV1()
		{
			return GetTimestampString() + "|" + GetKey() + "|" + GetParamsXml();
		}
	}
}
