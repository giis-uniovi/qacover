package giis.qacover.report;

import giis.portable.xml.tiny.XNodeAbstract;
import giis.qacover.model.QueryModel;
import giis.qacover.model.RuleModel;

public class ClassHtmlWriter {

	private static final String HTML_NEWLINE = "<br/>";
	
	public String getHeader(String className) {
		return SharedHtmlWriter.getHtmlBegin(className, true)
				+ "\n<h4>"+className+"</h4>"
				+ "\n<div class=\"panel-group\" id=\"accordion\">";
	}
	
	public String getQueryBody(QueryModel rules, String methodIdentifier, String queryId) {
		StringBuilder sb = new StringBuilder();
		sb.append(getRuleHeader(rules, methodIdentifier, queryId));
		for (RuleModel rule : rules.getRules())
			sb.append(getRuleBody(rule));
		sb.append(getRuleFooter());
		return sb.toString();
	}

	public String getFooter() {
		return "\n</div>" + SharedHtmlWriter.getHtmlEnd();
	}

	// Details of each rule
	
	private String getRuleHeader(QueryModel rules, String methodIdentifier, String queryId) {
		String sql=rules.getSql();
		return "\n\n<div class=\"panel panel-default\">"
				+ "\n<div class=\"panel-heading\">"
				+ "\n<table class=\"table table-striped table-sm\">"
				+ "\n<tr>"
				+ "\n<td><a data-toggle=\"collapse\" data-parent=\"#accordion\" href=\"#"+queryId+"\">" + methodIdentifier + "</a>"
				+ "\n<br/><strong>" + SharedHtmlWriter.percent(rules.getCount(),rules.getDead()) + "</strong> (" + rules.getDead()+"/" + rules.getCount() + ")"
				+ " " + rules.getQrun()+" run(s)"
				+ (rules.getError() > 0 ? " <span style=\"color:red;\">" + rules.getError()+" rule(s) with error</span>" : "")
				+ "\n</td>"
				+ "\n<td valign=\"top\">" + getSqlToHtml(XNodeAbstract.encodeText(sql))
				+ getErrorsHtml(rules)
				+ "\n</td>"
				+ "\n</tr>"
				+ "\n</table>"
				+ "\n</div>"
				+ "\n<div class=\"panel-collapse collapse\" id=\"" + queryId + "\">"
				+ "\n<table class=\"table table-sm\">"
				+ "\n<tr><th>ID</th><th>dead</th><th>count</th><th>category</th><th>type</th><th>subtype</th><th>location</th></tr>";
	}
	
	public String getRuleBody(RuleModel rule) {
		boolean covered = rule.getDead() != 0;
		String template="\n<tr><td><strong>$id</strong></td><td>$dead</td><td>$count</td>"
				+ "\n<td>$category</td><td>$type</td><td>$subtype</td><td>$location</td></tr>"
				+ "\n<td colspan=\"7\" $style >"
				+ "<div class=\"row\">"
				+ "<div class=\"col-5\">$description</div>"
				+ "<div class=\"col-7\">$sql$error</div>"
				+ "</div>"
				+ "</td></tr>";
		return template
				.replace("$id", rule.getId())
				.replace("$count", String.valueOf(rule.getCount()))
				.replace("$dead", String.valueOf(rule.getDead()))
				.replace("$category", rule.getCategory())
				.replace("$subtype", rule.getSubtype())
				.replace("$type", rule.getMainType())
				.replace("$location", rule.getLocation())
				.replace("$description", XNodeAbstract.encodeText(rule.getDescription()).replace("\n",HTML_NEWLINE))
				.replace("$sql", getSqlToHtml(rule.getSql()))
				.replace("$error", getErrorsHtml(rule))
				.replace("$style", covered ? bgcolor("palegreen") : bgcolor("lightyellow"));
	}
	
	private String getErrorsHtml(QueryModel rule) {
		String errors = rule.getErrorString().replace("\n", HTML_NEWLINE);
		if (!"".equals(errors))
			return "<br/><span style=\"color:red;\">" + errors + "</span>";
		return ""; //no error
	}
	private String getErrorsHtml(RuleModel rule) {
		String errors = rule.getErrorString().replace("\n", HTML_NEWLINE);
		if (!"".equals(errors))
			return "<br/><span style=\"color:red;\">"  + errors + "</span>";
		return ""; //no error
	}
	private String getRuleFooter() {
		return "\n</table></div></div>"; //NOSONAR
	}
	private String bgcolor(String color) {
		return  " style=\"background:"+color+";\"";
	}
	
	/**
	 * A simple formating of SQL to display in html
	 */
	private String getSqlToHtml(String sql) {
		sql = sql.replace("SELECT ", "<strong>SELECT</strong> ");
		sql = sql.replace("FROM ", "<br/><strong>FROM</strong> ");
		sql = sql.replace("LEFT JOIN ", "<br/><strong>LEFT JOIN</strong> ");
		sql = sql.replace("RIGHT JOIN ", "<br/><strong>RIGHT JOIN</strong> ");
		sql = sql.replace("INNER JOIN ", "<br/><strong>INNER JOIN</strong> ");
		sql = sql.replace("WHERE ", "<br/><strong>WHERE</strong> ");
		sql = sql.replace("GROUP BY ", "<br/><strong>GROUP BY</strong> ");
		sql = sql.replace("HAVING ", "<br/><strong>HAVING</strong> ");
		sql = sql.replace("ORDER BY ", "<br/><strong>ORDER BY</strong> ");
		return sql;
	}
	
}
