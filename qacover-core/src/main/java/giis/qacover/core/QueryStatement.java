package giis.qacover.core;

import java.util.ArrayList;
import java.util.List;

import giis.portable.util.JavaCs;
import giis.qacover.core.query.AbstractQueryStatement;
import giis.qacover.core.services.FaultInjector;
import giis.qacover.core.services.RuleServices;
import giis.qacover.model.QueryWithParameters;

/**
 * Specialization of the abstract query statement to manage queries that are obtained from the interception of
 * the execution in a given program.
 * 
 * On Java is created from a p6spy adapter when executed in response to the onBeforeExecuteQuery event.
 * 
 * Provides additional functionality to manage parameters and keep track of the variability and fault
 * injection for testing.
 */
public abstract class QueryStatement extends AbstractQueryStatement {
	private static FaultInjector faultInjector = null;

	/**
	 * If the query does not have parameters, transforms this object as a result 
	 * of the parameters inference on the query
	 */
	public void inferParameters(RuleServices svc, String storetype) {
		if (getParameters().getSize() > 0)
			return;
		QueryWithParameters queryAndParam = svc.inferQueryWithParameters(sql, storetype);
		sql = queryAndParam.getSql(); // este sql sera el que tenga parametros y se usara a partir de ahora
		parameters = queryAndParam.getParams();
	}

	/**
	 * Parses a comment anotation to allow named jdbc parameters (by encolsing a starting comment in the query).
	 * This allows to unify parameters at different locations in a query into a single parameter
	 */
	protected List<String> parseNamedParameters(String sql) {
		String comment = sql;
		List<String> spec = new ArrayList<String>();
		// comment before starting teh query
		if (comment.startsWith("/*"))
			comment = JavaCs.substring(comment, 2, comment.length());
		else
			return spec;
		if (comment.indexOf("*/") > -1)
			comment = JavaCs.substring(comment, 0, comment.indexOf("*/"));
		else
			return spec;
		// search the parameters, eg /* params=?a?,?b? */
		String[] components = JavaCs.splitByChar(comment.trim(), '=');
		if (components.length != 2)
			return spec;
		if (!"params".equals(components[0].trim()))
			return spec;
		String[] params = JavaCs.splitByChar(components[1].trim(), ',');
		for (int i = 0; params.length > i; i++)
			spec.add(params[i].trim());
		return spec;
	}

	/**
	 * Redefines parameter replacement to patch the behaviour in special cases
	 */
	@Override
	protected String replaceSingleParameter(String sourceSql, String name, String value) {
		if (variant.isOracle() && parameters.isDate(name)) {
			// Patch: GitLab #11 Excepcion ejecutando regla cuando un parametro de tipo fecha está
			// dentro de una funcion to_char (Oracle ORA-01722)
			// Localiza la expresion to_char (?XX? y si existe en vez de poner el parametro,
			// pone una coversion to_date
			// De esta forma la sql se encontrara con una fecha como argumento a to_char evitando el problema
			// Es un poco chapuza y dependiente de como se escribe la sql, pero como
			// proviene de una regla, siempre hay los espacios alrededor del parentesis
			String searchStr = "to_char(" + name;
			if (sourceSql.contains(searchStr) || sourceSql.contains(searchStr.toUpperCase())) {
				String dateFormat = getDatabaseDialectFormat();
				String replaceStr = "TO_CHAR(TO_DATE(" + name + ",'" + dateFormat + "')";
				sourceSql = sourceSql.replace(searchStr, replaceStr);
				sourceSql = sourceSql.replace(searchStr.toUpperCase(), replaceStr);
			}
		}
		return sourceSql.replace(name, value);
	}

	// Needed by the above patch, only for p6spy
	protected abstract String getDatabaseDialectFormat();

	public static void setFaultInjector(FaultInjector injector) {
		faultInjector = injector;
	}
	public FaultInjector getFaultInjector() {
		return faultInjector;
	}

}
