package test4giis.qacover.sqlserver;

import giis.qacover.model.Variability;
import test4giis.qacover.TestMutation;

public class TestSqlserverMutation extends TestMutation {

	@Override
	protected Variability getVariant() {
		return new Variability("sqlserver");
	}

}
