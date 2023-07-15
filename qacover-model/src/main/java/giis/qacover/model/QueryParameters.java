package giis.qacover.model;

import java.util.ArrayList;
import java.util.Date;
import java.util.List;
import java.util.Map;
import java.util.TreeMap;

import giis.portable.xml.tiny.XNodeAbstract;

/**
 * Map of parameter-value of a query that is being evaluated. Internally stores
 * two maps that differ on the value: 
 * (1) value as string, the same that p2spy provides (already with quotes in string types)
 * (2) object to manage specific features that depend on the datatype
 */
public class QueryParameters {
	protected Map<String, String> parameters = new TreeMap<String, String>(); 
	protected Map<String, Object> parameterObjects = new TreeMap<String, Object>();

	public int size() {
		return parameters.size();
	}

	public boolean containsKey(String name) {
		return parameters.containsKey(name);
	}

	@Override
	public String toString() {
		StringBuilder sb = new StringBuilder();
		List<String> keys = this.keySet();
		for (int i = 0; i < keys.size(); i++)
			sb.append(i == 0 ? "" : ", ").append(keys.get(i)).append("=").append(parameters.get(keys.get(i)));
		return "{" + sb.toString() + "}";
	}

	public String toXml() {
		StringBuilder sb = new StringBuilder();
		List<String> keys = this.keySet();
		for (String key : keys)
			sb.append("<parameter name=\"").append(XNodeAbstract.encodeAttribute(key))
				.append("\" value=\"").append(XNodeAbstract.encodeAttribute(parameters.get(key)))
				.append("\" />");
		return "<parameters>" + sb.toString() + "</parameters>";
	}

	public void put(String name, String valueString, Object valueObject) {
		parameters.put(name, valueString);
		parameterObjects.put(name, valueObject);
	}

	public void put(String name, String valueString) {
		parameters.put(name, valueString);
		parameterObjects.put(name, valueString);
	}

	public String get(String name) {
		return parameters.get(name);
	}

	public List<String> keySet() {
		List<String> keys = new ArrayList<String>();
		for (String name : parameters.keySet())
			keys.add(name);
		return keys;
	}

	public boolean isDate(String name) {
		return parameterObjects.containsKey(name) && parameterObjects.get(name) instanceof Date;
	}
}
