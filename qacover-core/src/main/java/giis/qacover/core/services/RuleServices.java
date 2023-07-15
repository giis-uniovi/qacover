package giis.qacover.core.services;

import static giis.tdrules.model.ModelUtil.safe;

import java.sql.Connection;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import giis.portable.util.JavaCs;
import giis.qacover.model.QueryModel;
import giis.qacover.model.QueryWithParameters;
import giis.qacover.model.SchemaModel;
import giis.qacover.portable.QaCoverException;
import giis.tdrules.client.TdRulesApi;
import giis.tdrules.client.rdb.DbSchemaApi;
import giis.tdrules.model.io.SqlRulesXmlSerializer;
import giis.tdrules.openapi.model.DbSchema;
import giis.tdrules.openapi.model.SqlParam;
import giis.tdrules.openapi.model.SqlParametersBody;
import giis.tdrules.openapi.model.SqlRules;
import giis.tdrules.openapi.model.SqlRulesBody;
import giis.tdrules.openapi.model.SqlTableListBody;

/**
 * Operations to get coverage rules and schemas
 */
public class RuleServices {
	private static final Logger log = LoggerFactory.getLogger(RuleServices.class);
	private String errorContext = ""; // to give clues on where an exception is raised
	private FaultInjector faultInjector;

	public RuleServices(FaultInjector injector) {
		faultInjector = injector;
	}

	public String getErrorContext() {
		return errorContext;
	}
	public void setErrorContext(String message) {
		errorContext = message;
	}

	private TdRulesApi getApi() {
		String uri = Configuration.getInstance().getFpcServiceUrl();
		log.debug("Call service: " + uri);
		return new TdRulesApi(uri);
	}

	public QueryModel getRulesModel(String sql, SchemaModel schema, String fpcOptions) {
		this.setErrorContext("Generate SQLFpc coverage rules");
		SqlRules model = getApi().getRules(schema.getModel(), sql, fpcOptions);
		injectFaultIfNeeded(model);
		if (!"".equals(model.getError()))
			throw new QaCoverException(model.getError());
		return new QueryModel(model);
	}

	public String getRulesInput(String sql, SchemaModel schemaModel, String fpcOptions) {
		SqlRulesBody body = new SqlRulesBody();
		body.setSql(sql);
		body.setSchema(schemaModel.getModel());
		body.setOptions(fpcOptions);
		return new SqlRulesXmlSerializer().serialize(body);
	}

	public String[] getAllTableNames(String sql) {
		this.setErrorContext("Get query table names");
		SqlTableListBody model = getApi().sqlTablesPost(sql);
		injectFaultIfNeeded(model);
		if (!"".equals(model.getError()))
			throw new QaCoverException(model.getError());
		return JavaCs.toArray(safe(model.getTables()));
	}

	public QueryWithParameters inferQueryWithParameters(String sql) {
		this.setErrorContext("Infer query parameters");
		SqlParametersBody model = getApi().sqlParametersPost(sql);
		injectFaultIfNeeded(model);
		if (!"".equals(model.getError()))
			throw new QaCoverException(model.getError());
		// determines the QueryStatement parameters
		QueryWithParameters ret = new QueryWithParameters();
		ret.setSql(model.getParsedsql());
		for (SqlParam param : safe(model.getParameters()))
			ret.putParam(param.getName(), param.getValue());
		return ret;
	}

	public SchemaModel getSchemaModel(Connection conn, String catalog, String schema, String[] tables) {
		this.setErrorContext("Get database schema");
		DbSchemaApi api = new DbSchemaApi(conn).setCatalogAndSchema(catalog, schema);
		DbSchema model = api.getDbSchema(JavaCs.toList(tables));
		return new SchemaModel(model);
	}

	// Fault injection when needed (only for testing)
	private void injectFaultIfNeeded(SqlRules model) { // NOSONAR false positive, method overloaded
		if (faultInjector != null && faultInjector.isRulesFaulty()) {
			model.setError(faultInjector.getRulesFault());
			model.setRules(null);
		}
	}
	private void injectFaultIfNeeded(SqlTableListBody model) { // NOSONAR false positive, method overloaded
		if (faultInjector != null && faultInjector.isTablesFaulty()) {
			model.setError(faultInjector.getTablesFault());
			model.setTables(null);
		}
	}
	private void injectFaultIfNeeded(SqlParametersBody model) { // NOSONAR false positive, method overloaded
		if (faultInjector != null && faultInjector.isInferFaulty()) {
			model.setError(faultInjector.getInferFault());
			model.setParameters(null);
		}
	}

}
