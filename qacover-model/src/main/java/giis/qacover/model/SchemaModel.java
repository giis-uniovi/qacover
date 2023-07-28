package giis.qacover.model;

import giis.tdrules.openapi.model.TdSchema;

/**
 * Wrapper of the TdRules schema model
 */
public class SchemaModel {
	private TdSchema model;

	public SchemaModel() {
		model = new TdSchema();
	}

	public SchemaModel(TdSchema schemaModel) {
		model = schemaModel;
	}

	public TdSchema getModel() {
		return model;
	}

	public String getDbms() {
		return model.getStoretype();
	}

	@Override
	public String toString() {
		return model.toString();
	}
}
