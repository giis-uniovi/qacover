/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using Giis.Tdrules.Openapi.Model;


namespace Giis.Qacover.Model
{
	/// <summary>Wrapper of the TdRules schema model</summary>
	public class SchemaModel
	{
		private TdSchema model;

		public SchemaModel()
		{
			model = new TdSchema();
		}

		public SchemaModel(TdSchema schemaModel)
		{
			model = schemaModel;
		}

		public virtual TdSchema GetModel()
		{
			return model;
		}

		public virtual string GetDbms()
		{
			return model.GetStoretype();
		}

		public override string ToString()
		{
			return model.ToString();
		}
	}
}
