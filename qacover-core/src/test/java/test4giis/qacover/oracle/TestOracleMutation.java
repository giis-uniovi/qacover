package test4giis.qacover.oracle;

import giis.qacover.model.Variability;
import test4giis.qacover.TestMutation;

public class TestOracleMutation extends TestMutation {

	@Override
	protected Variability getVariant() {
		return new Variability("oracle");
	}

}
