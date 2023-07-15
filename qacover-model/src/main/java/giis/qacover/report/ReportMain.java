package giis.qacover.report;

/**
 * When packaged in an executable jar, this is the entry point to generate the
 * report from the commandline with two args: rules folder and reports folder
 */
public class ReportMain {

	public static void main(String[] args) {
		if (args.length == 2) {
			new ReportManager().run(args[0], args[1]);
		} else {
			ReportManager.consoleWrite("Two parameters are required: rulesFolder reportFolder");
		}
	}

}
