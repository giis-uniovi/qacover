package test4giis.qacover.postgres;

import giis.qacover.model.Variability;
import test4giis.qacover.TestStore;

public class TestPostgresStore extends TestStore {

	protected Variability getVariant() {
		return new Variability("postgres");
	}
}
