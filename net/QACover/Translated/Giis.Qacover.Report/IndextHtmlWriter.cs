/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////


namespace Giis.Qacover.Report
{
	public class IndextHtmlWriter : BaseHtmlWriter
	{
		protected internal override string GetScripts()
		{
			return string.Empty;
		}

		protected internal override string GetStyles()
		{
			return "<style>\n" + "    .fill { min-width: 100%; width: 100%; }\n" + "    .progress {margin-bottom: 0 !important; }\n" + "    tr.class-row td { padding-top: 2px; padding-bottom: 2px; }\n" + "</style>\n";
		}

		public virtual string GetBodyContent(string title, string content)
		{
			return "<body>\n" + "<div class='container'>\n" + "  <h2>" + title + "</h2>\n" + "  <table class=\"table table-striped table-sm\">\n" + "  <tr><th>Class</th><th>%</th><th></th><th>qrun</th><th>qcount</th><th>qerror</th><th>dead</th><th>count</th><th>error</th></tr>\n" + content + 
				"  </table>\n" + GetFooter() + "</div>\n" + "</body>\n";
		}

		public virtual string GetBodyRow(string className, int qrun, int qcount, int qerror, int count, int dead, int error)
		{
			string template = "  <tr class='class-row'>\n" + "    <td>$classLink</td>\n" + "    <td>$percent</td>\n" + "    <td><div style=\"width:90px;\" class=\"progress\">\n" + "    <div style=\"width:$percent;\" class=\"progress-bar bg-success\" role=\"progressbar\" aria-valuenow=\"$progbarvalue\" aria-valuemin=\"0\" aria-valuemax=\"100\"></div>\n"
				 + "    </div></td>\n" + "    <td>$qrun</td><td>$qcount</td><td>$qerror</td><td>$dead</td><td>$count</td><td>$error</td>\n" + "  </tr>\n";
			return template.Replace("$classLink", "TOTAL".Equals(className) ? className : "<a href=\"" + className + ".html\">" + className + "</a>").Replace("$percent", Percent(count, dead)).Replace("$progbarvalue", Percent(count, dead).Replace("%", string.Empty)).Replace("$qrun", qrun.ToString
				()).Replace("$qcount", qcount.ToString()).Replace("$qerror", qerror > 0 ? "<span style=\"color:red;\">" + qerror + "</span>" : qerror.ToString()).Replace("$count", count.ToString()).Replace("$dead", dead.ToString()).Replace("$error", error > 0 ? "<span style=\"color:red;\">" + error
				 + "</span>" : error.ToString());
		}
	}
}
