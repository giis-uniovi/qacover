package test4giis.qacover.sqlserver;

import giis.qacover.model.Variability;
import test4giis.qacover.TestEvaluation;

public class TestSqlserverEvaluation extends TestEvaluation {

	protected Variability getVariant() {
		return new Variability("sqlserver");
	}
}
