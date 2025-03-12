package test4giis.qacover.sqlserver;

import giis.qacover.model.Variability;
import test4giis.qacover.TestEvaluationNulls;

public class TestSqlserverEvaluationNulls extends TestEvaluationNulls {

	@Override
	protected Variability getVariant() {
		return new Variability("sqlserver");
	}
}
