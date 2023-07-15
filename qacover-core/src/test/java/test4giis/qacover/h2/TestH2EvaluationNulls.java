package test4giis.qacover.h2;

import giis.qacover.model.Variability;
import test4giis.qacover.TestEvaluationNulls;

public class TestH2EvaluationNulls extends TestEvaluationNulls {

	protected Variability getVariant() {
		return new Variability("h2");
	}
}
