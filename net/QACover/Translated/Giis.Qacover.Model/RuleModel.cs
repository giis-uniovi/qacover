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
    /// Representation of single rule and the results/indicators about the
    /// evaluation. It is a wrapper of the TdRules rule model with additional
    /// information about the evaluation
    /// </summary>
    public class RuleModel : RuleBase
    {
        protected string runtimeError = "";
        // The model of the rules that is wrapped here
        protected TdRule model = null;
        public RuleModel(TdRule ruleModel)
        {
            model = ruleModel;
        }

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

        public virtual string GetTextSummary()
        {
            return base.ToString();
        }

        public virtual string GetRuntimeError()
        {
            return this.runtimeError;
        }

        public virtual void SetRuntimeError(string error)
        {
            this.runtimeError = error;
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