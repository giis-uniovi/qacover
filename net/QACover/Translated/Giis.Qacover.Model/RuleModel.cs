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
    /// Representation of a coverage rule and the measures obtained after the evaluation:
    /// - dead: number of times the has been covered
    /// - count: number of times the rule has been evaluated
    /// - error: number of times the rule raised an error when evaluated
    /// - errorString: Contains all error messages (if any).
    /// 
    /// Wraps a TdRules model and stores the measures in its summary attribute. Provides getters and setters to
    /// manage these measures.
    /// </summary>
    public class RuleModel : RuleBase
    {
        // The model of the rules that is wrapped here
        protected TdRule model = null;
        public RuleModel(TdRule ruleModel)
        {
            model = ruleModel;
        }

        /// <summary>
        /// Returns the wrapped TdRule model
        /// </summary>
        public virtual TdRule GetModel()
        {
            return model;
        }

        protected override string GetAttribute(string name)
        {
            return ModelUtil.Safe(model.GetSummary(), name);
        }

        protected override void SetAttribute(string name, string value)
        {
            model.PutSummaryItem(name, value);
        }

        public virtual string GetSql()
        {
            return model.GetQuery();
        }

        public virtual void SetSql(string sql)
        {
            model.SetQuery(sql);
        }

        /// <summary>
        /// Returns a string representation of the main coverage measures of this rule
        /// </summary>
        public virtual string GetTextSummary()
        {
            return base.ToString();
        }

        // Other specific attributes of the wrapped model
        public virtual string GetId()
        {
            return model.GetId();
        }

        public virtual string GetCategory()
        {
            return model.GetCategory();
        }

        public virtual string GetMainType()
        {
            return model.GetMaintype();
        }

        public virtual string GetSubtype()
        {
            return model.GetSubtype();
        }

        public virtual string GetLocation()
        {
            return model.GetLocation();
        }

        public virtual string GetDescription()
        {
            return model.GetDescription();
        }

        public virtual void AddErrorString(string msg)
        {
            if (model.GetError().Contains(msg))
                return;
            model.SetError("".Equals(model.GetError()) ? msg : model.GetError() + "\n" + msg);
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