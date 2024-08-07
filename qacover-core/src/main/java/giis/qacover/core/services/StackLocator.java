package giis.qacover.core.services;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import giis.portable.util.CallStack;

/**
 * Locates the position where a query call has been originated,
 * taking into account the inclusions and exclusions indicated in the options
 */
public class StackLocator {
	private static final String UNDEFINED = "undefined";
	private static final Logger log = LoggerFactory.getLogger(StackLocator.class);
	private CallStack stack;
	private String className = UNDEFINED;
	private String methodName = UNDEFINED;
	private String fileName = UNDEFINED;
	private String fullFileName = UNDEFINED;
	private int lineNumber = 0;

	public StackLocator() {
		stack = new CallStack();
		findLocation();
	}
	
	private void findLocation() {
		for (int i = 0; i < stack.size(); i++) {
			log.trace("StackTrace item: " + stack.getClassName(i) + " " + stack.getMethodName(i) + " "
					+ stack.getLineNumber(i) + " " + stack.getFileName(i));
			if (!excludeClassName(stack.getClassName(i))) {
				this.className = stack.getClassName(i);
				this.methodName = stack.getMethodName(i);
				this.fullFileName = stack.getFullFileName(i);
				this.fileName = stack.getFileName(i);
				this.lineNumber = stack.getLineNumber(i);
				return;
			}
		}
		log.warn("StackTrace is empty because of exclusions, full call stack displayed below: " + stack.getString());
	}

	private boolean excludeClassName(String stackClass) {
		if (Configuration.valueInProperty(stackClass, Configuration.getInstance().getStackInclusions(), true))
			return false;
		if (Configuration.valueInProperty(stackClass, Configuration.getInstance().getStackExclusions(), true)) // NOSONAR to keep cleaner structure
			return true;
		return false;
	}

	@Override
	public String toString() {
		return stack.getString();
	}

	public String getLocationAsString() {
		return className + "." + methodName + ":" + lineNumber + " " + fileName;
	}
	public String getClassName() {
		return className;
	}
	public String getMethodName() {
		return methodName;
	}
	public String getFileName() {
		return fileName;
	}
	public String getSourceFileName() {
		return fullFileName;
	}
	public int getLineNumber() {
		return lineNumber;
	}

}
