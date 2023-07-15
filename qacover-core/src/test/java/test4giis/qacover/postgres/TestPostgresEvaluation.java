package test4giis.qacover.postgres;

import java.sql.SQLException;

import org.junit.Test;

import giis.qacover.model.Variability;
import test4giis.qacover.TestEvaluation;

public class TestPostgresEvaluation extends TestEvaluation {

	protected Variability getVariant() {
		return new Variability("postgres");
	}

	@Test
	public void testEvalNoParameters() throws SQLException {
		super.testEvalNoParameters();
	}
}
