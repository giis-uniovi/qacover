package giis.qacover.model;

public class ParameterDao {
	public String name; // NOSONAR public access to allow direct serialization in the .NET version
	public String value; // NOSONAR

	public ParameterDao() {
		// default constructor required to deserialize java with jackson
	}

	public ParameterDao(String name, String value) {
		this.name = name;
		this.value = value;
	}

	@Override
	public String toString() {
		return this.name + "=" + this.value;
	}
}
