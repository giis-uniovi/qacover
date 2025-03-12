package giis.qacover.report;

import java.util.Map;

import giis.portable.util.FileUtil;
import giis.portable.util.JavaCs;
import giis.qacover.model.QueryModel;
import giis.qacover.model.Variability;
import giis.qacover.reader.CoverageCollection;
import giis.qacover.reader.CoverageReader;
import giis.qacover.reader.CoverageSummary;
import giis.qacover.reader.HistoryReader;
import giis.qacover.reader.QueryCollection;
import giis.qacover.reader.QueryReader;
import giis.qacover.reader.SourceCodeCollection;
import giis.qacover.reader.SourceCodeLine;

/**
 * Main class to generate html reports with all details of the coverage.
 */
public class ReportManager {

	public static void consoleWrite(String value) {
		System.out.println(value); // NOSONAR for console app
	}
	public void run(String rulesFolder, String reportFolder) {
		run(rulesFolder, reportFolder, "", "");
	}
	public void run(String rulesFolder, String reportFolder, String sourceFolders, String projectFolder) {
		consoleWrite("QACover version " + new Variability().getVersion());
		consoleWrite("Rules folder: " + FileUtil.getFullPath(rulesFolder));
		consoleWrite("Report folder: " + FileUtil.getFullPath(reportFolder));
		if (!JavaCs.isEmpty(sourceFolders))
			consoleWrite("Source folders: " + sourceFolders);
		if (!JavaCs.isEmpty(projectFolder))
			consoleWrite("Project folder: " + projectFolder);
		FileUtil.createDirectory(reportFolder);

		// Report index
		IndextHtmlWriter indexWriter = new IndextHtmlWriter();

		// Using a CoverageReader to access the store by class and then by query
		CoverageCollection classes = new CoverageReader(rulesFolder).getByClass();
		HistoryReader history = new CoverageReader(rulesFolder).getHistory();
		StringBuilder indexRowsSb = new StringBuilder();

		String coverageCriterion = "";
		for (int i = 0; i < classes.getSize(); i++) {
			QueryCollection query = classes.getItem(i);
			// a line of the index report
			String className = query.getName();
			CoverageSummary summary = query.getSummary();
			consoleWrite("Report for class: " + className + " " + summary.toString());
			indexRowsSb.append(indexWriter.getBodyRow(className, 
					summary.getQrun(), summary.getQcount(), summary.getQerror(),
					summary.getCount(), summary.getDead(), summary.getError()));

			// to customize headings
			coverageCriterion = query.getItem(0).getModel().getModel().getRulesClass();
			
			// Generates a file for this class
			ClassHtmlWriter classWriter = new ClassHtmlWriter();
			String htmlCoverage = getClassCoverage(query, history, classWriter, sourceFolders, projectFolder);
			String classTitle = className + " (" + coverageCriterion + " coverage)";
			String htmlCoverageContent = classWriter.getHeader(classTitle) 
					+ classWriter.getBodyContent(classTitle, htmlCoverage);
			FileUtil.fileWrite(reportFolder, className + ".html", htmlCoverageContent);
		}
		consoleWrite("Report index.");
		
		// Puts everything, with totals as first line
		CoverageSummary totals = classes.getSummary();
		String indexRowsHeader = indexWriter.getBodyRow("TOTAL", totals.getQrun(), totals.getQcount(), totals.getQerror(), totals.getCount(),
				totals.getDead(), totals.getError());
		String indexTitle = "QACover - SQL Query " + coverageCriterion.toUpperCase() + " Coverage";
		String indexContent = indexWriter.getHeader(indexTitle)
				+ indexWriter.getBodyContent(indexTitle, indexRowsHeader + indexRowsSb.toString());
		FileUtil.fileWrite(reportFolder, "index.html", indexContent);
		consoleWrite(classes.getSize() + " classes generated, see index.html at reports folder");
	}
	
	private String getClassCoverage(QueryCollection queries, HistoryReader history, ClassHtmlWriter writer, String sourceFolders, String projectFolders) {
		// first makes groups by line number, each line can have zero, one or more queries
		SourceCodeCollection lineCollection = new SourceCodeCollection();
		lineCollection.addQueries(queries); // groups by line number
		
		// locates the source code for this class (requires the source folders as parameter)
		if (!JavaCs.isEmpty(sourceFolders) && queries.getSize() > 0) // it is assumed that all queries are in the same class file
			lineCollection.addSources(queries.getItem(0), sourceFolders, projectFolders);
		
		StringBuilder csb = new StringBuilder();
		Map<Integer, SourceCodeLine> lines = lineCollection.getLines();
		for (int lineNumber : lines.keySet()) { // NOSONAR for net compatibility
			SourceCodeLine lineContent = lines.get(lineNumber);
			if (lineContent.getQueries().size() == 0) { // NOSONAR for net compatibility
				// only the source code if there are no queries
				csb.append(writer.getLineWithoutCoverage(lineNumber, lineContent.getSource()));
			} else {
				// Coverage and source Code of the line, or method if no line is available
				QueryReader query0 = lineContent.getQueries().get(0);
				csb.append(writer.getLineContent(lineNumber, 
					getLineCoverage(lineContent, writer),
					query0.getKey().getMethodName(), 
					lineContent.getSource()));
			}
			
			// Each query (if any) and details of their rules and history
			for (QueryReader thisQuery : lineContent.getQueries()) {
				String queryCoverage = getQueryCoverage(thisQuery.getModel(), lineContent, writer);
				HistoryReader queryHistory = history.getHistoryAtQuery(thisQuery.getKey());
				csb.append(writer.getQueryContent(thisQuery, queryCoverage, queryHistory));

				StringBuilder rsb = new StringBuilder();
				for (int k = 0; k < thisQuery.getModel().getRules().size(); k++)
					rsb.append(writer.getRuleContent(thisQuery.getModel().getRules().get(k)));
				csb.append(writer.getRulesContent(rsb.toString()));
			}
		}
		return csb.toString();
	}
	private String getLineCoverage(SourceCodeLine line, ClassHtmlWriter writer) {
		// If only a query, returns its coverage, if more, returns the aggregate
		// coverage of all queries
		// If there is no queries returns empty
		if (line.getQueries().size() == 1)
			return writer.coverage(line.getQueries().get(0).getModel().getDead(), line.getQueries().get(0).getModel().getCount());
		else if (line.getQueries().size() > 1)
			return writer.coverage(line.getDead(), line.getCount());
		return "";
	}
	private String getQueryCoverage(QueryModel query, SourceCodeLine line, ClassHtmlWriter writer) {
		// If more than a query returns the query coverage, if not, empty (coverage
		// should be shown in the line)
		if (line.getQueries().size() > 1)
			return writer.coverage(query.getDead(), query.getCount());
		return "";
	}

}
