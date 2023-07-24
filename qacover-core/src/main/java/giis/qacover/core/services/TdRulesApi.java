package giis.qacover.core.services;

import giis.tdrules.openapi.model.DbSchema;
import giis.tdrules.openapi.model.SqlParametersBody;
import giis.tdrules.openapi.model.SqlRules;
import giis.tdrules.openapi.model.SqlRulesBody;
import giis.tdrules.openapi.model.SqlTableListBody;

/**
 * Temporal implementation of the client api, to be moved to tdrules-client
 */
public class TdRulesApi extends giis.tdrules.client.TdRulesApi {
	
	private boolean useCache;
	private String cacheLocation;
	
	public TdRulesApi(String uri) {
		super(uri);
		this.cacheLocation = Configuration.getInstance().getCacheRulesLocation();
		this.useCache = !"".equals(coalesce(cacheLocation));
	}
	
	/**
	 * Gets the fpc rules for a query executed under the specified schema
	 */
	@Override
	public SqlRules getRules(DbSchema schema, String query, String options) {
		SqlRulesBody request=new SqlRulesBody();
		request.setSchema(schema);
		request.setSql(query);
		request.setOptions(coalesce(options));
		TdRulesCache cache = getCache("rulesPost", request);

		if (useCache() && cache.hit())
			return (SqlRules)cache.getPayload(SqlRules.class);
		SqlRules result = super.rulesPost(request);
		if (useCache())
			cache.putPayload(result);
		return result;
	}

	public SqlTableListBody getTables(String sql) {
		TdRulesCache cache = getCache("sqlTablesPost", sql);
		if (useCache() && cache.hit())
			return (SqlTableListBody)cache.getPayload(SqlTableListBody.class);
		SqlTableListBody result = super.sqlTablesPost(sql);
		if (useCache())
			cache.putPayload(result);
		return result;
	}
	
	public SqlParametersBody getParameters(String sql) {
		TdRulesCache cache = getCache("sqlParametersPost", sql);
		if (useCache() && cache.hit())
			return (SqlParametersBody)cache.getPayload(SqlParametersBody.class);
		SqlParametersBody result = super.sqlParametersPost(sql);
		if (useCache())
			cache.putPayload(result);
		return result;
	}
	
	// Cache management
	
	private boolean useCache() {
		return this.useCache;
	}
	private TdRulesCache getCache(String endpoint, Object request) {
		return useCache() ? new TdRulesCache(this.cacheLocation, endpoint, request) : null;
	}
	private String coalesce(String value) {
		return value == null ? "" : value;
	}

}
