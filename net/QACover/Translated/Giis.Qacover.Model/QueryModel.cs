/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using System.Text;
using Giis.Tdrules.Model;
using Giis.Tdrules.Openapi.Model;


namespace Giis.Qacover.Model
{
	/// <summary>Representation of a query, its rules and the results/indicators about the evaluation.</summary>
	/// <remarks>
	/// Representation of a query, its rules and the results/indicators about the evaluation.
	/// It is a wrapper of the TdRules model with additional information about the evaluation
	/// </remarks>
	public class QueryModel : RuleBase
	{
		private const string Qerror = "qerror";

		private const string Qrun = "qrun";

		private const string ClassName = "class";

		private const string MethodName = "method";

		private const string LineNumber = "line";

		private const string ClassFileName = "file";

		protected internal SqlRules model = null;

		// In addition to the standard attributes, indicates if the query itself 
		// has errors (it can't be evaluated)
		// How many times has been evaluated
		// It stores additional information about the query location
		// The model of the rules that is wrapped here
		protected internal override string GetAttribute(string name)
		{
			return ModelUtil.Safe(model.GetSummary(), name);
		}

		protected internal override void SetAttribute(string name, string value)
		{
			model.PutSummaryItem(name, value);
		}

		/// <summary>Creates an instance with the wrapped model</summary>
		public QueryModel(SqlRules rulesModel)
		{
			model = rulesModel;
		}

		public virtual SqlRules GetModel()
		{
			return model;
		}

		/// <summary>
		/// Creates an instance used to signal an error in the query execution,
		/// containing the sql and error message only
		/// </summary>
		public QueryModel(string sql, string error)
		{
			model = new SqlRules();
			model.SetRulesClass("sqlfpc");
			model.SetSql(sql);
			model.SetError(error);
			this.SetQerror(1);
		}

		// para contabilizar en totales al igual que la cobertura
		public virtual string GetDbms()
		{
			return GetAttribute("dbms");
		}

		public virtual void SetDbms(string value)
		{
			this.SetAttribute("dbms", value);
		}

		public virtual string GetTextSummary()
		{
			StringBuilder sb = new StringBuilder();
			// The summary may have an additional error message
			string strSummary = (this.GetQerror() > 0 ? "qerror=" + this.GetQerror() + "," : string.Empty) + base.ToString();
			sb.Append(strSummary);
			foreach (RuleModel rule in this.GetRules())
			{
				sb.Append("\n").Append(rule.GetTextSummary());
			}
			return sb.ToString();
		}

		public virtual void SetLocation(string className, string methodName, int lineNumber, string fileName)
		{
			SetAttribute(ClassName, className);
			SetAttribute(MethodName, methodName);
			SetAttribute(LineNumber, lineNumber.ToString());
			SetAttribute(ClassFileName, fileName);
		}

		public virtual int GetQrun()
		{
			return GetIntAttribute(Qrun);
		}

		public virtual void AddQrun(int value)
		{
			IncrementIntAttribute(Qrun, value);
		}

		public virtual int GetQerror()
		{
			return GetIntAttribute(Qerror);
		}

		public virtual void SetQerror(int value)
		{
			SetAttribute(Qerror, value.ToString());
		}

		public virtual string GetSql()
		{
			return model.GetSql();
		}

		public virtual IList<RuleModel> GetRules()
		{
			IList<RuleModel> rules = new List<RuleModel>();
			foreach (SqlRule rule in ModelUtil.Safe(model.GetRules()))
			{
				rules.Add(new RuleModel(rule));
			}
			return rules;
		}

		public virtual string GetErrorString()
		{
			return model.GetError();
		}

		public override string ToString()
		{
			return model.ToString();
		}
	}
}
