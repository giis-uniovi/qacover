package test4giis.qacover.oracle;

import giis.qacover.model.Variability;
import test4giis.qacover.TestEvaluationDbUtils;

public class TestOracleEvaluationDbUtils extends TestEvaluationDbUtils {

	protected Variability getVariant() {
		return new Variability("oracle");
	}
}
