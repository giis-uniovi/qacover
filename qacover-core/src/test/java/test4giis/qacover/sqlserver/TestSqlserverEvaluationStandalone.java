package test4giis.qacover.sqlserver;

import giis.qacover.model.Variability;
import test4giis.qacover.TestEvaluationStandalone;

public class TestSqlserverEvaluationStandalone extends TestEvaluationStandalone {

	@Override
	protected Variability getVariant() {
		return new Variability("sqlserver");
	}

}
