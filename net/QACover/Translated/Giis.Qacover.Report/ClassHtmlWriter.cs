using Giis.Portable.Util;
using Giis.Qacover.Model;
using Giis.Qacover.Reader;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////

namespace Giis.Qacover.Report
{
    public class ClassHtmlWriter : BaseHtmlWriter
    {
        public override string GetScripts()
        {
            return "<script>\n" + "    $(document).ready(function() {\n" + "        $('.rules').show();\n" + "        $('.rules-show').show();\n" + "        $('.rules-hide').hide();\n" + "        $('.params-show').show();\n" + "        $('.params-hide').hide();\n" + "        $('.rules').hide();\n" + "        $('.params').hide();\n" + "        $('.query br.canhide').hide(); \n" + "        $('.rules br.canhide').show(); \n" + "        if ($('#view-source').is(':checked')) { $('.method').hide(); } else { $('.method').show(); }\n" + "        $('.rules-show').click(function(){ $(this).hide(); $(this).next().show(); $(this).closest('.query').next('.rules').show(); });\n" + "        $('.rules-hide').click(function(){ $(this).hide(); $(this).prev().show(); $(this).closest('.query').next('.rules').hide(); });\n" + "        $('.params-show').click(function(){ $(this).hide(); $(this).next().show(); $(this).closest('.query').find('.params').show(); $(this).closest('.query').next('.rules').find('.params').show(); });\n" + "        $('.params-hide').click(function(){ $(this).hide(); $(this).prev().show(); $(this).closest('.query').find('.params').hide(); $(this).closest('.query').next('.rules').find('.params').hide(); });\n" + "        $('#format-queries').change(function(){ if ($(this).is(':checked')) $('.query br.canhide').show(); else $('.query br.canhide').hide(); });\n" + "        $('#format-rules').change(function(){ if ($(this).is(':checked')) $('.rules br.canhide').show(); else $('.rules br.canhide').hide(); });\n" + "        $('#view-source').change(function() { \n" + "            if ($(this).is(':checked')) { $('.line-code').show(); $('code').show(); $('.method').hide(); } \n" + "            else { $('.line-code').hide(); $('code').hide(); $('.method').show(); }\n" + "        }\n" + "    )});\n" + "</script>\n";
        }

        public override string GetStyles()
        {
            return "<style>\n" + "    .fill { min-width: 100%; width: 100%; } \n" + "    tr.line, tr.line td { line-height:18px; padding-top:0; padding-bottom:0 }\n" + "    tr.query-run td, tr.rule-summary td, tr.rule-sql td, tr.rule-error td { padding-top:0; padding-bottom:0 }" + "    code { color: DimGray; position: absolute; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }\n" + "    tbody.query { background: lightgrey; }\n" + "    td.nowrap, span.nowrap { white-space: nowrap; }\n" + "    td.covered { background:aquamarine; }\n" + "    td.uncovered { background:lightyellow; }\n" + "    .params  { font-size: small; }\n" + "    .result-vector  { font-family: 'Lucida Console', 'Courier New', monospace; }\n" + "    .rules-show, .rules-hide, .params-show, .params-hide { cursor: pointer; }\n" + "</style>\n";
        }

        public virtual string GetBodyContent(string title, string content)
        {
            return "<body style='overflow-x: hidden;'>\n" + "<div class='container fill'>\n" + "<div class='sticky-md-top text-bg-light'>\n" + "  <h4><a href='./index.html'>[Index]</a> " + title + "</h4>\n" + "  <div class='col-auto'>\n" + "    <div  class='form-check form-check-inline'>\n" + "    <input class='form-check-input' id='view-source' type='checkbox' value='' checked>\n" + "    <label class='form-check-label' for='view-source'>View source</label>\n" + "    </div>\n" + "    <div  class='form-check form-check-inline'>\n" + "    <input class='form-check-input' id='format-queries' type='checkbox' value=''>\n" + "    <label class='form-check-label' for='format-queries'>Format queries</label>\n" + "    </div>\n" + "    <div  class='form-check form-check-inline'>\n" + "    <input class='form-check-input' id='format-rules' type='checkbox' value='' checked>\n" + "    <label class='form-check-label' for='format-rules'>Format rules</label>\n" + "    </div>\n" + "  </div>\n" + "</div>\n" + "\n" + "<div>\n" + "<table class='table table-sm table-borderless'>\n" + "    <thead><th style='width:1%'>Line</th><th style='width:1%'>Coverage</th><th>Source code/method, queries and rules</th></thead>\n" + content + "\n" + "</table>\n" + "</div>\n" + GetFooter() + "</div>\n" + "</body>\n";
        }

        public virtual string GetLineWithoutCoverage(int lineNumber, string sourceCode)
        {
            string template = "<tr class='line line-code'><td>$lineNumber$</td><td></td>" + "<td colspan='2'><code>$sourceCode$</code></td>" + "</tr>\n";
            return template.Replace("$lineNumber$", JavaCs.NumToString(lineNumber)).Replace("$sourceCode$", GetSourceHtml(sourceCode));
        }

        public virtual string GetLineContent(int lineNumber, string coverage, string methodName, string sourceCode)
        {
            string template = "    <tr class='line line-coverage'>\n" + "        <td>$lineNumber$</td>\n" + "        <td class='nowrap'>$coverage$</td>\n" + "        <td colspan='2'>\n" + "            <span class='text-primary font-weight-bold $methodCssClass$'>$methodName$</span>\n" + "            <code>$sourceCode$</code>\n" + "        </td>\n" + "    </tr>\n";
            return template.Replace("$lineNumber$", JavaCs.NumToString(lineNumber)).Replace("$coverage$", coverage).Replace("$methodName$", methodName).Replace("$methodCssClass$", sourceCode == null ? "" : "method").Replace("$sourceCode$", sourceCode == null ? " &nbsp; (source code not available)" : GetSourceHtml(sourceCode));
        }

        public virtual string GetQueryContent(QueryReader query, string coverage, HistoryReader history)
        {
            string template = "    <tbody class='query'>\n" + "    <tr class='query-run'>\n" + "        <td></td>\n" + "        <td class='nowrap'>\n" + "            <span class='rules-show' title='Show rules'>&#9660;</span><span class='rules-hide' title='Hide rules'>&#9650;</span>\n" + "            $runCount$ run(s)\n" + "            <span class='params-show' title='Show eval result and params'>&#9655;</span><span class='params-hide' title='Hide eval result and params'>&#9665;</span> " + "            $coverage$\n" + "        </td>\n" + "        <td colspan='2'>\n" + "            <div class='params'>$parameters$</div>\n" + "            $sqlQuery$ $errorsQuery$" + "        </td>\n" + "    </tr>\n" + "    </tbody>\n";
            return template.Replace("$runCount$", JavaCs.NumToString(query.GetModel().GetQrun())).Replace("$coverage$", coverage).Replace("$sqlQuery$", GetSqlHtml(Encode(query.GetSql()))).Replace("$parameters$", GetHistoryItems(history)).Replace("$errorsQuery$", GetErrorsHtml(Encode(query.GetModel().GetErrorString().Replace("\n", "").Replace("\r", "")), query.GetModel().GetError()));
        }

        private string GetHistoryItems(HistoryReader history)
        {
            StringBuilder sb = new StringBuilder();
            if (history.GetItems() == null || history.GetItems().Count == 0)
                return "(Eval result and params are not available)";
            for (int i = 0; i < history.GetItems().Count; i++)
            {
                HistoryModel item = history.GetItems()[i];
                sb.Append(i != 0 ? "<br/>" : "").Append("<strong>").Append(i + 1).Append("</strong>.");
                string result = item.GetResult() == null ? "" : item.GetResult(); // can be null if query failed
                sb.Append(" Eval result: [<span class='result-vector'>" + result + "</span>]");
                sb.Append(" Params:");
                foreach (ParameterDao param in item.GetParams())
                    sb.Append(" ").Append(Encode(param.ToString()));
            }

            return sb.ToString();
        }

        public virtual string GetRulesContent(string rulesContent)
        {
            return "    <tbody class='rules'>\n" + rulesContent + "    <tr><td></td></tr>\n" + "    </tbody>\n\n";
        }

        public virtual string GetRuleContent(RuleModel rule)
        {
            string template = "    <tr class='rule-summary'>\n" + "        <td></td>\n" + "        <td class='coverage' rowspan='2'><strong>$ruleId$</strong> - dead:&nbsp;$ruleDead$ count:&nbsp;$ruleCount$</td>\n" + "        <td class='coverage' colspan='2'>\n" + "            category: $ruleCategory$ type: $ruleType$ subtype: $ruleSubtype$ location: $ruleLocation$\n" + "        </td>\n" + "    </tr>\n" + "    <tr class='rule-sql'>\n" + "        <td></td>\n" + ("".Equals(rule.GetDescription()) ? "            <td class='$ruleStatus$ rowspan='2'>$ruleSql$</td>\n" : "            <td class='$ruleStatus$'>$ruleDescription$</td>\n" + "            <td class='$ruleStatus$'>$ruleSql$</td>\n") + "    </tr>\n" + "    $ruleErrors$";
            return template.Replace("$ruleId$", rule.GetId()).Replace("$ruleDead$", JavaCs.NumToString(rule.GetDead())).Replace("$ruleCount$", JavaCs.NumToString(rule.GetCount())).Replace("$ruleCategory$", rule.GetCategory()).Replace("$ruleType$", rule.GetMainType()).Replace("$ruleSubtype$", rule.GetSubtype()).Replace("$ruleLocation$", Encode(rule.GetLocation())).Replace("$ruleStatus$", rule.GetDead() > 0 ? "covered" : "uncovered").Replace("$ruleDescription$", GetDescriptionHtml(Encode(rule.GetDescription()))).Replace("$ruleSql$", GetSqlHtml(Encode(rule.GetSql()))).Replace("$ruleErrors$", GetErrorsHtml(Encode(rule.GetErrorString())));
        }

        private string GetErrorsHtml(string queryErrors, int ruleErrorCount)
        {
            if (!"".Equals(queryErrors))
                return "\n<br/><span style='color:red;'>Query error: " + queryErrors + "</span>\n";
            if (ruleErrorCount > 0)
                return "\n<br/><span style='color:red;'>" + ruleErrorCount + " rule(s) with error</span>\n";
            return ""; // no error
        }

        private string GetErrorsHtml(string errors)
        {
            if (!"".Equals(errors))
                return "<tr class='rule-error'><td colspan='2'></td><td colspan='2'><span style=\"color:red;\">" + "Rule error: " + errors.Replace("\n", "\n<br/>") + "</span></td></tr>\n";
            return ""; // no error
        }

        private string GetSourceHtml(string sourceCode)
        {
            return Encode(sourceCode).Replace("\t", "    ").Replace(" ", "&nbsp;");
        }

        private string GetSqlHtml(string sql)
        {
            string br = " <br class='canhide'/>";
            sql = sql.Replace("SELECT ", "<strong>SELECT</strong> ").Replace("FROM ", br + "<strong>FROM</strong> ").Replace("LEFT JOIN ", br + "<strong>LEFT JOIN</strong> ").Replace("RIGHT JOIN ", br + "<strong>RIGHT JOIN</strong> ").Replace("INNER JOIN ", br + "<strong>INNER JOIN</strong> ").Replace("WHERE ", br + "<strong>WHERE</strong> ").Replace("GROUP BY ", br + "<strong>GROUP BY</strong> ").Replace("HAVING ", br + "<strong>HAVING</strong> ").Replace("ORDER BY ", br + "<strong>ORDER BY</strong> ").Replace("UNION ", br + "<strong>UNION</strong> ");
            return sql;
        }

        private string GetDescriptionHtml(string desc)
        {
            return desc.Replace("\n", " <br class='canhide'/>");
        }
    }
}