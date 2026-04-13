using Giis.Qacover.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////

namespace Giis.Qacover.Eval.Coverage
{
    /// <summary>
    /// Decides whether a rule is covered or not (different implementations for fpc or mutation)
    /// </summary>
    interface ICoverageDecisor
    {
        /// <summary>
        /// Executes the preliminary actions on the query before evaluating all rules (only needed for mutation).
        /// Returns the sql executed to prepare the mutation evaluation (empty for fpc)
        /// </summary>
        string PrepareEvaluation(AbstractQueryStatement stmt, QueryModel model);
        /// <summary>
        /// Gets the sql of a rule, with some preprocessing if required
        /// </summary>
        string GetRuleQuery(RuleModel model);
        /// <summary>
        /// Determines if the sql of a rule generated for a query statement is covered
        /// </summary>
        bool IsCovered(AbstractQueryStatement stmt, string sql);
    }
}