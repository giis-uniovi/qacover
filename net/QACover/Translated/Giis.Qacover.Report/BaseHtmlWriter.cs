/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using Giis.Portable.Xml.Tiny;
using Giis.Qacover.Model;


namespace Giis.Qacover.Report
{
	public abstract class BaseHtmlWriter
	{
		protected internal virtual string GetHeader(string title)
		{
			return "<!doctype html>\n" + "<html lang='en'>\n" + "<head>\n" + "<title>" + title + "</title>\n" + GetCommonHeaders() + GetStyles() + GetScripts() + "</head>\n\n";
		}

		protected internal virtual string GetCommonHeaders()
		{
			return "<meta charset='utf-8'>\n" + "<meta name='viewport' content='width=device-width, initial-scale=1, shrink-to-fit=no'>\n" + "<link rel='stylesheet' href='https://stackpath.bootstrapcdn.com/bootstrap/4.5.0/css/bootstrap.min.css' integrity='sha384-9aIt2nRpC12Uk9gS9baDl411NQApFmC26EwAOH8WgZl5MYYxFfc+NcPb1dKGj7Sk' crossorigin='anonymous'>\n"
				 + "<script src='https://code.jquery.com/jquery-3.5.1.slim.min.js' integrity='sha384-DfXdz2htPH0lsSSs5nCTpuj/zy4C+OGpamoFVy38MVBnE+IbbVYUew+OrCXaRkfj' crossorigin='anonymous'></script>\n" + "<script src='https://cdn.jsdelivr.net/npm/popper.js@1.16.0/dist/umd/popper.min.js' integrity='sha384-Q6E9RHvbIyZFJoft+2mJbHaEWldlvI9IOYy5n3zV9zzTtmI3UksdQRVvoxMfooAo' crossorigin='anonymous'></script>\n"
				 + "<script src='https://stackpath.bootstrapcdn.com/bootstrap/4.5.0/js/bootstrap.min.js' integrity='sha384-OgVRvuATP1z7JjHLkuOU7Xw704+h835Lr+6QL9UvYjZE3Ipu6Tp75j7Bh/kR0JKI' crossorigin='anonymous'></script>\n";
		}

		protected internal abstract string GetScripts();

		protected internal abstract string GetStyles();

		protected internal virtual string GetFooter()
		{
			Variability variab = new Variability();
			return "\n<div><span style=\"float:right;\">" + "<small>\n" + "Generated by: QACover " + variab.GetPlatformName() + " - [version " + variab.GetVersion() + "]" + " - <a href=\"" + variab.GetServiceDeskAddress() + "\">[Submit a problem report]</a>\n" + "</small></span></div>\n";
		}

		protected internal virtual string Encode(string text)
		{
			return XNodeAbstract.EncodeText(text);
		}

		public static string Percent(int count, int dead)
		{
			return count == 0 ? string.Empty : ((dead * 100) / count).ToString() + "%";
		}
	}
}
