package test4giis.qacover.oracle;

import giis.qacover.model.Variability;
import test4giis.qacover.TestExecute;

public class TestOracleExecute extends TestExecute {

	@Override
	protected Variability getVariant() {
		return new Variability("oracle");
	}
}
