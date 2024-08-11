package test4giis.qacover.postgres;

import giis.qacover.model.Variability;
import test4giis.qacover.TestMutation;

public class TestPostgresMutation extends TestMutation {

	@Override
	protected Variability getVariant() {
		return new Variability("postgres");
	}

}
