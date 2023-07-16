/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using System.Text;
using Giis.Portable.Xml.Tiny;
using Giis.Qacover.Model;


namespace Giis.Qacover.Report
{
	public class ClassHtmlWriter
	{
		private const string HtmlNewline = "<br/>";

		public virtual string GetHeader(string className)
		{
			return SharedHtmlWriter.GetHtmlBegin(className, true) + "\n<h4>" + className + "</h4>" + "\n<div class=\"panel-group\" id=\"accordion\">";
		}

		public virtual string GetQueryBody(QueryModel rules, string methodIdentifier, string queryId)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(GetRuleHeader(rules, methodIdentifier, queryId));
			foreach (RuleModel rule in rules.GetRules())
			{
				sb.Append(GetRuleBody(rule));
			}
			sb.Append(GetRuleFooter());
			return sb.ToString();
		}

		public virtual string GetFooter()
		{
			return "\n</div>" + SharedHtmlWriter.GetHtmlEnd();
		}

		// Details of each rule
		private string GetRuleHeader(QueryModel rules, string methodIdentifier, string queryId)
		{
			string sql = rules.GetSql();
			return "\n\n<div class=\"panel panel-default\">" + "\n<div class=\"panel-heading\">" + "\n<table class=\"table table-striped table-sm\">" + "\n<tr>" + "\n<td><a data-toggle=\"collapse\" data-parent=\"#accordion\" href=\"#" + queryId + "\">" + methodIdentifier + "</a>" + "\n<br/><strong>"
				 + SharedHtmlWriter.Percent(rules.GetCount(), rules.GetDead()) + "</strong> (" + rules.GetDead() + "/" + rules.GetCount() + ")" + " " + rules.GetQrun() + " run(s)" + (rules.GetError() > 0 ? " <span style=\"color:red;\">" + rules.GetError() + " rule(s) with error</span>" : string.Empty
				) + "\n</td>" + "\n<td valign=\"top\">" + GetSqlToHtml(XNodeAbstract.EncodeText(sql)) + GetErrorsHtml(rules) + "\n</td>" + "\n</tr>" + "\n</table>" + "\n</div>" + "\n<div class=\"panel-collapse collapse\" id=\"" + queryId + "\">" + "\n<table class=\"table table-sm\">" + "\n<tr><th>ID</th><th>dead</th><th>count</th><th>category</th><th>type</th><th>subtype</th><th>location</th></tr>";
		}

		public virtual string GetRuleBody(RuleModel rule)
		{
			bool covered = rule.GetDead() != 0;
			string template = "\n<tr><td><strong>$id</strong></td><td>$dead</td><td>$count</td>" + "\n<td>$category</td><td>$type</td><td>$subtype</td><td>$location</td></tr>" + "\n<td colspan=\"7\" $style >" + "<div class=\"row\">" + "<div class=\"col-5\">$description</div>" + "<div class=\"col-7\">$sql$error</div>"
				 + "</div>" + "</td></tr>";
			return template.Replace("$id", rule.GetId()).Replace("$count", rule.GetCount().ToString()).Replace("$dead", rule.GetDead().ToString()).Replace("$category", rule.GetCategory()).Replace("$subtype", rule.GetSubtype()).Replace("$type", rule.GetMainType()).Replace("$location", rule.GetLocation
				()).Replace("$description", XNodeAbstract.EncodeText(rule.GetDescription()).Replace("\n", HtmlNewline)).Replace("$sql", GetSqlToHtml(rule.GetSql())).Replace("$error", GetErrorsHtml(rule)).Replace("$style", covered ? Bgcolor("palegreen") : Bgcolor("lightyellow"));
		}

		private string GetErrorsHtml(QueryModel rule)
		{
			string errors = rule.GetErrorString().Replace("\n", HtmlNewline);
			if (!string.Empty.Equals(errors))
			{
				return "<br/><span style=\"color:red;\">" + errors + "</span>";
			}
			return string.Empty;
		}

		//no error
		private string GetErrorsHtml(RuleModel rule)
		{
			string errors = rule.GetErrorString().Replace("\n", HtmlNewline);
			if (!string.Empty.Equals(errors))
			{
				return "<br/><span style=\"color:red;\">" + errors + "</span>";
			}
			return string.Empty;
		}

		//no error
		private string GetRuleFooter()
		{
			return "\n</table></div></div>";
		}

		//
		private string Bgcolor(string color)
		{
			return " style=\"background:" + color + ";\"";
		}

		/// <summary>A simple formating of SQL to display in html</summary>
		private string GetSqlToHtml(string sql)
		{
			sql = sql.Replace("SELECT ", "<strong>SELECT</strong> ");
			sql = sql.Replace("FROM ", "<br/><strong>FROM</strong> ");
			sql = sql.Replace("LEFT JOIN ", "<br/><strong>LEFT JOIN</strong> ");
			sql = sql.Replace("RIGHT JOIN ", "<br/><strong>RIGHT JOIN</strong> ");
			sql = sql.Replace("INNER JOIN ", "<br/><strong>INNER JOIN</strong> ");
			sql = sql.Replace("WHERE ", "<br/><strong>WHERE</strong> ");
			sql = sql.Replace("GROUP BY ", "<br/><strong>GROUP BY</strong> ");
			sql = sql.Replace("HAVING ", "<br/><strong>HAVING</strong> ");
			sql = sql.Replace("ORDER BY ", "<br/><strong>ORDER BY</strong> ");
			return sql;
		}
	}
}
