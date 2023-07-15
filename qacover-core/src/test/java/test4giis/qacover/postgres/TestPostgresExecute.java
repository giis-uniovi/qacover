package test4giis.qacover.postgres;

import giis.qacover.model.Variability;
import test4giis.qacover.TestExecute;

public class TestPostgresExecute extends TestExecute {

	protected Variability getVariant() {
		return new Variability("postgres");
	}
}
