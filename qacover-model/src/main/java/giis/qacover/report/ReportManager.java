package giis.qacover.report;

import giis.portable.util.FileUtil;
import giis.qacover.model.Variability;
import giis.qacover.reader.CoverageCollection;
import giis.qacover.reader.CoverageReader;
import giis.qacover.reader.CoverageSummary;
import giis.qacover.reader.QueryCollection;
import giis.qacover.reader.QueryReader;

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
	
	private String getClassCoverage(QueryCollection query, ClassHtmlWriter writer) {
		StringBuilder csb=new StringBuilder();
		for (int j = 0; j < query.size(); j++) {
			QueryReader thisQuery=query.get(j);
			csb.append(writer.getLineContent(thisQuery))
				.append(writer.getQueryContent(query.get(j)));
			
			StringBuilder rsb=new StringBuilder();
			for (int k=0; k<thisQuery.getModel().getRules().size(); k++)
				rsb.append(writer.getRuleContent(thisQuery.getModel().getRules().get(k)));
			csb.append(writer.getRulesContent(rsb.toString()));
		}
		return csb.toString();
	}

}
