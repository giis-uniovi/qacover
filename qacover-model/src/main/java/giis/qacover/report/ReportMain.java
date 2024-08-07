package giis.qacover.report;

/**
 * When packaged in an executable jar, this is the entry point to generate the
 * report from the commandline with two args: rules folder and reports folder
 */
public class ReportMain {

	public static void main(String[] args) {
		if (args.length == 2) {
			new ReportManager().run(args[0], args[1]);
		} else if (args.length == 3) {
			new ReportManager().run(args[0], args[1], args[2], "");
		} else if (args.length == 4) {
			new ReportManager().run(args[0], args[1], args[2], args[3]);
		} else {
			ReportManager.consoleWrite("At least two parameters are required: rulesFolder reportFolder [sourceFolders [projecFolder]]");
		}
	}

}
