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
    /// Performs the required actions on the SQLMutation rules (get and evaluate the rules)
    /// </summary>
    class CoverageMutation : ICoverageDecisor
    {
        private IList<String[]> rows;
        private string orderCols = "";
        public virtual void PrepareEvaluation(AbstractQueryStatement stmt, QueryModel model)
        {

            // Mutation needs store the query result and order by the columns in the query to get repeatable comparations
            // Requires the model have a parsed query ???
            orderCols = GetOrderCols(model);
            string sql = model.GetModel().GetParsedquery();
            sql = AddOrderBy(sql, orderCols);
            this.rows = stmt.GetReader(sql).GetRows();
        }

        public virtual string GetRuleQuery(RuleModel model)
        {

            // rule must have the same order than the query
            string sql = model.GetSql();
            return AddOrderBy(sql, this.orderCols);
        }

        public virtual bool IsCovered(AbstractQueryStatement stmt, string sql)
        {
            return !stmt.GetReader(sql).EqualRows(this.rows);
        }

        /// <summary>
        /// Gets the column numbers to order the queries from the model, returns empty string if no found
        /// </summary>
        private string GetOrderCols(QueryModel model)
        {
            string order = "";

            // checks with containsKey for net compatibility (that fails if key does not exists)
            if (model.GetModel().GetSummary() != null && model.GetModel().GetSummary().ContainsKey("ordercols"))
                order = model.GetModel().GetSummary()["ordercols"];
            return order;
        }

        /// <summary>
        /// Transforms the sql query to add an order by that includes the orderCols, if not empty
        /// </summary>
        private string AddOrderBy(string sql, string orderCols)
        {
            if (!"".Equals(orderCols))
                sql += "\nORDER BY " + orderCols;
            return sql;
        }
    }
}