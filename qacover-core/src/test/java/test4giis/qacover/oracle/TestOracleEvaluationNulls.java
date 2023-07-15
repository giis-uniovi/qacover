package test4giis.qacover.oracle;

import giis.qacover.model.Variability;
import test4giis.qacover.TestEvaluationNulls;

public class TestOracleEvaluationNulls extends TestEvaluationNulls {

	protected Variability getVariant() {
		return new Variability("oracle");
	}
}
