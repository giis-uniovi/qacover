package test4giis.qacover.sqlserver;

import giis.qacover.model.Variability;
import test4giis.qacover.TestEvaluationNulls;

public class TestSqlserverEvaluationNulls extends TestEvaluationNulls {

	protected Variability getVariant() {
		return new Variability("sqlserver");
	}
}
