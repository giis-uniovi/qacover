package giis.qacover.driver;
import java.lang.reflect.Method;
import java.sql.Connection;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.p6spy.engine.common.PreparedStatementInformation;
import com.p6spy.engine.common.StatementInformation;
import com.p6spy.engine.common.Value;
import com.p6spy.engine.spy.P6SpyOptions;

import giis.qacover.core.*;
import giis.qacover.portable.QaCoverException;

/**
 * Adapter of StatementInformation to the QueryStatement that must be sent to
 * the controller
 */
public class StatementAdapter extends QueryStatement {
	private static final Logger log = LoggerFactory.getLogger(StatementAdapter.class);
	private StatementInformation p6stmt;

	public StatementAdapter(PreparedStatementInformation statementInformation) {
		log.debug("*** StatementAdapter.new StatementAdapter(PreparedStatementInformation statementInformation)");
		try {
			if (super.getFaultInjector() != null && super.getFaultInjector().isUnexpectedException())
				throw new QaCoverException(super.getFaultInjector().getUnexpectedException());

			p6stmt = statementInformation;
			super.sql = p6stmt.getSql().trim();
			Map<Integer, Value> parameterValues = invokeMethod(statementInformation);
			
			// Converts the parameters used by the p6spy QueryStatement into strings in the form ?n? (n=1,2...)
			// that allows easy replacement in the coverage rules.
			// Also accepts named jdbc parameters (e.g. a comment before slect like /*params=?1?,?1?,?2?*/
			// indicates that first and second parameter have the same value
			List<String> namedParameters = parseNamedParameters(sql);

			// Maps the parameter names to the values.
			// Performs the same conversion to string that used by p6spy in PreparedStatementInformation.java
			// where string literals have already the quotes
			int parameterCount = parameterValues.keySet().size();
			for (Integer i : parameterValues.keySet()) {
				Value value = parameterValues.get(i);
				// if there exist a parameter with name uses it, if not sets as ?n? where n is
				// secuential beginning with 1
				String name = "?" + (i + 1) + "?"; // default value
				if (namedParameters.size() >= parameterCount)
					name = namedParameters.get(i);
				// Check possible inconsistencies if different values are given to the same parameter
				if (parameters.containsKey(name) && !parameters.get(name).equals(value.toString())) {
					String message = "StatementAdapter: Parameter " + name + " had been assigned to "
							+ parameters.get(name) + ". Can't be assigned to a new value " + value.toString();
					log.error("*** StatementAdapter.new StatementAdapter(PreparedStatementInformation statementInformation) checking repeated parameters"
							+ message);
					throw new QaCoverException(message);
				}
				if (value == null)
					value = new Value();
				super.parameters.put(name, value.toString(), value.getValue());
			}
		} catch (Exception e) {
			log.error( "*** StatementAdapter.new StatementAdapter(PreparedStatementInformation statementInformation)", e);
			this.exception = e;
		}
	}
	@SuppressWarnings("unchecked")
	private Map<Integer, Value> invokeMethod(PreparedStatementInformation statementInformation) {
		// The access of parameters in PreparedStatementInformation is readonly
		// Uses reflection to invoke the method to get the parameters
		// http://tutorials.jenkov.com/java-reflection/private-fields-and-methods.html  
		Map<Integer, Value> parameterValues = new HashMap<Integer, Value>();
		Method protectedMethod;
		try {
			log.debug("#### Getting p6spy parameters ");
			log.debug("#### Call P6SpyOptions.getActiveInstance().getDatabaseDialectDateFormat(), return: "
					+ P6SpyOptions.getActiveInstance().getDatabaseDialectDateFormat());
			log.debug("#### Call P6SpyOptions.getActiveInstance().getDatabaseDialectBooleanFormat(), return: "
					+ P6SpyOptions.getActiveInstance().getDatabaseDialectBooleanFormat());
			protectedMethod = PreparedStatementInformation.class.getDeclaredMethod("getParameterValues", (Class<?>[]) null);
			protectedMethod.setAccessible(true); // NOSONAR
			parameterValues = (Map<Integer, Value>) protectedMethod.invoke(statementInformation, (Object[]) null);
			log.debug("#### p6spy parameter values: " + parameterValues.toString());
			log.debug("#### Override database dialect format");
		} catch (Exception e) {
			log.error("*** StatementAdapter.new StatementAdapter(PreparedStatementInformation statementInformation) getting parameters", e);
			throw new QaCoverException("StatementAdapter.new", e);
		}
		return parameterValues;
	}

	public StatementAdapter(StatementInformation statementInformation) {
		log.debug("*** SpyStatementAdapter.new SpyStatementAdapter(StatementInformation statementInformation)");
		try {
			if (super.getFaultInjector()!=null && super.getFaultInjector().isUnexpectedException())
				throw new QaCoverException(super.getFaultInjector().getUnexpectedException());
			
			p6stmt=statementInformation;
			super.sql=p6stmt.getSql();
		} catch (Exception e) {
			log.error("*** SpyStatementAdapter.new SpyStatementAdapter(StatementInformation statementInformation)", e);
			this.exception=e;
		}
	}
	@Override
	protected String getDatabaseDialectFormat() {
		return P6SpyOptions.getActiveInstance().getDatabaseDialectDateFormat();
	}
	
	@Override 
	public Connection getConnection() {
		log.debug("*** SpyStatementAdapter.getConnection()");
		return p6stmt.getConnectionInformation().getConnection();
	}
	
}
