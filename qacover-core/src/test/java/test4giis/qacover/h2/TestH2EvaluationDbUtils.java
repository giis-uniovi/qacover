package test4giis.qacover.h2;

import giis.qacover.model.Variability;
import test4giis.qacover.TestEvaluationDbUtils;

public class TestH2EvaluationDbUtils extends TestEvaluationDbUtils {

	@Override
	protected Variability getVariant() {
		return new Variability("h2");
	}
}
