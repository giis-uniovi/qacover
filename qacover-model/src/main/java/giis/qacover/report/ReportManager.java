package giis.qacover.report;

import java.util.Map;

import giis.portable.util.FileUtil;
import giis.qacover.model.QueryModel;
import giis.qacover.model.Variability;
import giis.qacover.reader.CoverageCollection;
import giis.qacover.reader.CoverageReader;
import giis.qacover.reader.CoverageSummary;
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
		consoleWrite("QACover version " + new Variability().getVersion());
		consoleWrite("Rules folder: " + FileUtil.getFullPath(rulesFolder));
		consoleWrite("Report folder: " + FileUtil.getFullPath(reportFolder));
		FileUtil.createDirectory(reportFolder);

		// Report index
		IndextHtmlWriter indexWriter = new IndextHtmlWriter();

		// Using a CoverageReader to access the store by class and then by query
		CoverageCollection classes = new CoverageReader(rulesFolder).getByClass();
		StringBuilder indexRowsSb = new StringBuilder();

		for (int i = 0; i < classes.size(); i++) {
			QueryCollection query = classes.get(i);
			// a line of the index report
			String className = query.getName();
			CoverageSummary summary = query.getSummary();
			consoleWrite("Report for class: " + className + " " + summary.toString());
			indexRowsSb.append(indexWriter.getBodyRow(className, 
					summary.getQrun(), summary.getQcount(), summary.getQerror(),
					summary.getCount(), summary.getDead(), summary.getError()));

			// Generates a file for this class
			ClassHtmlWriter classWriter = new ClassHtmlWriter();
			String htmlCoverage = getClassCoverage(query, classWriter);
			String htmlCoverageContent = classWriter.getHeader(className) 
					+ classWriter.getBodyContent(className, htmlCoverage);
			FileUtil.fileWrite(reportFolder, className + ".html", htmlCoverageContent);
		}
		
		// Puts everything, with totals as first line
		CoverageSummary totals = classes.getSummary();
		String indexRowsHeader = indexWriter.getBodyRow("TOTAL", totals.getQrun(), totals.getQcount(), totals.getQerror(), totals.getCount(),
				totals.getDead(), totals.getError());
		String indexContent = indexWriter.getHeader("SQL Query Fpc Coverage")
				+ indexWriter.getBodyContent("SQL Query Fpc Coverage", indexRowsHeader + indexRowsSb.toString());
		FileUtil.fileWrite(reportFolder, "index.html", indexContent);
		consoleWrite(classes.size() + " classes generated, see index.html at reports folder");
	}
	
	private String getClassCoverage(QueryCollection queries, ClassHtmlWriter writer) {
		// first makes groups by line number, each line can have zero, one or more queries
		SourceCodeCollection lineCollection = new SourceCodeCollection();
		lineCollection.addQueries(queries); // groups by line number
		
		StringBuilder csb = new StringBuilder();
		for (Map.Entry<Integer, SourceCodeLine> line : lineCollection.getLines().entrySet()) {
			// Coverage and source Code of the line, or method if no line is available
			QueryReader query0 = line.getValue().getQueries().get(0);
			csb.append(writer.getLineContent(line.getKey(), 
					getLineCoverage(line.getValue(), writer),
					query0.getKey().getMethodName(), 
					line.getValue().getSource()));

			// Each query (if any) and details of their rules
			for (QueryReader thisQuery : line.getValue().getQueries()) {
				String queryCoverage = getQueryCoverage(thisQuery.getModel(), line.getValue(), writer);
				csb.append(writer.getQueryContent(thisQuery, queryCoverage));

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
