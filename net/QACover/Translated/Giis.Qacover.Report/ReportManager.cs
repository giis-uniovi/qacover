/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
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

		private string GetClassCoverage(QueryCollection queries, ClassHtmlWriter writer)
		{
			// first makes groups by line number, each line can have zero, one or more queries
			SourceCodeCollection lineCollection = new SourceCodeCollection();
			lineCollection.AddQueries(queries);
			// groups by line number
			StringBuilder csb = new StringBuilder();
			foreach (KeyValuePair<int, SourceCodeLine> line in lineCollection.GetLines())
			{
				// Coverage and source Code of the line, or method if no line is available
				QueryReader query0 = line.Value.GetQueries()[0];
				csb.Append(writer.GetLineContent(line.Key, GetLineCoverage(line.Value, writer), query0.GetKey().GetMethodName(), line.Value.GetSource()));
				// Each query (if any) and details of their rules
				foreach (QueryReader thisQuery in line.Value.GetQueries())
				{
					string queryCoverage = GetQueryCoverage(thisQuery.GetModel(), line.Value, writer);
					csb.Append(writer.GetQueryContent(thisQuery, queryCoverage));
					StringBuilder rsb = new StringBuilder();
					for (int k = 0; k < thisQuery.GetModel().GetRules().Count; k++)
					{
						rsb.Append(writer.GetRuleContent(thisQuery.GetModel().GetRules()[k]));
					}
					csb.Append(writer.GetRulesContent(rsb.ToString()));
				}
			}
			return csb.ToString();
		}

		private string GetLineCoverage(SourceCodeLine line, ClassHtmlWriter writer)
		{
			// If only a query, returns its coverage, if more, returns the aggregate
			// coverage of all queries
			// If there is no queries returns empty
			if (line.GetQueries().Count == 1)
			{
				return writer.Coverage(line.GetQueries()[0].GetModel().GetDead(), line.GetQueries()[0].GetModel().GetCount());
			}
			else
			{
				if (line.GetQueries().Count > 1)
				{
					return writer.Coverage(line.GetDead(), line.GetCount());
				}
			}
			return string.Empty;
		}

		private string GetQueryCoverage(QueryModel query, SourceCodeLine line, ClassHtmlWriter writer)
		{
			// If more than a query returns the query coverage, if not, empty (coverage
			// should be shown in the line)
			if (line.GetQueries().Count > 1)
			{
				return writer.Coverage(query.GetDead(), query.GetCount());
			}
			return string.Empty;
		}
	}
}
