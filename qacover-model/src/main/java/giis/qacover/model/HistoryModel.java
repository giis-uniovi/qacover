package giis.qacover.model;

import java.util.ArrayList;
import java.util.Date;
import java.util.List;

import giis.portable.util.JavaCs;
import giis.portable.xml.tiny.XNode;
import giis.portable.xml.tiny.XNodeAbstract;
import giis.tdrules.model.io.ModelJsonSerializer;

/**
 * The history store holds sequentially the reference to each query evaluation;
 * Each query evaluation is represented in an instance of this class that
 * is instantiated from a line read from the history store.
 * 
 * There are two formats that can be read:
 * V1 (legacy): a csv separated with a vertical bar: timestamp | query key | parameters in xml
 * V2 (current): a json string with timestam, query key and parameters
 * 
 * Currently QACover writes in V2 format, but V1 is keep for compatibility
 * with legacy stores created before QACover version 2.0.0
 */
public class HistoryModel {
	private HistoryDao dao; // data is kept in the dao for easier serialization

	/**
	 * Creates an history model with the given parameters from a query that has been evaluated
	 */
	public HistoryModel(Date timestamp, String key, QueryParameters params) {
		this.dao = new HistoryDao();
		this.dao.at = JavaCs.getIsoDate(timestamp);
		this.dao.key = key;
		this.dao.params = params.toDao();
	}

	/**
	 * Create an history model from a string that is read from the history storage
	 * (supports V1 and V2 formats)
	 */
	public HistoryModel(String item) {
		if (item.startsWith("{"))
			loadHistoryItemV2(item);
		else
			loadHistoryItemV1(item);
	}
	
	private void loadHistoryItemV2(String item) {
		this.dao = (HistoryDao) new ModelJsonSerializer().deserialize(item, HistoryDao.class);
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

	public String getKey() {
		return dao.key;
	}
	public List<ParameterDao> getParams() {
		return dao.params;
	}
	
	public String getParamsJson() {
		return new ModelJsonSerializer().serialize(dao.params, false);
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

	public String toStringV2() {
		return new ModelJsonSerializer().serialize(dao, false);
	}

	public String toStringV1() {
		return dao.at + "|" + dao.key + "|" + getParamsXml();
	}
	
}
