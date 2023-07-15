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
			IndextHtmlWriter ihw = new IndextHtmlWriter();
			StringBuilder isb = new StringBuilder();
			isb.Append(ihw.GetHeader());
			// Using a CoverageReader to access the store by class and then by query
			CoverageCollection classes = new CoverageReader(rulesFolder).GetByClass();
			StringBuilder ibodysb = new StringBuilder();
			for (int i = 0; i < classes.Size(); i++)
			{
				QueryCollection query = classes.Get(i);
				string className = query.GetName();
				CoverageSummary summary = query.GetSummary();
				ConsoleWrite("Report for class: " + className + " " + summary.ToString());
				ibodysb.Append(ihw.GetBody(className, summary.GetQrun(), summary.GetQcount(), summary.GetQerror(), summary.GetCount(), summary.GetDead(), summary.GetError()));
				// Generates a file for this class
				ClassHtmlWriter writer = new ClassHtmlWriter();
				StringBuilder qsb = new StringBuilder();
				qsb.Append(writer.GetHeader(className));
				// Includes each query
				for (int j = 0; j < query.Size(); j++)
				{
					string methodIdentifier = query.Get(j).GetKey().GetMethodName(true);
					QueryModel rules = query.Get(j).GetModel();
					qsb.Append(writer.GetQueryBody(rules, methodIdentifier, "query" + j));
				}
				qsb.Append(writer.GetFooter());
				FileUtil.FileWrite(reportFolder, className + ".html", qsb.ToString());
			}
			// Puts everything, with totals as first line
			CoverageSummary totals = classes.GetSummary();
			isb.Append(ihw.GetBody("TOTAL", totals.GetQrun(), totals.GetQcount(), totals.GetQerror(), totals.GetCount(), totals.GetDead(), totals.GetError()));
			isb.Append(ibodysb.ToString());
			isb.Append(ihw.GetFooter());
			FileUtil.FileWrite(reportFolder, "index.html", isb.ToString());
			ConsoleWrite(classes.Size() + " classes generated, see index.html at reports folder");
		}
	}
}
