package test4giis.qacover.h2;

import java.sql.SQLException;

import org.junit.Before;

import giis.qacover.model.Variability;
import test4giis.qacover.DataTypeAssert;
import test4giis.qacover.TestDataTypeFormats;

public class TestH2DataTypeFormats extends TestDataTypeFormats {

	@Before
	public void setUp() throws SQLException {
		super.setUp();
		com.p6spy.engine.spy.P6SpyOptions.getActiveInstance().setDatabaseDialectBooleanFormat("boolean");
	}

	protected Variability getVariant() {
		return new Variability("h2");
	}

	protected String boolValue(boolean value) {
		return value ? "true" : "false";
	}

	// Datatype checks
	// http://www.h2database.com/html/datatypes.html
	public void testDataTypes() throws SQLException {
		DataTypeAssert dta = new DataTypeAssert();
		dta.add("identity", "", "1");
		dta.add("int", "11", "11");
		dta.add("integer", "111", "111");
		dta.add("mediumint", "2", "2");
		dta.add("signed", "2", "2");
		dta.add("int4", "31", "31");
		dta.add("tinyint", "32", "32");
		dta.add("smallint", "33", "33");
		dta.add("int2", "34", "34");
		// dta.add("year", "35", "35");
		dta.add("bigint", "36", "36");
		dta.add("int8", "37", "37");
		dta.add("decimal(8,4)", "4.3", "4.3000");
		dta.add("number(10,2)", "5.4", "5.40");
		dta.add("dec(10,2)", "5.5", "5.50");
		dta.add("numeric(10,2)", "5.6", "5.60");
		dta.add("double", "66.55", "66.55");
		dta.add("double precision", "66.56", "66.56");
		dta.add("float", "66.57", "66.57");
		dta.add("float(3)", "66.5723", "66.5723");
		dta.add("float8", "66.58", "66.58");
		dta.add("float4", "66.59", "66.59");
		dta.add("varchar(10)", "'abc'", "abc");
		dta.add("character varying(4)", "'efg'", "efg");
		dta.add("longvarchar(4)", "'efh'", "efh");
		dta.add("varchar2(4)", "'efi'", "efi");
		dta.add("nvarchar(4)", "'efj'", "efj");
		dta.add("nvarchar2(4)", "'efk'", "efk");
		dta.add("varchar_casesensitive(4)", "'efm'", "efm");
		// dta.add("varchar_ignorecase(4)", "'efn'", "efn");
		dta.add("char(4)", "'d'", "d   "); // no hay espacios en blanco al final
		dta.add("character(4)", "'efg'", "efg ");
		dta.add("nchar(4)", "'efh'", "efh ");
		dta.add("date", "'2021-01-30'", "2021-01-30");
		dta.add("time", "'13:14:15'", "13:14:15");
		dta.add("timestamp", "'2021-01-30 13:14:15'", "2021-01-30 13:14:15");
		dta.add("datetime", "'2021-01-30 13:14:16'", "2021-01-30 13:14:16");
		dta.add("smalldatetime", "'2021-01-30 13:14:17'", "2021-01-30 13:14:17");
		dta.add("time with time zone", "'12:13:14+01:00'", "12:13:14+01");
		dta.add("timestamp with time zone", "'2021-01-30 12:13:15+01:00'", "2021-01-30 12:13:15+01");
		dta.add("boolean", "true", "TRUE");
		dta.add("bit", "true", "TRUE");
		dta.add("bool", "true", "TRUE");
		dta.assertAll(true, app, rs);
	}
}
