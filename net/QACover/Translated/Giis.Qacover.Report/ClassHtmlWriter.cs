/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using Giis.Qacover.Model;
using Giis.Qacover.Reader;


namespace Giis.Qacover.Report
{
	public class ClassHtmlWriter : BaseHtmlWriter
	{
		protected internal override string GetScripts()
		{
			return "<script>\n" + "    $(document).ready(function() {\n" + "        $('.rules').show();\n" + "        $('.rules-show').show();\n" + "        $('.rules-hide').hide();\n" + "        $('.params-show').show();\n" + "        $('.params-hide').hide();\n" + "        $('.rules').hide();\n"
				 + "        $('.params').hide();\n" + "        $('.query br.canhide').hide(); \n" + "        $('.rules br.canhide').show(); \n" + "        if ($('#view-source').is(':checked')) { $('.method').hide(); } else { $('.method').show(); }\n" + "        $('.rules-show').click(function(){ $(this).hide(); $(this).next().show(); $(this).closest('.query').next('.rules').show(); });\n"
				 + "        $('.rules-hide').click(function(){ $(this).hide(); $(this).prev().show(); $(this).closest('.query').next('.rules').hide(); });\n" + "        $('.params-show').click(function(){ $(this).hide(); $(this).next().show(); $(this).closest('.query').find('.params').show(); $(this).closest('.query').next('.rules').find('.params').show(); });\n"
				 + "        $('.params-hide').click(function(){ $(this).hide(); $(this).prev().show(); $(this).closest('.query').find('.params').hide(); $(this).closest('.query').next('.rules').find('.params').hide(); });\n" + "        $('#format-queries').change(function(){ if ($(this).is(':checked')) $('.query br.canhide').show(); else $('.query br.canhide').hide(); });\n"
				 + "        $('#format-rules').change(function(){ if ($(this).is(':checked')) $('.rules br.canhide').show(); else $('.rules br.canhide').hide(); });\n" + "        $('#view-source').change(function() { \n" + "            if ($(this).is(':checked')) { $('.line-code').show(); $('code').show(); $('.method').hide(); } \n"
				 + "            else { $('.line-code').hide(); $('code').hide(); $('.method').show(); }\n" + "        }\n" + "    )});\n" + "</script>\n";
		}

		protected internal override string GetStyles()
		{
			return "<style>\n" + "    .fill { min-width: 100%; width: 100%; } \n" + "    tr.line, tr.line td { line-height:18px; padding-top:0; padding-bottom:0 }\n" + "    tr.query-run td, tr.rule-summary td, tr.rule-sql td, tr.rule-error td { padding-top:0; padding-bottom:0 }" + "    code { color: DimGray; position: absolute; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }\n"
				 + "    tbody.query { background: lightgrey; }\n" + "    td.nowrap, span.nowrap { white-space: nowrap; }\n" + "    td.covered { background:aquamarine; }\n" + "    td.uncovered { background:lightyellow; }\n" + "    .params  { font-size: small; font-style: italic }\n" + "    .rules-show, .rules-hide, .params-show, .params-hide { cursor: pointer; }\n"
				 + "</style>\n";
		}

		public virtual string GetBodyContent(string title, string content)
		{
			return "<body style='overflow-x: hidden;'>\n" + "<div class='container fill'>\n" + "<div class='sticky-md-top text-bg-light'>\n" + "  <h4><a href='./index.html'>[Index]</a> " + title + "</h4>\n" + "  <div class='col-auto'>\n" + "    <div  class='form-check form-check-inline'>\n" +
				 "    <input class='form-check-input' id='view-source' type='checkbox' value='' checked>\n" + "    <label class='form-check-label' for='view-source'>View source</label>\n" + "    </div>\n" + "    <div  class='form-check form-check-inline'>\n" + "    <input class='form-check-input' id='format-queries' type='checkbox' value=''>\n"
				 + "    <label class='form-check-label' for='format-queries'>Format queries</label>\n" + "    </div>\n" + "    <div  class='form-check form-check-inline'>\n" + "    <input class='form-check-input' id='format-rules' type='checkbox' value='' checked>\n" + "    <label class='form-check-label' for='format-rules'>Format rules</label>\n"
				 + "    </div>\n" + "  </div>\n" + "</div>\n" + "\n" + "<div>\n" + "<table class='table table-sm table-borderless'>\n" + "    <thead><th style='width:1%'>Line</th><th style='width:1%'>Coverage</th><th>Source code/method, queries and rules</th></thead>\n" + content + "\n" + "</table>\n"
				 + "</div>\n" + GetFooter() + "</div>\n" + "</body>\n";
		}

		public virtual string GetLineWithoutCoverage(int lineNumber, string sourceCode)
		{
			string template = "<tr class='line line-code'><td>$lineNumber$</td><td></td>" + "<td colspan='2'><code>$sourceCode$</code></td>" + "</tr>\n";
			return template.Replace("$lineNumber$", lineNumber.ToString()).Replace("$sourceCode$", GetSourceHtml(sourceCode));
		}

		public virtual string GetLineContent(int lineNumber, string coverage, string methodName, string sourceCode)
		{
			string template = "    <tr class='line line-coverage'>\n" + "        <td>$lineNumber$</td>\n" + "        <td class='nowrap'>$coverage$</td>\n" + "        <td colspan='2'>\n" + "            <span class='text-primary font-weight-bold $methodCssClass$'>$methodName$</span>\n" + "            <code>$sourceCode$</code>\n"
				 + "        </td>\n" + "    </tr>\n";
			return template.Replace("$lineNumber$", lineNumber.ToString()).Replace("$coverage$", coverage).Replace("$methodName$", methodName).Replace("$methodCssClass$", sourceCode == null ? string.Empty : "method").Replace("$sourceCode$", sourceCode == null ? " &nbsp; (source code not available)"
				 : GetSourceHtml(sourceCode));
		}

		// if source code not available, the css class for method is empty (it will be shown even if hidden by the ui)
		public virtual string GetQueryContent(QueryReader query, string coverage)
		{
			string template = "    <tbody class='query'>\n" + "    <tr class='query-run'>\n" + "        <td></td>\n" + "        <td class='nowrap'>\n" + "            <span class='rules-show' title='Show rules'>&#9660;</span><span class='rules-hide' title='Hide rules'>&#9650;</span>\n" + "            $runCount$ run(s)\n"
				 + "            <span class='params-show' title='Show run params'>&#9655;</span><span class='params-hide' title='Hide run paramss'>&#9665;</span> " + "            $coverage$\n" + "        </td>\n" + "        <td colspan='2'>\n" + "            <div class='params'>(run params not available)</div>\n"
				 + "            $sqlQuery$ $errorsQuery$" + "        </td>\n" + "    </tr>\n" + "    </tbody>\n";
			return template.Replace("$runCount$", query.GetModel().GetQrun().ToString()).Replace("$coverage$", coverage).Replace("$sqlQuery$", GetSqlHtml(Encode(query.GetSql()))).Replace("$errorsQuery$", GetErrorsHtml(Encode(query.GetModel().GetErrorString().Replace("\n", string.Empty).Replace
				("\r", string.Empty)), query.GetModel().GetError()));
		}

		// encode and remove line endings (to allow use a regex to replace platform dependent messages)
		public virtual string GetRulesContent(string rulesContent)
		{
			return "    <tbody class='rules'>\n" + rulesContent + "    <tr><td></td></tr>\n" + "    </tbody>\n\n";
		}

		// just to give a little separation after rules
		public virtual string GetRuleContent(RuleModel rule)
		{
			string template = "    <tr class='rule-summary'>\n" + "        <td></td>\n" + "        <td class='coverage' rowspan='2'><strong>$ruleId$</strong> - dead:&nbsp;$ruleDead$ count:&nbsp;$ruleCount$</td>\n" + "        <td class='coverage' colspan='2'>\n" + "            category: $ruleCategory$ type: $ruleType$ subtype: $ruleSubtype$ location: $ruleLocation$\n"
				 + "            <div class='params'>(run params not available)</div>\n" + "        </td>\n" + "    </tr>\n" + "    <tr class='rule-sql'>\n" + "        <td></td>\n" + "            <td class='$ruleStatus$'>$ruleDescription$</td>\n" + "            <td class='$ruleStatus$'>$ruleSql$</td>\n"
				 + "    </tr>\n" + "    $ruleErrors$";
			// needs a complete row to allow error message span across description and sql columns
			return template.Replace("$ruleId$", rule.GetId()).Replace("$ruleDead$", rule.GetDead().ToString()).Replace("$ruleCount$", rule.GetCount().ToString()).Replace("$ruleCategory$", rule.GetCategory()).Replace("$ruleType$", rule.GetMainType()).Replace("$ruleSubtype$", rule.GetSubtype())
				.Replace("$ruleLocation$", Encode(rule.GetLocation())).Replace("$ruleStatus$", rule.GetDead() > 0 ? "covered" : "uncovered").Replace("$ruleDescription$", GetDescriptionHtml(Encode(rule.GetDescription()))).Replace("$ruleSql$", GetSqlHtml(Encode(rule.GetSql()))).Replace("$ruleErrors$"
				, GetErrorsHtml(Encode(rule.GetErrorString())));
		}

		private string GetErrorsHtml(string queryErrors, int ruleErrorCount)
		{
			if (!string.Empty.Equals(queryErrors))
			{
				// error in query execution
				return "\n<br/><span style='color:red;'>Query error: " + queryErrors + "</span>\n";
			}
			if (ruleErrorCount > 0)
			{
				// error in any rule(s)
				return "\n<br/><span style='color:red;'>" + ruleErrorCount + " rule(s) with error</span>\n";
			}
			return string.Empty;
		}

		// no error
		private string GetErrorsHtml(string errors)
		{
			if (!string.Empty.Equals(errors))
			{
				return "<tr class='rule-error'><td colspan='2'></td><td colspan='2'><span style=\"color:red;\">" + "Rule error: " + errors.Replace("\n", "\n<br/>") + "</span></td></tr>\n";
			}
			return string.Empty;
		}

		// no error
		private string GetSourceHtml(string sourceCode)
		{
			return Encode(sourceCode).Replace("\t", "    ").Replace(" ", "&nbsp;");
		}

		private string GetSqlHtml(string sql)
		{
			string br = " <br class='canhide'/>";
			sql = sql.Replace("SELECT ", "<strong>SELECT</strong> ").Replace("FROM ", br + "<strong>FROM</strong> ").Replace("LEFT JOIN ", br + "<strong>LEFT JOIN</strong> ").Replace("RIGHT JOIN ", br + "<strong>RIGHT JOIN</strong> ").Replace("INNER JOIN ", br + "<strong>INNER JOIN</strong> "
				).Replace("WHERE ", br + "<strong>WHERE</strong> ").Replace("GROUP BY ", br + "<strong>GROUP BY</strong> ").Replace("HAVING ", br + "<strong>HAVING</strong> ").Replace("ORDER BY ", br + "<strong>ORDER BY</strong> ").Replace("UNION ", br + "<strong>UNION</strong> ");
			return sql;
		}

		private string GetDescriptionHtml(string desc)
		{
			return desc.Replace("\n", " <br class='canhide'/>");
		}
	}
}
