using Java.Sql;
using NLog;
using Giis.Portable.Util;
using Giis.Qacover.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////

namespace Giis.Qacover.Core.Query
{
    /// <summary>
    /// General representation of a database query statement that is going to be evaluated.
    /// 
    /// As this is strongly platform dependent, each platform must declare an adapter that extends this class to
    /// handle specific issues such as getting the connection, the IQueryStatementReader used for evaluation and
    /// parameter management.
    /// 
    /// Also provides a generic method to replace parameters (only used in java, as net manages this inside the
    /// adapter)
    /// </summary>
    public abstract class AbstractQueryStatement
    {
        protected QueryParameters parameters = new QueryParameters();
        protected string sql;
        protected Variability variant;
        protected Exception exception = null; // si ha habido algun error en la creacion
        /// <summary>
        /// Returns the connection to the database.
        /// </summary>
        public abstract Connection GetConnection();
        /// <summary>
        /// Returns an object to browse the data accessed from the current connection
        /// </summary>
        public abstract IQueryStatementReader GetReader(string sql);
        public virtual string GetSql()
        {
            return sql == null ? "" : sql;
        }

        public virtual QueryParameters GetParameters()
        {
            return parameters;
        }

        public virtual Exception GetException()
        {
            return exception;
        }

        public virtual void SetVariant(Variability currentVariant)
        {
            variant = currentVariant;
        }

        /// <summary>
        /// Replaces query parameter placeholders by their values
        /// </summary>
        public virtual string GetSqlWithValues(string sourceSql)
        {
            if (parameters.GetSize() == 0)
                return sourceSql;
            foreach (string name in parameters.KeySet())
                sourceSql = ReplaceSingleParameter(sourceSql, name, GetParameters().GetItem(name));
            return sourceSql;
        }

        protected virtual string ReplaceSingleParameter(string sourceSql, string name, string value)
        {
            return sourceSql.Replace(name, value);
        }

        /// <summary>
        /// Determines if the query is a select. Needed to filter other statements when a generic jdbc execute() method
        /// is used (does not differentiates between query and update)
        /// </summary>
        public static bool IsSelectQuery(string sql, Logger log)
        {
            string sqlStart = JavaCs.Substring(sql.Trim(), 0, 6);
            if (JavaCs.EqualsIgnoreCase("select", sqlStart.ToLower()))
                return true;
            log.Debug("---- Ignored by qacover. clause is " + sqlStart);
            return false;
        }
    }
}