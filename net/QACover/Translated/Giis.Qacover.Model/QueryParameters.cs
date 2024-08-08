/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Text;


namespace Giis.Qacover.Model
{
	/// <summary>Map of parameter-value of a query that is being evaluated.</summary>
	/// <remarks>
	/// Map of parameter-value of a query that is being evaluated. Internally stores
	/// two maps that differ on the value:
	/// (1) value as string, the same that p2spy provides (already with quotes in string types)
	/// (2) object to manage specific features that depend on the datatype
	/// </remarks>
	public class QueryParameters
	{
		protected internal IDictionary<string, string> parameters = new SortedDictionary<string, string>();

		protected internal IDictionary<string, object> parameterObjects = new SortedDictionary<string, object>();

		public virtual int Size()
		{
			return parameters.Count;
		}

		public virtual bool ContainsKey(string name)
		{
			return parameters.ContainsKey(name);
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			IList<string> keys = this.KeySet();
			for (int i = 0; i < keys.Count; i++)
			{
				sb.Append(i == 0 ? string.Empty : ", ").Append(keys[i]).Append("=").Append(parameters[keys[i]]);
			}
			return "{" + sb.ToString() + "}";
		}

		public virtual IList<ParameterDao> ToDao()
		{
			IList<ParameterDao> dao = new List<ParameterDao>();
			IList<string> keys = this.KeySet();
			foreach (string key in keys)
			{
				dao.Add(new ParameterDao(key, parameters[key]));
			}
			return dao;
		}

		public virtual void Put(string name, string valueString, object valueObject)
		{
			parameters[name] = valueString;
			parameterObjects[name] = valueObject;
		}

		public virtual void Put(string name, string valueString)
		{
			parameters[name] = valueString;
			parameterObjects[name] = valueString;
		}

		public virtual string Get(string name)
		{
			return parameters[name];
		}

		public virtual IList<string> KeySet()
		{
			IList<string> keys = new List<string>();
			foreach (string name in parameters.Keys)
			{
				keys.Add(name);
			}
			return keys;
		}

		public virtual bool IsDate(string name)
		{
			return parameterObjects.ContainsKey(name) && parameterObjects[name] is DateTime;
		}
	}
}
