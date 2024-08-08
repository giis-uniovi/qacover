package giis.qacover.model;

import java.util.ArrayList;
import java.util.Date;
import java.util.List;

import giis.portable.util.JavaCs;
import giis.portable.xml.tiny.XNode;
import giis.portable.xml.tiny.XNodeAbstract;

/**
 * The history model holds secuentially the reference to each query evaluation:
 * (1) datetime
 * (2) QueryKey
 * (3) Parameters used in the evaluation
 * 
 * The string representation of each item is a csv with | as separator
 */
public class HistoryModel {
	private HistoryDao dao;

	/**
	 * Creates an history model from a query that has been evaluated with the given parameters
	 */
	public HistoryModel(Date timestamp, String key, QueryParameters params) {
		this.dao = new HistoryDao();
		this.dao.at = JavaCs.getIsoDate(timestamp);
		this.dao.key = key;
		this.dao.params = params.toDao();
	}

	/**
	 * Create an history model from a string read from the storage
	 */
	public HistoryModel(String item) {
		if (item.startsWith("{"))
			throw new RuntimeException("V2 history still not implemented");
		else
			loadHistoryItemV1(item);
	}
	
	private void loadHistoryItemV1(String item) {
		String[] splitted = JavaCs.splitByBar(item); // should have 3 at least
		this.dao = new HistoryDao();
		this.dao.at = JavaCs.getIsoDate(JavaCs.parseIsoDate(splitted[0]));
		this.dao.key = splitted[1];
		// If more than 3, joins the remaining items
		String paramStr = "";
		for (int i = 2; i < splitted.length; i++)
			paramStr += (i == 2 ? "" : "|") + splitted[i]; // NOSONAR
		this.dao.params = paramsFromXml(paramStr);
	}

	public String getTimestampString() {
		return dao.at;
	}
	public String getKey() {
		return dao.key;
	}
	public String getParamsXml() {
		return paramsToXml(dao.params);
	}
	
	private String paramsToXml(List<ParameterDao> params) {
		StringBuilder sb = new StringBuilder();
		for (ParameterDao param : params)
			sb.append("<parameter name=\"")
					.append(XNodeAbstract.encodeAttribute(param.name)).append("\" value=\"")
					.append(XNodeAbstract.encodeAttribute(param.value)).append("\" />");
		return "<parameters>" + sb.toString() + "</parameters>";
	}

	private List<ParameterDao> paramsFromXml(String paramXml) {
		List<ParameterDao> dao = new ArrayList<>();
		List<XNode> paramNodes = new XNode(paramXml).getChildren("parameter");
		for (XNode paramNode : paramNodes)
			dao.add(new ParameterDao(XNodeAbstract.decodeAttribute(paramNode.getAttribute("name")),
					XNodeAbstract.decodeAttribute(paramNode.getAttribute("value"))));
		return dao;
	}

	public String toStringV1() {
		return getTimestampString() + "|" + getKey() + "|" + getParamsXml();
	}

}
