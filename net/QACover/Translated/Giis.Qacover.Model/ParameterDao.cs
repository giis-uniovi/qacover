/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////


namespace Giis.Qacover.Model
{
	public class ParameterDao
	{
		public string name;

		public string value;

		public ParameterDao()
		{
		}

		public ParameterDao(string name, string value)
		{
			// NOSONAR public access to allow direct serialization in the .NET version
			// NOSONAR
			// default constructor required to deserialize java with jackson
			this.name = name;
			this.value = value;
		}

		public override string ToString()
		{
			return this.name + "=" + this.value;
		}
	}
}
