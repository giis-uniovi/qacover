using Java.Sql;
using NLog;
using Giis.Portable.Util;
using Giis.Qacover.Core.Services;
using Giis.Qacover.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////

namespace Giis.Qacover.Core
{
    /// <summary>
    /// Representation of a jdbc Statement that has been intercepted by p6spy.
    /// On Java is created from a p6spy adapter when executed in response to the
    /// onBeforeExecuteQuery event.
    /// The main attributes are the query and parameters (in the case of Prepared Statements).
    /// Also keeps track of the variability and fault injection for testing.
    /// </summary>
    public abstract class QueryStatement
    {
        protected QueryParameters parameters = new QueryParameters();
        protected string sql;
        protected Variability variant;
        protected Exception exception = null; // si ha habido algun error en la creacion
        private static FaultInjector faultInjector = null;
        public virtual void SetVariant(Variability currentVariant)
        {
            variant = currentVariant;
        }

        /// <summary>
        /// If the query does not have parameters, transforms this object as a result
        /// of the parameters inference on the query
        /// </summary>
        public virtual void InferParameters(RuleServices svc, string storetype)
        {
            if (GetParameters().GetSize() > 0)
                return;
            QueryWithParameters queryAndParam = svc.InferQueryWithParameters(sql, storetype);
            sql = queryAndParam.GetSql(); // este sql sera el que tenga parametros y se usara a partir de ahora
            parameters = queryAndParam.GetParams();
        }

        /// <summary>
        /// Parses a comment anotation to allow named jdbc parameters
        /// (by encolsing a starting comment in the query)
        /// </summary>
        protected virtual IList<string> ParseNamedParameters(string sql)
        {
            string comment = sql;
            IList<string> spec = new List<string>();

            // comment before starting teh query
            if (comment.StartsWith("/*"))
                comment = JavaCs.Substring(comment, 2, comment.Length);
            else
                return spec;
            if (comment.IndexOf("*/") > -1)
                comment = JavaCs.Substring(comment, 0, comment.IndexOf("*/"));
            else
                return spec;

            // search the parameters, eg /* params=?a?,?b? */
            string[] components = JavaCs.SplitByChar(comment.Trim(), '=');
            if (components.Length != 2)
                return spec;
            if (!"params".Equals(components[0].Trim()))
                return spec;
            string[] @params = JavaCs.SplitByChar(components[1].Trim(), ',');
            for (int i = 0; @params.Length > i; i++)
                spec.Add(@params[i].Trim());
            return spec;
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

        private string ReplaceSingleParameter(string sourceSql, string name, string value)
        {
            if (variant.IsOracle() && parameters.IsDate(name))
            {

                // Patch: GitLab #11 Excepcion ejecutando regla cuando un parametro de tipo fecha está
                // dentro de una funcion to_char (Oracle ORA-01722)
                // Localiza la expresion to_char (?XX? y si existe en vez de poner el parametro,
                // pone una coversion to_date
                // De esta forma la sql se encontrara con una fecha como argumento a to_char evitando el problema
                // Es un poco chapuza y dependiente de como se escribe la sql, pero como
                // proviene de una regla, siempre hay los espacios alrededor del parentesis
                string searchStr = "to_char(" + name;
                if (sourceSql.Contains(searchStr) || sourceSql.Contains(searchStr.ToUpper()))
                {
                    string dateFormat = GetDatabaseDialectFormat();
                    string replaceStr = "TO_CHAR(TO_DATE(" + name + ",'" + dateFormat + "')";
                    sourceSql = sourceSql.Replace(searchStr, replaceStr);
                    sourceSql = sourceSql.Replace(searchStr.ToUpper(), replaceStr);
                }
            }

            return sourceSql.Replace(name, value);
        }

        // necesario para el parche anterior, implementacion en subclase pues depende de la plataforma (solo para uso de p6spy)
        protected abstract string GetDatabaseDialectFormat();
        public abstract Connection GetConnection();
        /// <summary>
        /// Returns an object to browse the data accessed from the current connection
        /// </summary>
        public abstract IQueryStatementReader GetReader(string sql);
        /// <summary>
        /// Determines if the query is a select, needed to filter other statements
        /// when a generic jdbc execute() method is used (does not differentiates
        /// between query and update)
        /// </summary>
        public static bool IsSelectQuery(string sql, Logger log)
        {
            string sqlStart = JavaCs.Substring(sql.Trim(), 0, 6);
            if (JavaCs.EqualsIgnoreCase("select", sqlStart.ToLower()))
                return true;
            log.Debug("---- Ignored by qacover. clause is " + sqlStart);
            return false;
        }

        public virtual Exception GetException()
        {
            return exception;
        }

        public virtual string GetSql()
        {
            return sql == null ? "" : sql;
        }

        public virtual QueryParameters GetParameters()
        {
            return parameters;
        }

        public static void SetFaultInjector(FaultInjector injector)
        {
            faultInjector = injector;
        }

        public virtual FaultInjector GetFaultInjector()
        {
            return faultInjector;
        }
    }
}