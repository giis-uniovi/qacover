package giis.qacover.model;

/**
 * Represents the evaluation status of each rule of a query
 */
public class ResultVector {
	public static final String NOT_EVALUATED = "NOT_EVALUATED";
	public static final String COVERED = "COVERED";
	public static final String UNCOVERED = "UNCOVERED";
	public static final String ALREADY_COVERED = "ALREADY_COVERED";
	public static final String RUNTIME_ERROR = "RUNTIME_ERROR";

	private String[] vector;

	public ResultVector(int ruleCount) {
		vector = new String[ruleCount];
		for (int i = 0; i < vector.length; i++)
			vector[i] = NOT_EVALUATED;
	}

	public void setResult(int ruleNumber, String status) {
		vector[ruleNumber] = status;
	}

	@Override
	public String toString() {
		StringBuilder sb = new StringBuilder();
		for (String item : vector)
			if (NOT_EVALUATED.equals(item))
				sb.append(".");
			else if (COVERED.equals(item))
				sb.append("#");
			else if (UNCOVERED.equals(item))
				sb.append("o");
			else if (ALREADY_COVERED.equals(item))
				sb.append("+");
			else if (RUNTIME_ERROR.equals(item))
				sb.append("!");
		return sb.toString();
	}

}
