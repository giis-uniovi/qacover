package giis.qacover.model;

import java.util.List;

public class HistoryDao {
	public String at; // NOSONAR public access to allow direct serialization in the .NET version
	public String key; // NOSONAR
	public List<ParameterDao> params; // NOSONAR
}
