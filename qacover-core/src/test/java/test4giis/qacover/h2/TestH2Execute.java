package test4giis.qacover.h2;

import giis.qacover.model.Variability;
import test4giis.qacover.TestExecute;

public class TestH2Execute extends TestExecute {

	@Override
	protected Variability getVariant() {
		return new Variability("h2");
	}
}
