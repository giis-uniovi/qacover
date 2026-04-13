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
    /// Generate the model with the rules according to the coverage criterion specified in the configuration
    /// </summary>
    public class RuleGenerator
    {
        public virtual QueryModel GetRules(RuleServices svc, string sql, SchemaModel schema, string fpcOptions)
        {
            if ("mutation".Equals(Configuration.GetInstance().GetRuleCriterion()))
                return svc.GetMutationRulesModel(sql, schema, fpcOptions);
            else

                // fpc by default
                return svc.GetFpcRulesModel(sql, schema, fpcOptions);
        }
    }
}