package test4giis.qacover.postgres;

import java.sql.SQLException;

import org.junit.Before;
import org.junit.Test;

import giis.qacover.model.Variability;
import test4giis.qacover.DataTypeAssert;
import test4giis.qacover.TestDataTypeFormats;

public class TestPostgresDataTypeFormats extends TestDataTypeFormats {

	@Before
	@Override
	public void setUp() throws SQLException {
		super.setUp();
		com.p6spy.engine.spy.P6SpyOptions.getActiveInstance().setDatabaseDialectBooleanFormat("boolean");
	}

	@Override
	protected Variability getVariant() {
		return new Variability("postgres");
	}

	@Override
	protected String boolValue(boolean value) {
		return value ? "true" : "false";
	}

	// Datatype checks:
	// https://www.postgresql.org/docs/current/datatype.html#DATATYPE-TABLE
	@Test
	public void testDataTypes() throws SQLException {
		DataTypeAssert dta = new DataTypeAssert();
		dta.add("serial", "", "1");
		dta.add("integer", "111", "111");
		dta.add("smallint", "2", "2");
		dta.add("bigint", "3", "3");
		dta.add("decimal(8,4)", "4.3", "4.3000");
		dta.add("numeric(10,2)", "5.4", "5.40");
		dta.add("real", "6.5", "6.5");
		dta.add("double precision", "66.55", "66.55"); // no redondea
		dta.add("money", "7.6", "$7.60");
		dta.add("varchar(10)", "'abc'", "abc");
		dta.add("char(4)", "'d'", "d   "); // espacios en blanco al final
		dta.add("character(4)", "'efg'", "efg ");
		dta.add("date", "'2021-01-30'", "2021-01-30");
		dta.add("time", "'13:14:15'", "13:14:15");
		dta.add("timestamp", "'2021-01-30 13:14:15'", "2021-01-30 13:14:15");
		dta.add("interval", "'11:12:13'", "11:12:13");
		dta.add("time with time zone", "'12:13:14+0100'", "12:13:14+01");
		dta.add("timestamp with time zone", "'2021-01-30 12:13:14+0200'", "2021-01-30 11:13:14+01");
		dta.add("boolean", "true", "t");
		dta.assertAll(true, app, rs);
	}
}
