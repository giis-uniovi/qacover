/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using Giis.Qacover.Core.Services;
using Giis.Qacover.Model;


namespace Giis.Qacover.Core
{
	/// <summary>
	/// Delegate that performs the required actions
	/// on the SQLMutation rules (get and evaluate the rules)
	/// </summary>
	public class RuleDriverMutation : RuleDriver
	{
		private IList<string[]> rows;

		public override QueryModel GetRules(RuleServices svc, string sql, SchemaModel schema, string fpcOptions)
		{
			return svc.GetMutationRulesModel(sql, schema, fpcOptions);
		}

		public override void PrepareEvaluation(QueryStatement stmt, QueryModel model, string orderCols)
		{
			string sql = model.GetModel().GetParsedquery();
			sql = AddOrderBy(sql, orderCols);
			this.rows = stmt.GetReader(sql).GetRows();
		}

		protected internal override bool IsCovered(QueryStatement stmt, string sql)
		{
			return !stmt.GetReader(sql).EqualRows(this.rows);
		}
	}
}
