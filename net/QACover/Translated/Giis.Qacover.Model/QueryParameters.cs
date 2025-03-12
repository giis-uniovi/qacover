using Java.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////

namespace Giis.Qacover.Model
{
    /// <summary>
    /// Map of parameter-value of a query that is being evaluated. Internally stores
    /// two maps that differ on the value:
    /// (1) value as string, the same that p2spy provides (already with quotes in string types)
    /// (2) object to manage specific features that depend on the datatype
    /// </summary>
    public class QueryParameters
    {
        protected Map<string, string> parameters = new TreeMap<string, string>();
        protected Map<string, object> parameterObjects = new TreeMap<string, object>();
        public virtual int GetSize()
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
                sb.Append(i == 0 ? "" : ", ").Append(keys[i]).Append("=").Append(parameters[keys[i]]);
            return "{" + sb.ToString() + "}";
        }

        public virtual IList<ParameterDao> ToDao()
        {
            IList<ParameterDao> dao = new List<ParameterDao>();
            IList<string> keys = this.KeySet();
            foreach (string key in keys)
                dao.Add(new ParameterDao(key, parameters[key]));
            return dao;
        }

        public virtual void PutItem(string name, string valueString, object valueObject)
        {
            parameters.Put(name, valueString);
            parameterObjects.Put(name, valueObject);
        }

        public virtual void PutItem(string name, string valueString)
        {
            parameters.Put(name, valueString);
            parameterObjects.Put(name, valueString);
        }

        public virtual string GetItem(string name)
        {
            return parameters[name];
        }

        public virtual IList<string> KeySet()
        {
            IList<string> keys = new List<string>();
            foreach (string name in parameters.KeySet())
                keys.Add(name);
            return keys;
        }

        public virtual bool IsDate(string name)
        {
            return parameterObjects.ContainsKey(name) && parameterObjects[name] is DateTime;
        }
    }
}