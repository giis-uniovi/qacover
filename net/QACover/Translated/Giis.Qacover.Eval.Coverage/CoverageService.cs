using NLog;
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
    /// Evaluates the coverage rules that are contained in a model, the results are stored in the model.
    /// 
    /// Decision of the coverage result is implemented in a delegate (different for fpc or mutation).
    /// </summary>
    public class CoverageService
    {
        private static readonly Logger log = Giis.Portable.Util.NLogUtil.GetLogger(typeof(CoverageService));
        private QueryModel model;
        private ICoverageDecisor decisor;
        private bool debugSql = false;
        private string lastRuleError; // Error message after evaluation of a single rule (used in logs)
        public CoverageService(QueryModel model)
        {
            this.model = model;
            if ("mutation".Equals(model.GetModel().GetRulesClass()))
                this.decisor = new CoverageMutation();
            else
                this.decisor = new CoverageFpc(); // fpc by default
        }

        public virtual CoverageService SetDebugSql(bool value)
        {
            this.debugSql = value;
            return this;
        }

        /// <summary>
        /// Executes ead rule that have been generated in the model
        /// </summary>
        public virtual ResultVector EvaluateQuery(AbstractQueryStatement stmt, bool skipIfCovered)
        {
            IList<RuleModel> rules = model.GetRules();
            int totalDead = 0;
            int totalError = 0;
            ResultVector result = new ResultVector(rules.Count);
            stmt.SetVariant(new Variability(model.GetDbms()));

            // Mutation will require a previous step to read query results to compare to the output of each mutant
            string prepareSql = decisor.PrepareEvaluation(stmt, model);
            if (!"".Equals(prepareSql) && this.debugSql)
                log.Debug("Query: " + prepareSql);

            // Executes and annotates the coverage/status of each rule
            for (int i = 0; i < rules.Count; i++)
            {
                RuleModel ruleModel = rules[i];
                string res;
                if (skipIfCovered && ruleModel.GetDead() > 0)
                    res = ResultVector.ALREADY_COVERED;
                else
                    res = this.EvaluateRule(ruleModel, stmt);

                // Results for logging/debugging
                string logString = this.GetLogString(ruleModel, stmt, res, this.lastRuleError);
                log.Debug(logString);
                result.SetResult(i, res, logString);

                //  Calculate totals
                if (ruleModel.GetDead() > 0)
                    totalDead++;
                if (ruleModel.GetError() > 0)
                    totalError++;
            }

            model.SetCount(rules.Count);
            model.SetDead(totalDead);
            model.SetError(totalError);
            model.AddQrun(1);
            log.Info(" SUMMARY: Covered " + model.GetDead() + " out of " + model.GetCount());
            return result;
        }

        /// <summary>
        /// Determines the coverage of a rule, saving the results and returning if it is covered
        /// </summary>
        public virtual string EvaluateRule(RuleModel model, AbstractQueryStatement stmt)
        {
            string sql = this.decisor.GetRuleQuery(model);
            model.AddCount(1);
            this.lastRuleError = "";
            try
            {

                // save results
                bool isCovered = decisor.IsCovered(stmt, sql);
                if (this.debugSql)
                    log.Debug("Rule: " + sql);
                model.AddDead(isCovered ? 1 : 0);
                return isCovered ? ResultVector.COVERED : ResultVector.UNCOVERED;
            }
            catch (Exception e)
            {
                model.AddError(1);
                model.AddErrorString(e.ToString()); // cumulative error messages
                this.lastRuleError = e.ToString(); // current error message
                return ResultVector.RUNTIME_ERROR;
            }
        }

        private string GetLogString(RuleModel ruleModel, AbstractQueryStatement stmt, string res, string lastRuleError)
        {
            string sql = stmt.GetSqlWithValues(ruleModel.GetSql());
            string ruleWithSql = sql.Replace("\r", "").Replace("\n", " ").Trim();
            string logString = res + " " + (ResultVector.COVERED.Equals(res) ? "  " : "") + ruleWithSql;
            logString += "".Equals(lastRuleError) ? "" : "\n" + lastRuleError;
            return logString;
        }
    }
}