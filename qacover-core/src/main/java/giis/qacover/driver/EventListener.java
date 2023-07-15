package giis.qacover.driver;

import com.p6spy.engine.common.PreparedStatementInformation;
import com.p6spy.engine.common.StatementInformation;
import com.p6spy.engine.event.SimpleJdbcEventListener;
import com.p6spy.engine.logging.P6LogLoadableOptions;

import giis.qacover.core.Controller;
import giis.qacover.core.QueryStatement;

import java.sql.SQLException;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

/**
 * Intercepts the jdbc query calls and call the Controller to process the query
 */
final class EventListener extends SimpleJdbcEventListener {
	private static final Logger log = LoggerFactory.getLogger(EventListener.class);

	// @Nullable
	final String remoteServiceName;
	final boolean includeParameterValues;
	final P6LogLoadableOptions logOptions;

	EventListener(/* @Nullable */ String remoteServiceName, boolean includeParameterValues, P6LogLoadableOptions logOptions) {
		log.debug("*** EventListener.new EventListener(...)");
		this.remoteServiceName = remoteServiceName;
		this.includeParameterValues = includeParameterValues;
		this.logOptions = logOptions;
	}

	@Override
	public void onBeforeExecuteQuery(PreparedStatementInformation statementInformation) {
		log.debug("***** EventListener.onBeforeExecuteQuery(PreparedStatementInformation statementInformation)");
		runPreparedStatement(statementInformation);
	}

	@Override
	public void onBeforeExecuteQuery(StatementInformation statementInformation, String sql) {
		log.debug("***** EventListener.onBeforeExecuteQuery(StatementInformation statementInformation, String sql)");
		runStatement(statementInformation);
	}

	// Events with a generic execute, must filter out queries that are not select.
	// This is checked by the begining string.
	// Note that this will not identify as select the queries that have 
	// a comment annotation for named jdbc parameters
	@Override
	public void onBeforeExecute(PreparedStatementInformation statementInformation) {
		log.debug("***** EventListener.onBeforeExecute(PreparedStatementInformation statementInformation)");
		if (QueryStatement.isSelectQuery(statementInformation.getSql(), log))
			runPreparedStatement(statementInformation);
	}

	@Override
	public void onBeforeExecute(StatementInformation statementInformation, String sql) {
		log.debug("***** EventListener.onBeforeExecute(StatementInformation statementInformation, String sql)");
		if (QueryStatement.isSelectQuery(statementInformation.getSql(), log))
			runStatement(statementInformation);
	}

	// Internal methods to evaluation and checking the kind of query

	private void runStatement(StatementInformation statementInformation) {
		QueryStatement stmt = new StatementAdapter(statementInformation);
		Controller ctrl = new Controller();
		ctrl.processSql(stmt);
	}

	private void runPreparedStatement(PreparedStatementInformation statementInformation) {
		QueryStatement stmt = new StatementAdapter(statementInformation);
		Controller ctrl = new Controller();
		ctrl.processSql(stmt);
	}

	// All the other events, only for log

	@Override
	public void onBeforeExecuteBatch(StatementInformation statementInformation) {
		log.debug("----- Ignored by qacover EventListener.onBeforeExecuteBatch");
	}

	@Override
	public void onBeforeExecuteUpdate(PreparedStatementInformation statementInformation) {
		log.debug("----- Ignored by qacover EventListener.onBeforeExecuteUpdate");
	}

	@Override
	public void onBeforeExecuteUpdate(StatementInformation statementInformation, String sql) {
		log.debug("----- Ignored by qacover EventListener.onBeforeExecuteUpdate");
	}

	@Override
	public void onAfterExecute(PreparedStatementInformation statementInformation, long timeElapsedNanos,
			SQLException e) {
		log.debug("----- Ignored by qacover EventListener.onAfterExecute");
	}

	@Override
	public void onAfterExecute(StatementInformation statementInformation, long timeElapsedNanos, String sql,
			SQLException e) {
		log.debug("----- Ignored by qacover EventListener.onAfterExecute");
	}

	@Override
	public void onAfterExecuteBatch(StatementInformation statementInformation, long timeElapsedNanos,
			int[] updateCounts, SQLException e) {
		log.debug("----- Ignored by qacover EventListener.onAfterExecuteBatch");
	}

	@Override
	public void onAfterExecuteUpdate(PreparedStatementInformation statementInformation, long timeElapsedNanos,
			int rowCount, SQLException e) {
		log.debug("----- Ignored by qacover EventListener.onAfterExecuteUpdate");
	}

	@Override
	public void onAfterExecuteUpdate(StatementInformation statementInformation, long timeElapsedNanos, String sql,
			int rowCount, SQLException e) {
		log.debug("----- Ignored by qacover EventListener.onAfterExecuteUpdate");
	}

	@Override
	public void onAfterExecuteQuery(PreparedStatementInformation statementInformation, long timeElapsedNanos,
			SQLException e) {
		log.debug("----- Ignored by qacover EventListener.onAfterExecuteQuery");
	}

	@Override
	public void onAfterExecuteQuery(StatementInformation statementInformation, long timeElapsedNanos, String sql,
			SQLException e) {
		log.debug("----- Ignored by qacover EventListener.onAfterExecuteQuery");
	}

	@Override
	public void onBeforeAddBatch(PreparedStatementInformation statementInformation) {
		log.debug("----- Ignored by qacover EventListener.onBeforeAddBatch");
	}

	@Override
	public void onBeforeAddBatch(StatementInformation statementInformation, String sql) {
		log.debug("----- Ignored by qacover EventListener.onBeforeAddBatch");
	}

	@Override
	public void onAfterAddBatch(PreparedStatementInformation statementInformation, long timeElapsedNanos,
			SQLException e) {
		log.debug("----- Ignored by qacover EventListener.onAfterAddBatch");
	}

	@Override
	public void onAfterAddBatch(StatementInformation statementInformation, long timeElapsedNanos, String sql,
			SQLException e) {
		log.debug("----- Ignored by qacover EventListener.onAfterAddBatch");
	}

}
