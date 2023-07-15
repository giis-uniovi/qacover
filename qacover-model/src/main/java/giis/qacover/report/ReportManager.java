package giis.qacover.report;

import giis.portable.util.FileUtil;
import giis.qacover.model.QueryModel;
import giis.qacover.model.Variability;
import giis.qacover.reader.CoverageCollection;
import giis.qacover.reader.CoverageReader;
import giis.qacover.reader.CoverageSummary;
import giis.qacover.reader.QueryCollection;

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
		IndextHtmlWriter ihw = new IndextHtmlWriter();
		StringBuilder isb = new StringBuilder();
		isb.append(ihw.getHeader());

		// Using a CoverageReader to access the store by class and then by query
		CoverageCollection classes = new CoverageReader(rulesFolder).getByClass();
		StringBuilder ibodysb = new StringBuilder();

		for (int i = 0; i < classes.size(); i++) {
			QueryCollection query = classes.get(i);
			String className = query.getName();
			CoverageSummary summary = query.getSummary();
			consoleWrite("Report for class: " + className + " " + summary.toString());
			ibodysb.append(ihw.getBody(className, summary.getQrun(), summary.getQcount(), summary.getQerror(),
					summary.getCount(), summary.getDead(), summary.getError()));

			// Generates a file for this class
			ClassHtmlWriter writer = new ClassHtmlWriter();
			StringBuilder qsb = new StringBuilder();
			qsb.append(writer.getHeader(className));
			// Includes each query
			for (int j = 0; j < query.size(); j++) {
				String methodIdentifier = query.get(j).getKey().getMethodName(true);
				QueryModel rules = query.get(j).getModel();
				qsb.append(writer.getQueryBody(rules, methodIdentifier, "query" + j));
			}
			qsb.append(writer.getFooter());
			FileUtil.fileWrite(reportFolder, className + ".html", qsb.toString());
		}
		
		// Puts everything, with totals as first line
		CoverageSummary totals = classes.getSummary();
		isb.append(ihw.getBody("TOTAL", totals.getQrun(), totals.getQcount(), totals.getQerror(), totals.getCount(),
				totals.getDead(), totals.getError()));
		isb.append(ibodysb.toString());
		isb.append(ihw.getFooter());
		FileUtil.fileWrite(reportFolder, "index.html", isb.toString());
		consoleWrite(classes.size() + " classes generated, see index.html at reports folder");
	}

}
