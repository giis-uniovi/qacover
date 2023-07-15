using Giis.Qacover.Model;

using NUnit.Framework;

namespace Test4giis.Qacover.Ef.Sqlserver
{
	/// <summary>Particularidades especificas de SQL Server (boolean y date)</summary>
	public class TestSqlserverEvaluationEf : TestEvaluationEf
	{
		//override la variabilidad para indicar el uso de este sgbd, heredando todos los tests
		protected override Variability GetVariant()
		{
			return new Variability("sqlserver");
		}
	}
}
