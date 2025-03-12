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
    /// Delegate that performs the required actions
    /// on the SQLfpc coverage rules (get and evaluate the rules)
    /// </summary>
    public class RuleDriverFpc : RuleDriver
    {
        public override QueryModel GetRules(RuleServices svc, string sql, SchemaModel schema, string fpcOptions)
        {
            return svc.GetFpcRulesModel(sql, schema, fpcOptions);
        }

        public override void PrepareEvaluation(QueryStatement stmt, QueryModel model, string orderCols)
        {
        }

        protected override bool IsCovered(QueryStatement stmt, string sql)
        {
            return stmt.GetReader(sql).HasRows();
        }
    }
}