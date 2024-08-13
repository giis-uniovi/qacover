package giis.qacover.core;

import giis.qacover.core.services.Configuration;

public class RuleDriverFactory {

	/**
	 * Instantiation of a rule driver according with the current configuration
	 * options. At this moment, only fpc
	 */
	public RuleDriver getDriver() {
		if ("mutation".equals(Configuration.getInstance().getRuleCriterion()))
			return new RuleDriverMutation();
		else // fpc by default
			return new RuleDriverFpc();
	}
}
