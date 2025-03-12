using Giis.Portable.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////

namespace Giis.Qacover.Model
{
    /// <summary>
    /// Unique identifier of a query that is being evaluated in the form
    /// package+class.method.line.hash, where the cash encodes the SQL query to allow
    /// differentiate between queries that are executed from the same line.
    /// </summary>
    public class QueryKey
    {
        private string key;
        public QueryKey(string className, string methodName, int lineNumber, string sql)
        {
            key = className + "." + methodName + "." + lineNumber + "." + GetSqlHash(sql);

            // On java, queries executed in the constructor are <init>, replace this kind of chars
            if (key.Contains("<"))
                key = key.Replace("<", "-");
            if (key.Contains(">"))
                key = key.Replace(">", "-");
        }

        public QueryKey(string stringKey)
        {
            key = stringKey;

            // If string comes from a filename, removes the extension because is not part of the key
            if (key.EndsWith(".xml"))
                key = JavaCs.Substring(key, 0, key.Length - 4);
        }

        private string GetSqlHash(string sql)
        {
            if (new Variability().IsJava4())
                return JavaCs.GetHashMd5(sql); //standard sha256 hash not available in Java 1.4
            else
                return JavaCs.GetHash(sql);
        }

        public override string ToString()
        {
            return key;
        }

        public virtual string GetKey()
        {
            return key;
        }

        public virtual string GetClassName()
        {
            string[] parts = JavaCs.SplitByDot(key);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < parts.Length - 3; i++)
                sb.Append(i == 0 ? "" : ".").Append(parts[i]);
            return sb.ToString();
        }

        public virtual string GetMethodName()
        {
            return GetMethodName(false);
        }

        /// <summary>
        /// Gets the method name with the line number (opt in)
        /// </summary>
        public virtual string GetMethodName(bool includeLineNumbers)
        {
            string[] parts = JavaCs.SplitByDot(key);
            return parts[parts.Length - 3] + (includeLineNumbers ? ":" + parts[parts.Length - 2] : "");
        }

        public virtual string GetClassLine()
        {
            string[] parts = JavaCs.SplitByDot(key);
            return parts[parts.Length - 2];
        }
    }
}