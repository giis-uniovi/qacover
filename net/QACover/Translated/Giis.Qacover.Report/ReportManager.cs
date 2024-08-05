/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using System.Text;
using Giis.Portable.Util;
using Giis.Qacover.Model;
using Giis.Qacover.Reader;


namespace Giis.Qacover.Report
{
	/// <summary>Main class to generate html reports with all details of the coverage.</summary>
	public class ReportManager
	{
		public static void ConsoleWrite(string value)
		{
			System.Console.Out.WriteLine(value);
		}

		// NOSONAR for console app
		public virtual void Run(string rulesFolder, string reportFolder)
		{
			ConsoleWrite("QACover version " + new Variability().GetVersion());
			ConsoleWrite("Rules folder: " + FileUtil.GetFullPath(rulesFolder));
			ConsoleWrite("Report folder: " + FileUtil.GetFullPath(reportFolder));
			FileUtil.CreateDirectory(reportFolder);
			// Report index
			IndextHtmlWriter indexWriter = new IndextHtmlWriter();
			// Using a CoverageReader to access the store by class and then by query
			CoverageCollection classes = new CoverageReader(rulesFolder).GetByClass();
			StringBuilder indexRowsSb = new StringBuilder();
			for (int i = 0; i < classes.Size(); i++)
			{
				QueryCollection query = classes.Get(i);
				// a line of the index report
				string className = query.GetName();
				CoverageSummary summary = query.GetSummary();
				ConsoleWrite("Report for class: " + className + " " + summary.ToString());
				indexRowsSb.Append(indexWriter.GetBodyRow(className, summary.GetQrun(), summary.GetQcount(), summary.GetQerror(), summary.GetCount(), summary.GetDead(), summary.GetError()));
				// Generates a file for this class
				ClassHtmlWriter classWriter = new ClassHtmlWriter();
				string htmlCoverage = GetClassCoverage(query, classWriter);
				string htmlCoverageContent = classWriter.GetHeader(className) + classWriter.GetBodyContent(className, htmlCoverage);
				FileUtil.FileWrite(reportFolder, className + ".html", htmlCoverageContent);
			}
			// Puts everything, with totals as first line
			CoverageSummary totals = classes.GetSummary();
			string indexRowsHeader = indexWriter.GetBodyRow("TOTAL", totals.GetQrun(), totals.GetQcount(), totals.GetQerror(), totals.GetCount(), totals.GetDead(), totals.GetError());
			string indexContent = indexWriter.GetHeader("SQL Query Fpc Coverage") + indexWriter.GetBodyContent("SQL Query Fpc Coverage", indexRowsHeader + indexRowsSb.ToString());
			FileUtil.FileWrite(reportFolder, "index.html", indexContent);
			ConsoleWrite(classes.Size() + " classes generated, see index.html at reports folder");
		}

		private string GetClassCoverage(QueryCollection query, ClassHtmlWriter writer)
		{
			StringBuilder csb = new StringBuilder();
			for (int j = 0; j < query.Size(); j++)
			{
				QueryReader thisQuery = query.Get(j);
				csb.Append(writer.GetLineContent(thisQuery)).Append(writer.GetQueryContent(query.Get(j)));
				StringBuilder rsb = new StringBuilder();
				for (int k = 0; k < thisQuery.GetModel().GetRules().Count; k++)
				{
					rsb.Append(writer.GetRuleContent(thisQuery.GetModel().GetRules()[k]));
				}
				csb.Append(writer.GetRulesContent(rsb.ToString()));
			}
			return csb.ToString();
		}
	}
}
