package giis.qacover.core.services;

import static giis.tdrules.model.shared.ModelUtil.safe;

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
import giis.tdrules.model.io.TdRulesXmlSerializer;
import giis.tdrules.openapi.model.QueryEntitiesBody;
import giis.tdrules.openapi.model.QueryParam;
import giis.tdrules.openapi.model.QueryParametersBody;
import giis.tdrules.openapi.model.TdRules;
import giis.tdrules.openapi.model.TdRulesBody;
import giis.tdrules.openapi.model.TdSchema;

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
		String uri = Configuration.getInstance().getRuleServiceUrl();
		String cacheLocation = Configuration.getInstance().getRuleCacheFolder();
		log.debug("Call service: {}, using cache: {}", uri, cacheLocation);
		TdRulesApi api = new TdRulesApi(uri);
		if (!"".equals(cacheLocation))
			api.setCache(cacheLocation);
		return api;
	}

	public QueryModel getFpcRulesModel(String sql, SchemaModel schema, String fpcOptions) {
		this.setErrorContext("Generate SQLFpc coverage rules");
		TdRules model = getApi().getRules(schema.getModel(), sql, fpcOptions);
		injectFaultIfNeeded(model);
		if (!"".equals(model.getError()))
			throw new QaCoverException(model.getError());
		return new QueryModel(model);
	}

	public QueryModel getMutationRulesModel(String sql, SchemaModel schema, String fpcOptions) {
		this.setErrorContext("Generate SQLMutation coverage rules");
		TdRules model = getApi().getMutants(schema.getModel(), sql, fpcOptions);
		injectFaultIfNeeded(model);
		if (!"".equals(model.getError()))
			throw new QaCoverException(model.getError());
		return new QueryModel(model);
	}

	public String getRulesInput(String sql, SchemaModel schemaModel, String fpcOptions) {
		TdRulesBody body = new TdRulesBody();
		body.setQuery(sql);
		body.setSchema(schemaModel.getModel());
		body.setOptions(fpcOptions);
		return new TdRulesXmlSerializer().serialize(body);
	}

	public String[] getAllTableNames(String sql, String storetype) {
		this.setErrorContext("Get query table names");
		QueryEntitiesBody model = getApi().getEntities(sql, storetype);
		injectFaultIfNeeded(model);
		if (!"".equals(model.getError()))
			throw new QaCoverException(model.getError());
		return JavaCs.toArray(safe(model.getEntities()));
	}

	public QueryWithParameters inferQueryWithParameters(String sql, String storetype) {
		this.setErrorContext("Infer query parameters");
		QueryParametersBody model = getApi().getParameters(sql, storetype);
		injectFaultIfNeeded(model);
		if (!"".equals(model.getError()))
			throw new QaCoverException(model.getError());
		// determines the QueryStatement parameters
		QueryWithParameters ret = new QueryWithParameters();
		ret.setSql(model.getParsedquery());
		for (QueryParam param : safe(model.getParameters()))
			ret.putParam(param.getName(), param.getValue());
		return ret;
	}

	public SchemaModel getSchemaModel(Connection conn, String catalog, String schema, String[] tables) {
		this.setErrorContext("Get database schema");
		DbSchemaApi api = new DbSchemaApi(conn).setCatalogAndSchema(catalog, schema);
		TdSchema model = api.getSchema(JavaCs.toList(tables));
		return new SchemaModel(model);
	}

	// Fault injection when needed (only for testing)
	private void injectFaultIfNeeded(TdRules model) { // NOSONAR false positive, method overloaded
		if (faultInjector != null && faultInjector.isRulesFaulty()) {
			model.setError(faultInjector.getRulesFault());
			model.setRules(null);
		}
	}
	private void injectFaultIfNeeded(QueryEntitiesBody model) { // NOSONAR false positive, method overloaded
		if (faultInjector != null && faultInjector.isTablesFaulty()) {
			model.setError(faultInjector.getTablesFault());
			model.setEntities(null);
		}
	}
	private void injectFaultIfNeeded(QueryParametersBody model) { // NOSONAR false positive, method overloaded
		if (faultInjector != null && faultInjector.isInferFaulty()) {
			model.setError(faultInjector.getInferFault());
			model.setParameters(null);
		}
	}

}
