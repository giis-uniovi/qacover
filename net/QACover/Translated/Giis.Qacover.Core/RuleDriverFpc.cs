/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using Giis.Qacover.Core.Services;
using Giis.Qacover.Model;


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
			return svc.GetRulesModel(sql, schema, fpcOptions);
		}

		protected internal override bool IsCovered(QueryStatement stmt, string sql)
		{
			return stmt.HasRows(sql);
		}
	}
}
