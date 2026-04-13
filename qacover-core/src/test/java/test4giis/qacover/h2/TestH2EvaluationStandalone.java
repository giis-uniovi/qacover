package test4giis.qacover.h2;

import giis.qacover.model.Variability;
import test4giis.qacover.TestEvaluationStandalone;

public class TestH2EvaluationStandalone extends TestEvaluationStandalone {

	@Override
	protected Variability getVariant() {
		return new Variability("h2");
	}

}
