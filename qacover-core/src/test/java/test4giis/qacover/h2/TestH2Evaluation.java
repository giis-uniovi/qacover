package test4giis.qacover.h2;

import java.sql.SQLException;

import org.junit.Test;

import giis.qacover.model.Variability;
import test4giis.qacover.TestEvaluation;

public class TestH2Evaluation extends TestEvaluation {

	protected Variability getVariant() {
		return new Variability("h2");
	}

	@Test
	public void testEvalNoParameters() throws SQLException {
		super.testEvalNoParameters();
	}
}
