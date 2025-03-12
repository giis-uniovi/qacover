using Java.Util;
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
    /// <summary>
    /// Main class to generate html reports with all details of the coverage.
    /// </summary>
    public class ReportManager
    {
        public static void ConsoleWrite(string value)
        {
            System.Console.WriteLine(value); // NOSONAR for console app
        }

        public virtual void Run(string rulesFolder, string reportFolder)
        {
            Run(rulesFolder, reportFolder, "", "");
        }

        public virtual void Run(string rulesFolder, string reportFolder, string sourceFolders, string projectFolder)
        {
            ConsoleWrite("QACover version " + new Variability().GetVersion());
            ConsoleWrite("Rules folder: " + FileUtil.GetFullPath(rulesFolder));
            ConsoleWrite("Report folder: " + FileUtil.GetFullPath(reportFolder));
            if (!JavaCs.IsEmpty(sourceFolders))
                ConsoleWrite("Source folders: " + sourceFolders);
            if (!JavaCs.IsEmpty(projectFolder))
                ConsoleWrite("Project folder: " + projectFolder);
            FileUtil.CreateDirectory(reportFolder);

            // Report index
            IndextHtmlWriter indexWriter = new IndextHtmlWriter();

            // Using a CoverageReader to access the store by class and then by query
            CoverageCollection classes = new CoverageReader(rulesFolder).GetByClass();
            HistoryReader history = new CoverageReader(rulesFolder).GetHistory();
            StringBuilder indexRowsSb = new StringBuilder();
            string coverageCriterion = "";
            for (int i = 0; i < classes.GetSize(); i++)
            {
                QueryCollection query = classes.GetItem(i);

                // a line of the index report
                string className = query.GetName();
                CoverageSummary summary = query.GetSummary();
                ConsoleWrite("Report for class: " + className + " " + summary.ToString());
                indexRowsSb.Append(indexWriter.GetBodyRow(className, summary.GetQrun(), summary.GetQcount(), summary.GetQerror(), summary.GetCount(), summary.GetDead(), summary.GetError()));

                // to customize headings
                coverageCriterion = query.GetItem(0).GetModel().GetModel().GetRulesClass();

                // Generates a file for this class
                ClassHtmlWriter classWriter = new ClassHtmlWriter();
                string htmlCoverage = GetClassCoverage(query, history, classWriter, sourceFolders, projectFolder);
                string classTitle = className + " (" + coverageCriterion + " coverage)";
                string htmlCoverageContent = classWriter.GetHeader(classTitle) + classWriter.GetBodyContent(classTitle, htmlCoverage);
                FileUtil.FileWrite(reportFolder, className + ".html", htmlCoverageContent);
            }

            ConsoleWrite("Report index.");

            // Puts everything, with totals as first line
            CoverageSummary totals = classes.GetSummary();
            string indexRowsHeader = indexWriter.GetBodyRow("TOTAL", totals.GetQrun(), totals.GetQcount(), totals.GetQerror(), totals.GetCount(), totals.GetDead(), totals.GetError());
            string indexTitle = "QACover - SQL Query " + coverageCriterion.ToUpper() + " Coverage";
            string indexContent = indexWriter.GetHeader(indexTitle) + indexWriter.GetBodyContent(indexTitle, indexRowsHeader + indexRowsSb.ToString());
            FileUtil.FileWrite(reportFolder, "index.html", indexContent);
            ConsoleWrite(classes.GetSize() + " classes generated, see index.html at reports folder");
        }

        private string GetClassCoverage(QueryCollection queries, HistoryReader history, ClassHtmlWriter writer, string sourceFolders, string projectFolders)
        {

            // first makes groups by line number, each line can have zero, one or more queries
            SourceCodeCollection lineCollection = new SourceCodeCollection();
            lineCollection.AddQueries(queries); // groups by line number

            // locates the source code for this class (requires the source folders as parameter)
            if (!JavaCs.IsEmpty(sourceFolders) && queries.GetSize() > 0)
                lineCollection.AddSources(queries.GetItem(0), sourceFolders, projectFolders);
            StringBuilder csb = new StringBuilder();
            Map<int, SourceCodeLine> lines = lineCollection.GetLines();
            foreach (int lineNumber in lines.KeySet())
            {

                // NOSONAR for net compatibility
                SourceCodeLine lineContent = lines[lineNumber];
                if (lineContent.GetQueries().Count == 0)
                {

                    // NOSONAR for net compatibility
                    // only the source code if there are no queries
                    csb.Append(writer.GetLineWithoutCoverage(lineNumber, lineContent.GetSource()));
                }
                else
                {

                    // Coverage and source Code of the line, or method if no line is available
                    QueryReader query0 = lineContent.GetQueries()[0];
                    csb.Append(writer.GetLineContent(lineNumber, GetLineCoverage(lineContent, writer), query0.GetKey().GetMethodName(), lineContent.GetSource()));
                }


                // Each query (if any) and details of their rules and history
                foreach (QueryReader thisQuery in lineContent.GetQueries())
                {
                    string queryCoverage = GetQueryCoverage(thisQuery.GetModel(), lineContent, writer);
                    HistoryReader queryHistory = history.GetHistoryAtQuery(thisQuery.GetKey());
                    csb.Append(writer.GetQueryContent(thisQuery, queryCoverage, queryHistory));
                    StringBuilder rsb = new StringBuilder();
                    for (int k = 0; k < thisQuery.GetModel().GetRules().Count; k++)
                        rsb.Append(writer.GetRuleContent(thisQuery.GetModel().GetRules()[k]));
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
                return writer.Coverage(line.GetQueries()[0].GetModel().GetDead(), line.GetQueries()[0].GetModel().GetCount());
            else if (line.GetQueries().Count > 1)
                return writer.Coverage(line.GetDead(), line.GetCount());
            return "";
        }

        private string GetQueryCoverage(QueryModel query, SourceCodeLine line, ClassHtmlWriter writer)
        {

            // If more than a query returns the query coverage, if not, empty (coverage
            // should be shown in the line)
            if (line.GetQueries().Count > 1)
                return writer.Coverage(query.GetDead(), query.GetCount());
            return "";
        }
    }
}