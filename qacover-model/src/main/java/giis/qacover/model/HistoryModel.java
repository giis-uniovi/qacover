package giis.qacover.model;

import java.util.Date;

import giis.portable.util.JavaCs;

/**
 * The history model holds secuentially the reference to each query evaluation:
 * (1) datetime
 * (2) QueryKey
 * (3) Parameters used in the evaluation
 * 
 * The string representation of each item is a csv with | as separator
 */
public class HistoryModel {
	private Date timestamp;
	private String key = "";
	private String params = "";

	public HistoryModel(Date timestamp, String key, String params) {
		this.timestamp = timestamp;
		this.key = key;
		this.params = params;
	}

	public HistoryModel(String item) {
		String[] splitted = JavaCs.splitByBar(item); // should have 3 at least
		timestamp = JavaCs.parseIsoDate(splitted[0]);
		key = splitted[1];
		// If more than 3, joins the remaining items
		for (int i = 2; i < splitted.length; i++)
			params = getParams() + (i == 2 ? "" : "|") + splitted[i]; // NOSONAR
	}

	public Date getTimestamp() {
		return timestamp;
	}
	public String getTimestampString() {
		return JavaCs.getIsoDate(timestamp);
	}
	public String getKey() {
		return key;
	}
	public String getParams() {
		return params;
	}

	@Override
	public String toString() {
		return JavaCs.getIsoDate(getTimestamp()) + "|" + getKey() + "|" + getParams();
	}

}
