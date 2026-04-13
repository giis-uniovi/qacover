package giis.qacover.core.coverage;

import java.util.List;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import giis.qacover.core.query.AbstractQueryStatement;
import giis.qacover.model.QueryModel;
import giis.qacover.model.ResultVector;
import giis.qacover.model.RuleModel;
import giis.qacover.model.Variability;

/**
 * Evaluates the coverage rules that are contained in a model, the results are stored in the model.
 * 
 * Decision of the coverage result is implemented in a delegate (different for fpc or mutation).
 */
public class CoverageService {
	private static final Logger log = LoggerFactory.getLogger(CoverageService.class);

	private QueryModel model;
	private ICoverageDecisor decisor;
	private boolean debugSql = false;

	public CoverageService(QueryModel model) {
		this.model = model;
		if ("mutation".equals(model.getModel().getRulesClass()))
			this.decisor = new CoverageMutation();
		else
			this.decisor = new CoverageFpc(); // fpc by default
	}
	
	public CoverageService setDebugSql(boolean value) {
		this.debugSql = value;
		return this;
	}

	/**
	 * Executes ead rule that have been generated in the model
	 */
	public ResultVector evaluateQuery(AbstractQueryStatement stmt, boolean skipIfCovered) {
		List<RuleModel> rules = model.getRules();
		model.reset(); // query counters will be recalculated at the end
		ResultVector result = new ResultVector(rules.size());
		stmt.setVariant(new Variability(model.getDbms()));

		// Mutation will require a previous step to read query results to compare to the output of each mutant
		String prepareSql = decisor.prepareEvaluation(stmt, model);
		if (!"".equals(prepareSql) && this.debugSql)
			log.debug("Query: " + prepareSql);

		// Executes and annotates the coverage/status of each rule
		for (int i = 0; i < rules.size(); i++) {
			RuleModel ruleModel = rules.get(i);
			String res;
			if (skipIfCovered && ruleModel.getDead() > 0)
				res = ResultVector.ALREADY_COVERED;
			else
				res = this.evaluateRule(ruleModel, stmt);

			// Results for logging
			String logString = this.getLogString(ruleModel, stmt, res);
			log.debug(logString);

			// store results
			result.setResult(i, res, logString);
			if (ruleModel.getDead() > 0)
				model.addDead(1);
			if (ruleModel.getError() > 0)
				model.addError(1);
		}
		model.setCount(rules.size());
		model.addQrun(1);
		log.info(" SUMMARY: Covered " + model.getDead() + " out of " + model.getCount());
		return result;
	}

	/**
	 * Determines the coverage of a rule, saving the results and returning if it is covered
	 */
	public String evaluateRule(RuleModel model, AbstractQueryStatement stmt) {
		String sql = this.decisor.getRuleQuery(model);
		model.addCount(1);
		try { // save results
			boolean isCovered = decisor.isCovered(stmt, sql);
			if (this.debugSql)
				log.debug("Rule: " + sql);
			model.addDead(isCovered ? 1 : 0);
			return isCovered ? ResultVector.COVERED : ResultVector.UNCOVERED;
		} catch (Exception e) {
			model.addError(1);
			model.addErrorString(e.toString());
			model.setRuntimeError(e.toString());
			return ResultVector.RUNTIME_ERROR;
		}
	}

	public String getLogString(RuleModel ruleModel, AbstractQueryStatement stmt, String res) {
		String sql = stmt.getSqlWithValues(ruleModel.getSql());
		String ruleWithSql = sql.replace("\r", "").replace("\n", " ").trim();
		String logString = res + " " + (ResultVector.COVERED.equals(res) ? "  " : "") + ruleWithSql;
		logString += ResultVector.RUNTIME_ERROR.equals(res) ? "\n" + ruleModel.getRuntimeError() : "";
		return logString;
	}

}
