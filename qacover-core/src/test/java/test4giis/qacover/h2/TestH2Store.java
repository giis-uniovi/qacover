package test4giis.qacover.h2;

import giis.qacover.model.Variability;
import test4giis.qacover.TestStore;

public class TestH2Store extends TestStore {

	protected Variability getVariant() {
		return new Variability("h2");
	}
}
