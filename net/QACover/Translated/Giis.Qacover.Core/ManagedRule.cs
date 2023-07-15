/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Giis.Qacover.Model;


namespace Giis.Qacover.Core
{
	/// <summary>Wraps a rule, providing actions needed by the CoverageManager</summary>
	public class ManagedRule
	{
		public const string RuntimeError = "RUNTIME_ERROR";

		public const string Uncovered = "UNCOVERED";

		public const string Covered = "COVERED";

		public const string AlreadyCovered = "ALREADY_COVERED";

		private RuleModel model;

		public ManagedRule(RuleModel model)
		{
			this.model = model;
		}

		public virtual RuleModel GetModel()
		{
			return this.model;
		}

		/// <summary>Determines the coverage of this rule, saving the results, returning if it is covered</summary>
		public virtual string Run(QueryStatement stmt)
		{
			string sqlWithoutValues = model.GetSql();
			model.AddCount(1);
			try
			{
				// save results
				bool isCovered = stmt.HasRows(sqlWithoutValues);
				model.AddDead(isCovered ? 1 : 0);
				return isCovered ? Covered : Uncovered;
			}
			catch (Exception e)
			{
				model.AddError(1);
				model.AddErrorString(e.ToString());
				model.SetRuntimeError(e.ToString());
				return RuntimeError;
			}
		}

		/// <summary>Gets the sql of the rule with the parameters replaced</summary>
		public virtual string GetSqlWithValues(QueryStatement stmt)
		{
			string sql = model.GetSql();
			return stmt.GetSqlWithValues(sql);
		}
	}
}
