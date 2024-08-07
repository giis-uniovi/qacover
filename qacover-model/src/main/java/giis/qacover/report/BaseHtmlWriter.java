package giis.qacover.report;

import giis.portable.xml.tiny.XNodeAbstract;
import giis.qacover.model.Variability;

public abstract class BaseHtmlWriter {

	protected String getHeader(String title) {
		return "<!doctype html>\n"
				+ "<html lang='en'>\n"
				+ "<head>\n"
				+ "<title>" + title + "</title>\n"
				+ getCommonHeaders()
				+ getStyles()
				+ getScripts()
				+ "</head>\n\n";
	}
	
	protected String getCommonHeaders() {
		return "<meta charset='utf-8'>\n"
				+ "<meta name='viewport' content='width=device-width, initial-scale=1, shrink-to-fit=no'>\n"
				+ "<link rel='stylesheet' href='https://stackpath.bootstrapcdn.com/bootstrap/4.5.0/css/bootstrap.min.css' integrity='sha384-9aIt2nRpC12Uk9gS9baDl411NQApFmC26EwAOH8WgZl5MYYxFfc+NcPb1dKGj7Sk' crossorigin='anonymous'>\n"
				+ "<script src='https://code.jquery.com/jquery-3.5.1.slim.min.js' integrity='sha384-DfXdz2htPH0lsSSs5nCTpuj/zy4C+OGpamoFVy38MVBnE+IbbVYUew+OrCXaRkfj' crossorigin='anonymous'></script>\n"
				+ "<script src='https://cdn.jsdelivr.net/npm/popper.js@1.16.0/dist/umd/popper.min.js' integrity='sha384-Q6E9RHvbIyZFJoft+2mJbHaEWldlvI9IOYy5n3zV9zzTtmI3UksdQRVvoxMfooAo' crossorigin='anonymous'></script>\n"
				+ "<script src='https://stackpath.bootstrapcdn.com/bootstrap/4.5.0/js/bootstrap.min.js' integrity='sha384-OgVRvuATP1z7JjHLkuOU7Xw704+h835Lr+6QL9UvYjZE3Ipu6Tp75j7Bh/kR0JKI' crossorigin='anonymous'></script>\n";
	}
	
	protected abstract String getScripts();
	
	protected abstract String getStyles();

	protected String getFooter() {
		Variability variab=new Variability();
		return "\n<div><span style=\"float:right;\">"
				+ "<small>\n"
				+ "Generated by: QACover " + variab.getPlatformName()
				+ " - [version " + variab.getVersion() + "]"
				+ " - <a href=\"" + variab.getServiceDeskAddress() + "\">[Submit a problem report]</a>\n"
				+ "</small></span></div>\n";
	}
	
	protected String encode(String text) {
		return XNodeAbstract.encodeText(text);
	}

	public String coverage(int dead, int count) {
		return "<strong>" + percent(count, dead) + "</strong>&nbsp;(" + dead + "/" + count + ")";
	}
	
	public static String percent(int count, int dead) {
		return count == 0 ? "" : String.valueOf((dead * 100) / count) + "%";
	}

}