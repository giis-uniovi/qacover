package test4giis.qacover.postgres;

import giis.qacover.model.Variability;
import test4giis.qacover.TestEvaluationDbUtils;

public class TestPostgresEvaluationDbUtils extends TestEvaluationDbUtils {

	@Override
	protected Variability getVariant() {
		return new Variability("postgres");
	}
}
