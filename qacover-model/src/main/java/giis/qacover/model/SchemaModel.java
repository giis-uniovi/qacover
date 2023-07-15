package giis.qacover.model;

import giis.tdrules.openapi.model.DbSchema;

/**
 * Wrapper of the TdRules schema model
 */
public class SchemaModel {
	private DbSchema model;

	public SchemaModel() {
		model = new DbSchema();
	}

	public SchemaModel(DbSchema schemaModel) {
		model = schemaModel;
	}

	public DbSchema getModel() {
		return model;
	}

	public String getDbms() {
		return model.getDbms();
	}

	@Override
	public String toString() {
		return model.toString();
	}
}
