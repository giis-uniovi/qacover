package test4giis.qacover.postgres;

import giis.qacover.model.Variability;
import test4giis.qacover.TestEvaluationStandalone;

public class TestPostgresEvaluationStandalone extends TestEvaluationStandalone {

	@Override
	protected Variability getVariant() {
		return new Variability("postgres");
	}

}
