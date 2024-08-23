/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Giis.Qacover.Core.Services;
using Giis.Qacover.Model;


namespace Giis.Qacover.Core
{
	/// <summary>
	/// Base class of the delegates that perform the required actions
	/// on the coverage rules (get and evaluate the rules).
	/// </summary>
	/// <remarks>
	/// Base class of the delegates that perform the required actions
	/// on the coverage rules (get and evaluate the rules).
	/// Subclasses will implement the specific actions that depend
	/// on the coverage criterion (fpc or mutation).
	/// </remarks>
	public abstract class RuleDriver
	{
		/// <summary>Generate the model with the rules</summary>
		public abstract QueryModel GetRules(RuleServices svc, string sql, SchemaModel schema, string fpcOptions);

		/// <summary>
		/// Executes the preliminary actions on the query before evaluating each rule
		/// (only needed for mutation)
		/// </summary>
		public abstract void PrepareEvaluation(QueryStatement stmt, QueryModel model, string orderCols);

		/// <summary>Determines the coverage of a rule, saving the results and returning if it is covered</summary>
		public virtual string EvaluateRule(RuleModel model, QueryStatement stmt, string orderCols)
		{
			string sql = model.GetSql();
			sql = AddOrderBy(sql, orderCols);
			// only needed for mutation
			model.AddCount(1);
			try
			{
				// save results
				bool isCovered = IsCovered(stmt, sql);
				model.AddDead(isCovered ? 1 : 0);
				return isCovered ? ResultVector.Covered : ResultVector.Uncovered;
			}
			catch (Exception e)
			{
				model.AddError(1);
				model.AddErrorString(e.ToString());
				model.SetRuntimeError(e.ToString());
				return ResultVector.RuntimeError;
			}
		}

		/// <summary>Determines if the sql of a rule generated for a query statement is covered</summary>
		protected internal abstract bool IsCovered(QueryStatement stmt, string sql);

		/// <summary>
		/// Gets the columns to order the queries from the model (only for mutations),
		/// returns emtpy string if no found
		/// </summary>
		public virtual string GetOrderCols(QueryModel model)
		{
			string orderCols = string.Empty;
			// checks with containsKey for net compatibility (that fails if key does not exists)
			if (model.GetModel().GetSummary() != null && model.GetModel().GetSummary().ContainsKey("ordercols"))
			{
				orderCols = model.GetModel().GetSummary()["ordercols"];
			}
			return orderCols;
		}

		/// <summary>
		/// Transforms the sql query to add an order by that includes the orderCols,
		/// if not empty
		/// </summary>
		protected internal virtual string AddOrderBy(string sql, string orderCols)
		{
			if (!string.Empty.Equals(orderCols))
			{
				sql += "\nORDER BY " + orderCols;
			}
			return sql;
		}

		internal virtual string GetLogString(RuleModel ruleModel, QueryStatement stmt, string res)
		{
			string sql = stmt.GetSqlWithValues(ruleModel.GetSql());
			string ruleWithSql = sql.Replace("\r", string.Empty).Replace("\n", " ").Trim();
			string logString = res + " " + (ResultVector.Covered.Equals(res) ? "  " : string.Empty) + ruleWithSql;
			logString += ResultVector.RuntimeError.Equals(res) ? "\n" + ruleModel.GetRuntimeError() : string.Empty;
			return logString;
		}
	}
}