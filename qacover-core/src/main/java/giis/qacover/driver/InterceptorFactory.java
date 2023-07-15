package giis.qacover.driver;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.p6spy.engine.event.JdbcEventListener;
import com.p6spy.engine.spy.P6Factory;
import com.p6spy.engine.spy.P6LoadableOptions;
import com.p6spy.engine.spy.option.P6OptionsRepository;

/**
 * Add this class name to the "moduleslist" in spy.properties
 */
public class InterceptorFactory implements P6Factory {
	private static final Logger log = LoggerFactory.getLogger(InterceptorFactory.class);

	InterceptorOptions options;

	@Override
	public P6LoadableOptions getOptions(P6OptionsRepository repository) {
		log.debug("*** InterceptorFactory.getOptions(P6OptionsRepository repository)");
		options = new InterceptorOptions(repository);
		return options;
	}

	@Override
	public JdbcEventListener getJdbcEventListener() {
		log.debug("*** InterceptorFactory.getJdbcEventListener()");
		return new EventListener(options.remoteServiceName(), options.includeParameterValues(),
				options.getLogOptions());
	}
}
