using Giis.Portable.Util;
using Giis.Tdrules.Model.Shared;
using Giis.Tdrules.Openapi.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////

namespace Giis.Qacover.Model
{
    /// <summary>
    /// Representation of a query, its rules and the measures obtained after the evaluation:
    /// - dead: number of coverage rules that have been covered
    /// - count: number of coverage rules generated in the query
    /// - qrun: number of times the query has been evaluated.
    /// 
    /// Wraps a TdRules model and stores the measures in its summary attribute. Provides getters and setters to
    /// manage these measures.
    /// </summary>
    public class QueryModel : RuleBase
    {
        // How many times has been evaluated
        private static readonly string QRUN = "qrun";
        // Below additional attributes are used only when executor runs integrated with query interception,
        // they are not used when using the StandaloneEvaluator
        // Error message if the query itself has errors (it can't be evaluated)
        private static readonly string QERROR = "qerror";
        // Additional information about the query location in the source code
        private static readonly string CLASS_NAME = "class";
        private static readonly string METHOD_NAME = "method";
        private static readonly string LINE_NUMBER = "line";
        private static readonly string CLASS_FILE_NAME = "file";
        private static readonly string SOURCE_FILE_NAME = "source";
        // The model of the rules that is wrapped here
        protected TdRules model = null;
        protected override string GetAttribute(string name)
        {
            return ModelUtil.Safe(model.GetSummary(), name);
        }

        protected override void SetAttribute(string name, string value)
        {
            model.PutSummaryItem(name, value);
        }

        /// <summary>
        /// Creates an instance with the wrapped model
        /// </summary>
        public QueryModel(TdRules rulesModel)
        {
            model = rulesModel;
        }

        /// <summary>
        /// Returns the wrapped TdRules model
        /// </summary>
        public virtual TdRules GetModel()
        {
            return model;
        }

        /// <summary>
        /// Creates an instance used to signal an error in the query execution,
        /// containing the sql and error message only
        /// </summary>
        public QueryModel(string sql, string error)
        {
            model = new TdRules();
            model.SetRulesClass("sqlfpc");
            model.SetQuery(sql);
            model.SetError(error);
            this.SetQerror(1); // para contabilizar en totales al igual que la cobertura
        }

        public virtual string GetDbms()
        {
            return GetAttribute("dbms");
        }

        public virtual void SetDbms(string value)
        {
            this.SetAttribute("dbms", value);
        }

        /// <summary>
        /// Returns a string representation of the main coverage measures of this query and each of its rules (one per row)
        /// </summary>
        public virtual string GetTextSummary()
        {
            return GetTextSummary(false);
        }

        // By default do not include qrun, only for test, at least at this moment
        public virtual string GetTextSummary(bool includeQRun)
        {
            StringBuilder sb = new StringBuilder();

            // The summary may have an additional error message
            string strSummary = (this.GetQerror() > 0 ? "qerror=" + this.GetQerror() + "," : "") + base.ToString() + (includeQRun ? ",qrun=" + this.GetQrun() : "");
            sb.Append(strSummary);
            foreach (RuleModel rule in this.GetRules())
                sb.Append("\n").Append(rule.GetTextSummary());
            return sb.ToString();
        }

        public virtual void SetLocation(string className, string methodName, int lineNumber, string fileName, string sourceFileName)
        {
            SetAttribute(CLASS_NAME, className);
            SetAttribute(METHOD_NAME, methodName);
            SetAttribute(LINE_NUMBER, JavaCs.NumToString(lineNumber));
            SetAttribute(CLASS_FILE_NAME, fileName);
            SetAttribute(SOURCE_FILE_NAME, sourceFileName);
        }

        public virtual int GetQrun()
        {
            return GetIntAttribute(QRUN);
        }

        public virtual void AddQrun(int value)
        {
            IncrementIntAttribute(QRUN, value);
        }

        public virtual int GetQerror()
        {
            return GetIntAttribute(QERROR);
        }

        public virtual void SetQerror(int value)
        {
            SetAttribute(QERROR, JavaCs.NumToString(value));
        }

        public virtual string GetSql()
        {
            return model.GetQuery();
        }

        public virtual string GetSourceLocation()
        {
            return GetAttribute(SOURCE_FILE_NAME);
        }

        /// <summary>
        /// Returns all rules stored, wrapped as RuleModel objects
        /// </summary>
        public virtual IList<RuleModel> GetRules()
        {
            IList<RuleModel> rules = new List<RuleModel>();
            foreach (TdRule rule in ModelUtil.Safe(model.GetRules()))
                rules.Add(new RuleModel(rule));
            return rules;
        }

        public virtual string GetErrorString()
        {
            return model.GetError();
        }

        public override string ToString()
        {
            return model.ToString();
        }
    }
}