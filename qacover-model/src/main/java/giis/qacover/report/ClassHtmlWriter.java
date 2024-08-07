package giis.qacover.report;

import giis.qacover.model.RuleModel;
import giis.qacover.reader.QueryReader;

public class ClassHtmlWriter extends BaseHtmlWriter {
	
	@Override
	protected String getScripts() {
		return "<script>\n"
				+ "    $(document).ready(function() {\n"
				+ "        $('.rules').show();\n"
				+ "        $('.rules-show').show();\n"
				+ "        $('.rules-hide').hide();\n"
				+ "        $('.params-show').show();\n"
				+ "        $('.params-hide').hide();\n"
				+ "        $('.rules').hide();\n"
				+ "        $('.params').hide();\n"
				+ "        $('.query br.canhide').hide(); \n"
				+ "        $('.rules br.canhide').show(); \n"
				+ "        if ($('#view-source').is(':checked')) { $('.method').hide(); } else { $('.method').show(); }\n"
				+ "        $('.rules-show').click(function(){ $(this).hide(); $(this).next().show(); $(this).closest('.query').next('.rules').show(); });\n"
				+ "        $('.rules-hide').click(function(){ $(this).hide(); $(this).prev().show(); $(this).closest('.query').next('.rules').hide(); });\n"
				+ "        $('.params-show').click(function(){ $(this).hide(); $(this).next().show(); $(this).closest('.query').find('.params').show(); $(this).closest('.query').next('.rules').find('.params').show(); });\n"
				+ "        $('.params-hide').click(function(){ $(this).hide(); $(this).prev().show(); $(this).closest('.query').find('.params').hide(); $(this).closest('.query').next('.rules').find('.params').hide(); });\n"
				+ "        $('#format-queries').change(function(){ if ($(this).is(':checked')) $('.query br.canhide').show(); else $('.query br.canhide').hide(); });\n"
				+ "        $('#format-rules').change(function(){ if ($(this).is(':checked')) $('.rules br.canhide').show(); else $('.rules br.canhide').hide(); });\n"
				+ "        $('#view-source').change(function() { \n"
				+ "            if ($(this).is(':checked')) { $('.line-code').show(); $('code').show(); $('.method').hide(); } \n"
				+ "            else { $('.line-code').hide(); $('code').hide(); $('.method').show(); }\n"
				+ "        }\n"
				+ "    )});\n"
				+ "</script>\n";
	}
	
	@Override
	protected String getStyles() {
		return "<style>\n"
				+ "    .fill { min-width: 100%; width: 100%; } \n"
				+ "    tr.line, tr.line td { line-height:18px; padding-top:0; padding-bottom:0 }\n"
				+ "    tr.query-run td, tr.rule-summary td, tr.rule-sql td, tr.rule-error td { padding-top:0; padding-bottom:0 }"
				+ "    code { color: DimGray; position: absolute; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }\n"
				+ "    tbody.query { background: lightgrey; }\n"
				+ "    td.nowrap, span.nowrap { white-space: nowrap; }\n"
				+ "    td.covered { background:aquamarine; }\n"
				+ "    td.uncovered { background:lightyellow; }\n"
				+ "    .params  { font-size: small; font-style: italic }\n"
				+ "    .rules-show, .rules-hide, .params-show, .params-hide { cursor: pointer; }\n"
				+ "</style>\n";
	}

	public String getBodyContent(String title, String content) {
		return "<body style='overflow-x: hidden;'>\n"
				+ "<div class='container fill'>\n"
				+ "<div class='sticky-md-top text-bg-light'>\n"
				+ "  <h4><a href='./index.html'>[Index]</a> " + title + "</h4>\n"
				+ "  <div class='col-auto'>\n"
				+ "    <div  class='form-check form-check-inline'>\n"
				+ "    <input class='form-check-input' id='view-source' type='checkbox' value='' checked>\n"
				+ "    <label class='form-check-label' for='view-source'>View source</label>\n"
				+ "    </div>\n"
				+ "    <div  class='form-check form-check-inline'>\n"
				+ "    <input class='form-check-input' id='format-queries' type='checkbox' value=''>\n"
				+ "    <label class='form-check-label' for='format-queries'>Format queries</label>\n"
				+ "    </div>\n"
				+ "    <div  class='form-check form-check-inline'>\n"
				+ "    <input class='form-check-input' id='format-rules' type='checkbox' value='' checked>\n"
				+ "    <label class='form-check-label' for='format-rules'>Format rules</label>\n"
				+ "    </div>\n"
				+ "  </div>\n"
				+ "</div>\n"
				+ "\n"
				+ "<div>\n"
				+ "<table class='table table-sm table-borderless'>\n"
				+ "    <thead><th style='width:1%'>Line</th><th style='width:1%'>Coverage</th><th>Source code/method, queries and rules</th></thead>\n"
				+ content + "\n"
				+ "</table>\n"
				+ "</div>\n"
				+ getFooter()
				+ "</div>\n"
				+ "</body>\n";
	}
	
	public String getLineWithoutCoverage(int lineNumber, String sourceCode) {
		String template =  "<tr class='line line-code'><td>$lineNumber$</td><td></td>"
				+ "<td colspan='2'><code>$sourceCode$</code></td>"
				+ "</tr>\n";
		return template
				.replace("$lineNumber$", String.valueOf(lineNumber))
				.replace("$sourceCode$", getSourceHtml(sourceCode));
	}
	
	public String getLineContent(int lineNumber, String coverage, String methodName, String sourceCode) {
		String template = "    <tr class='line line-coverage'>\n"
				+ "        <td>$lineNumber$</td>\n"
				+ "        <td class='nowrap'>$coverage$</td>\n"
				+ "        <td colspan='2'>\n"
				+ "            <span class='text-primary font-weight-bold $methodCssClass$'>$methodName$</span>\n"
				+ "            <code>$sourceCode$</code>\n"
				+ "        </td>\n"
				+ "    </tr>\n";
		return template
				.replace("$lineNumber$", String.valueOf(lineNumber))
				.replace("$coverage$", coverage)
				.replace("$methodName$", methodName)
				// if source code not available, the css class for method is empty (it will be shown even if hidden by the ui)
				.replace("$methodCssClass$", sourceCode == null ? "" : "method")
				.replace("$sourceCode$", sourceCode == null ? " &nbsp; (source code not available)" : getSourceHtml(sourceCode));
	}
	
	public String getQueryContent(QueryReader query, String coverage) {
		String template="    <tbody class='query'>\n"
				+ "    <tr class='query-run'>\n"
				+ "        <td></td>\n"
				+ "        <td class='nowrap'>\n"
				+ "            <span class='rules-show' title='Show rules'>&#9660;</span><span class='rules-hide' title='Hide rules'>&#9650;</span>\n"
				+ "            $runCount$ run(s)\n"
				+ "            <span class='params-show' title='Show run params'>&#9655;</span><span class='params-hide' title='Hide run paramss'>&#9665;</span> "
				+ "            $coverage$\n"
				+ "        </td>\n"
				+ "        <td colspan='2'>\n"
				+ "            <div class='params'>(run params not available)</div>\n"
				+ "            $sqlQuery$ $errorsQuery$"
				+ "        </td>\n"
				+ "    </tr>\n"
				+ "    </tbody>\n";
		return template.replace("$runCount$", String.valueOf(query.getModel().getQrun()))
				.replace("$coverage$", coverage)
				.replace("$sqlQuery$", getSqlHtml(encode(query.getSql())))
				// encode and remove line endings (to allow use a regex to replace platform dependent messages)
				.replace("$errorsQuery$", getErrorsHtml(encode(
						query.getModel().getErrorString().replace("\n", "").replace("\r", "")), 
						query.getModel().getError()
						));
	}
	
	public String getRulesContent(String rulesContent) {
		return "    <tbody class='rules'>\n"
				+ rulesContent
				+ "    <tr><td></td></tr>\n" // just to give a little separation after rules
				+ "    </tbody>\n\n";
	}
	
	public String getRuleContent(RuleModel rule) {
		String template = "    <tr class='rule-summary'>\n"
				+ "        <td></td>\n"
				+ "        <td class='coverage' rowspan='2'><strong>$ruleId$</strong> - dead:&nbsp;$ruleDead$ count:&nbsp;$ruleCount$</td>\n"
				+ "        <td class='coverage' colspan='2'>\n"
				+ "            category: $ruleCategory$ type: $ruleType$ subtype: $ruleSubtype$ location: $ruleLocation$\n"
				+ "            <div class='params'>(run params not available)</div>\n"
				+ "        </td>\n"
				+ "    </tr>\n"
				+ "    <tr class='rule-sql'>\n"
				+ "        <td></td>\n"
				+ "            <td class='$ruleStatus$'>$ruleDescription$</td>\n"
				+ "            <td class='$ruleStatus$'>$ruleSql$</td>\n"
				+ "    </tr>\n"
				// needs a complete row to allow error message span across description and sql columns
				+ "    $ruleErrors$";
		return template.replace("$ruleId$", rule.getId())
				.replace("$ruleDead$", String.valueOf(rule.getDead()))
				.replace("$ruleCount$", String.valueOf(rule.getCount()))
				.replace("$ruleCategory$", rule.getCategory())
				.replace("$ruleType$", rule.getMainType())
				.replace("$ruleSubtype$", rule.getSubtype())
				.replace("$ruleLocation$", encode(rule.getLocation()))
				.replace("$ruleStatus$", rule.getDead() > 0 ? "covered" : "uncovered")
				.replace("$ruleDescription$", getDescriptionHtml(encode(rule.getDescription())))
				.replace("$ruleSql$", getSqlHtml(encode(rule.getSql())))
				.replace("$ruleErrors$", getErrorsHtml(encode(rule.getErrorString())))
				;
	}

	private String getErrorsHtml(String queryErrors, int ruleErrorCount) {
		if (!"".equals(queryErrors)) // error in query execution
			return "\n<br/><span style='color:red;'>Query error: " + queryErrors + "</span>\n";
		if (ruleErrorCount > 0) // error in any rule(s)
			return "\n<br/><span style='color:red;'>" + ruleErrorCount + " rule(s) with error</span>\n";
		return ""; // no error
	}

	private String getErrorsHtml(String errors) {
		if (!"".equals(errors))
			return "<tr class='rule-error'><td colspan='2'></td><td colspan='2'><span style=\"color:red;\">"
					+ "Rule error: " + errors.replace("\n", "\n<br/>") + "</span></td></tr>\n";
		return ""; // no error
	}
	
	private String getSourceHtml(String sourceCode) {
		return encode(sourceCode).replace("\t", "    ").replace(" ", "&nbsp;");
	}
	
	private String getSqlHtml(String sql) {
		String br = " <br class='canhide'/>";
		sql = sql.replace("SELECT ", "<strong>SELECT</strong> ")
			.replace("FROM ", br + "<strong>FROM</strong> ")
			.replace("LEFT JOIN ", br + "<strong>LEFT JOIN</strong> ")
			.replace("RIGHT JOIN ", br + "<strong>RIGHT JOIN</strong> ")
			.replace("INNER JOIN ", br + "<strong>INNER JOIN</strong> ")
			.replace("WHERE ", br + "<strong>WHERE</strong> ")
			.replace("GROUP BY ", br + "<strong>GROUP BY</strong> ")
			.replace("HAVING ", br + "<strong>HAVING</strong> ")
			.replace("ORDER BY ", br + "<strong>ORDER BY</strong> ")
			.replace("UNION ", br + "<strong>UNION</strong> ");
		return sql;
	}
	
	private String getDescriptionHtml(String desc) {
		return desc.replace("\n", " <br class='canhide'/>");
	}
	
}
