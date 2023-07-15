package test4giis.qacover.oracle;

import giis.qacover.model.Variability;
import test4giis.qacover.TestEvaluation;

public class TestOracleEvaluation extends TestEvaluation {
	
	protected Variability getVariant() {
		return new Variability("oracle");
	}
}
