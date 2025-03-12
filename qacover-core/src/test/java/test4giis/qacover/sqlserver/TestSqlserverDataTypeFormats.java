package test4giis.qacover.sqlserver;

import giis.qacover.model.Variability;
import test4giis.qacover.TestDataTypeFormats;

public class TestSqlserverDataTypeFormats extends TestDataTypeFormats {

	@Override
	protected Variability getVariant() {
		return new Variability("sqlserver");
	}
}
