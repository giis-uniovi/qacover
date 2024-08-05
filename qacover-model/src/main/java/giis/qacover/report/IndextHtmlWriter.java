package giis.qacover.report;

public class IndextHtmlWriter extends BaseHtmlWriter {
	
	@Override
	protected String getScripts() {
		return "";
	}
	
	@Override
	protected String getStyles() {
		return "<style>\n"
				+ "    .fill { min-width: 100%; width: 100%; }\n"
				+ "    .progress {margin-bottom: 0 !important; }\n"
				+ "    tr.class-row td { padding-top: 2px; padding-bottom: 2px; }\n"
				+ "</style>\n";
	}

	public String getBodyContent(String title, String content) {
		return "<body>\n"
				+ "<div class='container'>\n"
				+ "  <h2>" + title + "</h2>\n"
				+ "  <table class=\"table table-striped table-sm\">\n"
				+ "  <tr><th>Class</th><th>%</th><th></th><th>qrun</th><th>qcount</th><th>qerror</th><th>dead</th><th>count</th><th>error</th></tr>\n"
				+ content
				+ "  </table>\n"
				+ getFooter()
				+ "</div>\n"
				+ "</body>\n";
	}
	
	public String getBodyRow(String className, int qrun, int qcount, int qerror, int count, int dead, int error) {
		String template="  <tr class='class-row'>\n"
				+ "    <td>$classLink</td>\n"
				+ "    <td>$percent</td>\n"
				+ "    <td><div style=\"width:90px;\" class=\"progress\">\n"
				+ "    <div style=\"width:$percent;\" class=\"progress-bar bg-success\" role=\"progressbar\" aria-valuenow=\"$progbarvalue\" aria-valuemin=\"0\" aria-valuemax=\"100\"></div>\n"
				+ "    </div></td>\n"
				+ "    <td>$qrun</td><td>$qcount</td><td>$qerror</td><td>$dead</td><td>$count</td><td>$error</td>\n"
				+ "  </tr>\n";
		return template
				.replace("$classLink", "TOTAL".equals(className) ? className : "<a href=\"" + className + ".html\">" + className + "</a>")
				.replace("$percent", percent(count,dead))
				.replace("$progbarvalue", percent(count,dead).replace("%", ""))
				.replace("$qrun", String.valueOf(qrun))
				.replace("$qcount", String.valueOf(qcount))
				.replace("$qerror", qerror > 0 ? "<span style=\"color:red;\">" + qerror + "</span>" : String.valueOf(qerror))
				.replace("$count", String.valueOf(count))
				.replace("$dead", String.valueOf(dead))
				.replace("$error", error>0 ? "<span style=\"color:red;\">" + error + "</span>" : String.valueOf(error));
	}

}
