using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////

namespace Giis.Qacover.Model
{
    /// <summary>
    /// DTO to store both a sql query and its parameters
    /// </summary>
    public class QueryWithParameters
    {
        private string sql;
        protected QueryParameters parameters = new QueryParameters();
        public virtual QueryParameters GetParams()
        {
            return parameters;
        }

        public virtual void PutParam(string name, string value)
        {
            parameters.PutItem(name, value);
        }

        public virtual string GetSql()
        {
            return sql;
        }

        public virtual void SetSql(string sql)
        {
            this.sql = sql;
        }
    }
}