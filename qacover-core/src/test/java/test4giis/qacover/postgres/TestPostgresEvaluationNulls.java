package test4giis.qacover.postgres;

import giis.qacover.model.Variability;
import test4giis.qacover.TestEvaluationNulls;

public class TestPostgresEvaluationNulls extends TestEvaluationNulls {

	@Override
	protected Variability getVariant() {
		return new Variability("postgres");
	}
}
