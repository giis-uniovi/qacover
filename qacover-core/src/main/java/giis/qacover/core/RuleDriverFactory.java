package giis.qacover.core;

public class RuleDriverFactory {

	/**
	 * Instantiation of a rule driver according with the current configuration
	 * options. At this moment, only fpc
	 */
	public RuleDriver getDriver() {
		return new RuleDriverFpc();
	}
}
