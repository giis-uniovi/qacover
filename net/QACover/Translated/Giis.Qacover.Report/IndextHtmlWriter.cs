/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////


namespace Giis.Qacover.Report
{
	public class IndextHtmlWriter
	{
		public virtual string GetHeader()
		{
			return SharedHtmlWriter.GetHtmlBegin("SQL Query Fpc Coverage", false) + "\n<h2>SQL Query Fpc Coverage</h2>" + "\n<table class=\"table table-striped table-sm\">" + "\n<tr><th>Class</th><th>%</th><th></th><th>qrun</th><th>qcount</th><th>qerror</th><th>dead</th><th>count</th><th>error</th></tr>";
		}

		public virtual string GetBody(string className, int qrun, int qcount, int qerror, int count, int dead, int error)
		{
			string template = "\n\n<tr><td>$classLink</td>" + "\n<td>$percent</td>" + "\n<td><div style=\"width:90px;\" class=\"progress\">" + "\n<div style=\"width:$percent;\" class=\"progress-bar bg-success\" role=\"progressbar\" aria-valuenow=\"$progbarvalue\" aria-valuemin=\"0\" aria-valuemax=\"100\"></div>"
				 + "\n</div></td>" + "\n<td>$qrun</td><td>$qcount</td><td>$qerror</td><td>$dead</td><td>$count</td><td>$error</td></tr>";
			return template.Replace("$classLink", "TOTAL".Equals(className) ? className : "<a href=\"" + className + ".html\">" + className + "</a>").Replace("$percent", SharedHtmlWriter.Percent(count, dead)).Replace("$progbarvalue", SharedHtmlWriter.Percent(count, dead).Replace("%", string.Empty
				)).Replace("$qrun", qrun.ToString()).Replace("$qcount", qcount.ToString()).Replace("$qerror", qerror > 0 ? "<span style=\"color:red;\">" + qerror + "</span>" : qerror.ToString()).Replace("$count", count.ToString()).Replace("$dead", dead.ToString()).Replace("$error", error > 0 ? "<span style=\"color:red;\">"
				 + error + "</span>" : error.ToString());
		}

		public virtual string GetFooter()
		{
			return "\n</table>" + SharedHtmlWriter.GetHtmlFooter() + SharedHtmlWriter.GetHtmlEnd();
		}
	}
}
