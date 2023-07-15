package giis.qacover.core.services;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

/**
 * To support some test situations we need to inject faults in strategic
 * points of the evaluation process.
 * 
 * Each QueryStatement has a static instance of this class. If not null
 * will perform the injection of faults configured in this class.
 * WARNING: This is for testing only and not concurrency safe.
 */
public class FaultInjector {
	private static final Logger log = LoggerFactory.getLogger(FaultInjector.class);
	private String faultException = "";
	private String faultSchema = "";
	private String faultTables = "";
	private String faultInfer = "";
	private String faultRules = "";
	private String faultSingleRule = "";

	public FaultInjector() {
		reset();
	}

	public FaultInjector reset() {
		faultSchema = "";
		return this;
	}

	public FaultInjector setUnexpectedException(String msg) {
		this.faultException = msg;
		return this;
	}
	public boolean isUnexpectedException() {
		return !"".equals(faultException);
	}
	public String getUnexpectedException() {
		log.warn("Injecting unexpected exception, message: " + faultSchema);
		return faultException;
	}
	
	public FaultInjector setSchemaFaulty(String sql) {
		this.faultSchema = sql;
		return this;
	}
	public boolean isSchemaFaulty() {
		return !"".equals(faultSchema);
	}
	public String getSchemaFault() {
		log.warn("Injecting fault at GetSchema, sql: " + faultSchema);
		return faultSchema;
	}
	
	public FaultInjector setTablesFaulty(String sql) {
		this.faultTables = sql;
		return this;
	}
	public boolean isTablesFaulty() {
		return !"".equals(faultTables);
	}
	public String getTablesFault() {
		log.warn("Injecting fault at GetTables, message: " + faultTables);
		return faultTables;
	}
	
	public FaultInjector setInferFaulty(String msg) {
		this.faultInfer = msg;
		return this;
	}
	public boolean isInferFaulty() {
		return !"".equals(faultInfer);
	}
	public String getInferFault() {
		log.warn("Injecting fault at GetInfer, message: " + faultInfer);
		return faultInfer;
	}
	
	public FaultInjector setRulesFaulty(String sql) {
		this.faultRules = sql;
		return this;
	}
	public boolean isRulesFaulty() {
		return !"".equals(faultRules);
	}
	public String getRulesFault() {
		log.warn("Injecting fault at GetRules, message: " + faultRules);
		return faultRules;
	}
	
	public FaultInjector setSingleRuleFaulty(String sql) {
		this.faultSingleRule = sql;
		return this;
	}
	public boolean isSingleRuleFaulty() {
		return !"".equals(faultSingleRule);
	}
	public String getSingleRuleFault() {
		log.warn("Injecting fault at a rule, message: " + faultSingleRule);
		return faultSingleRule;
	}

}
