/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using Giis.Tdrules.Openapi.Model;


namespace Giis.Qacover.Model
{
	/// <summary>Wrapper of the TdRules schema model</summary>
	public class SchemaModel
	{
		private DbSchema model;

		public SchemaModel()
		{
			model = new DbSchema();
		}

		public SchemaModel(DbSchema schemaModel)
		{
			model = schemaModel;
		}

		public virtual DbSchema GetModel()
		{
			return model;
		}

		public virtual string GetDbms()
		{
			return model.GetDbms();
		}

		public override string ToString()
		{
			return model.ToString();
		}
	}
}
