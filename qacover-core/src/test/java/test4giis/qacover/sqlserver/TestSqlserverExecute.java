package test4giis.qacover.sqlserver;

import giis.qacover.model.Variability;
import test4giis.qacover.TestExecute;

public class TestSqlserverExecute extends TestExecute {

	protected Variability getVariant() {
		return new Variability("sqlserver");
	}
}
