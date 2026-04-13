using Giis.Qacover.Core.Query;
using Giis.Qacover.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////

namespace Giis.Qacover.Core.Coverage
{
    /// <summary>
    /// Performs the required actions on the SQLfpc coverage rules (get and evaluate the rules)
    /// </summary>
    class CoverageFpc : ICoverageDecisor
    {
        public virtual string PrepareEvaluation(AbstractQueryStatement stmt, QueryModel model)
        {

            // no preparation actions, evaluation is made by checking number of rows for each rule
            return "";
        }

        public virtual string GetRuleQuery(RuleModel model)
        {
            return model.GetSql(); // no preprocessing for fpc
        }

        public virtual bool IsCovered(AbstractQueryStatement stmt, string sql)
        {
            return stmt.GetReader(sql).HasRows();
        }
    }
}