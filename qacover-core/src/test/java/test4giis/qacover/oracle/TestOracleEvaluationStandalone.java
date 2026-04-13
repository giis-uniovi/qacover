package test4giis.qacover.oracle;

import giis.qacover.model.Variability;
import test4giis.qacover.TestEvaluationStandalone;

public class TestOracleEvaluationStandalone extends TestEvaluationStandalone {

	@Override
	protected Variability getVariant() {
		return new Variability("oracle");
	}

}
