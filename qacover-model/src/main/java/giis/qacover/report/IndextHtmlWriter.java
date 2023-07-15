package giis.qacover.report;

public class IndextHtmlWriter {
	
	public String getHeader() {
		return SharedHtmlWriter.getHtmlBegin("SQL Query Fpc Coverage", false) 
				+ "\n<h2>SQL Query Fpc Coverage</h2>"
				+ "\n<table class=\"table table-striped table-sm\">"
				+ "\n<tr><th>Class</th><th>%</th><th></th><th>qrun</th><th>qcount</th><th>qerror</th><th>dead</th><th>count</th><th>error</th></tr>";
	}
	
	public String getBody(String className, int qrun, int qcount, int qerror, int count, int dead, int error) {
		String template="\n\n<tr><td>$classLink</td>"
				+ "\n<td>$percent</td>"
				+ "\n<td><div style=\"width:90px;\" class=\"progress\">"
				+ "\n<div style=\"width:$percent;\" class=\"progress-bar bg-success\" role=\"progressbar\" aria-valuenow=\"$progbarvalue\" aria-valuemin=\"0\" aria-valuemax=\"100\"></div>"
				+ "\n</div></td>"
				+ "\n<td>$qrun</td><td>$qcount</td><td>$qerror</td><td>$dead</td><td>$count</td><td>$error</td></tr>";
		return template
				.replace("$classLink", "TOTAL".equals(className) ? className : "<a href=\"" + className + ".html\">" + className + "</a>")
				.replace("$percent", SharedHtmlWriter.percent(count,dead))
				.replace("$progbarvalue", SharedHtmlWriter.percent(count,dead).replace("%", ""))
				.replace("$qrun", String.valueOf(qrun))
				.replace("$qcount", String.valueOf(qcount))
				.replace("$qerror", qerror>0 ? "<span style=\"color:red;\">" + qerror + "</span>" : String.valueOf(qerror))
				.replace("$count", String.valueOf(count))
				.replace("$dead", String.valueOf(dead))
				.replace("$error", error>0 ? "<span style=\"color:red;\">" + error + "</span>" : String.valueOf(error));
	}
	public String getFooter() {
		return "\n</table>" + SharedHtmlWriter.getHtmlFooter() + SharedHtmlWriter.getHtmlEnd();
	}
}
