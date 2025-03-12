package test4giis.qacover.oracle;

import giis.qacover.model.Variability;
import test4giis.qacover.TestStore;

public class TestOracleStore extends TestStore {

	@Override
	protected Variability getVariant() {
		return new Variability("oracle");
	}
}
