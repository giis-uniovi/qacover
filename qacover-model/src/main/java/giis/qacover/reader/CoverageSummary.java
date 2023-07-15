package giis.qacover.reader;

/**
 * The contents of a summary of an evaluation of a query
 */
public class CoverageSummary {
	// Overall counters
	protected int count = 0; // total rules
	protected int dead = 0; // total covered
	protected int error = 0; // total with error
	// Query related
	private int qcount = 0; // number of queries
	private int qrun = 0; // number of times executed
	private int qerror = 0; // number of errors in queries

	@Override
	public String toString() {
		return "qcount=" + qcount + ",qerror=" + qerror + ",count=" + count + ",dead=" + dead + ",error=" + error;
	}

	public int getCount() {
		return count;
	}
	public int getDead() {
		return dead;
	}
	public int getError() {
		return error;
	}
	public void addRuleCounters(int countValue, int deadValue, int errorValue) {
		this.count += countValue;
		this.dead += deadValue;
		this.error += errorValue;
	}

	public int getQcount() {
		return qcount;
	}
	public int getQrun() {
		return qrun;
	}
	public int getQerror() {
		return qerror;
	}
	public void addQueryCounters(int countValue, int runValue, int errorValue) {
		this.qcount += countValue;
		this.qrun += runValue;
		this.qerror += errorValue;
	}

}
