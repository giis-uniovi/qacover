package test4giis.qacover.oracle;

import java.sql.SQLException;

import org.junit.Test;

import giis.qacover.core.services.Configuration;
import giis.qacover.model.Variability;
import test4giis.qacover.SqlUtil;
import test4giis.qacover.TestDataTypeFormats;

public class TestOracleDataTypeFormats extends TestDataTypeFormats {

	@Override
	protected Variability getVariant() {
		return new Variability("oracle");
	}

	// Adicionale tests not includer in the base class

	// QACover#11 Excepcion ejecutando regla cuando un parámetro de tipo fecha está
	// dentro de una función to_char (Oracle ORA-01722)
	// similar testEvalDateParameters pero encerrando en to_char (mayuscula o
	// minuscula) el parametro de tipo Date que se compara
	// Funcion to_char con argumento de tipo fecha en lower/uppercase
	@Test
	public void testEvalDateParametersInsideToChar() throws SQLException {
		doEvalDateParametersInsideToChar("select id,dat from dbmstest where to_char( dat, 'dd/MM/yyyy' ) = to_char( ?, 'dd/MM/yyyy' )");
		doEvalDateParametersInsideToChar("select id,dat from dbmstest where to_char( dat, 'dd/MM/yyyy' ) = TO_CHAR( ?, 'dd/MM/yyyy' )");
	}
	private void doEvalDateParametersInsideToChar(String sql) throws SQLException {
		Configuration.getInstance().setInferQueryParameters(false);
		rs = app.executeQuery(sql, java.sql.Date.valueOf("2020-01-02"));
		assertEvalResults(sql, "2 2020-01-02 00:00:00", SqlUtil.resultSet2csv(rs, " "),
				"COVERED   SELECT id , dat FROM dbmstest WHERE NOT(to_char(dat , 'dd/MM/yyyy') = TO_CHAR(TO_DATE('2020-01-02','yyyy-MM-dd') , 'dd/MM/yyyy'))\n"
						+ "COVERED   SELECT id , dat FROM dbmstest WHERE (to_char(dat , 'dd/MM/yyyy') = TO_CHAR(TO_DATE('2020-01-02','yyyy-MM-dd') , 'dd/MM/yyyy'))\n"
						+ "UNCOVERED SELECT id , dat FROM dbmstest WHERE (dat IS NULL)",
				"{?1?='2020-01-02'}", false, false);
	}

	@Test
	public void testEvalNumericParametersInsideToChar() throws SQLException {
		Configuration.getInstance().setInferQueryParameters(false);
		String sql = "select id,dat from dbmstest where '00002.0' = to_char( ?, '09999.9' )";
		rs = app.executeQuery(sql, 2);
		assertEvalResults(sql, "", SqlUtil.resultSet2csv(rs, " "),
				"COVERED   SELECT id , dat FROM dbmstest WHERE NOT('00002.0' = to_char(2 , '09999.9'))\n"
						+ "UNCOVERED SELECT id , dat FROM dbmstest WHERE ('00002.0' = to_char(2 , '09999.9'))",
				"{?1?=2}", false, false);
	}
}
