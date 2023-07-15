using Giis.Qacover.Model;
using System.Collections.Generic;


namespace Test4giis.Qacoverapp
{
	/// <summary>Diferentes tipos de queries que simulan lo ejecutado por una aplicacion basada conexiones jdbc obtenidas con DriverManager</summary>
	public class AppDbUtils : AppBase
	{
		public AppDbUtils(Variability targetVariant) : base(targetVariant) {  }

		/// <exception cref="Java.Imported.Sql.SQLException"/>
		public virtual IList<SimpleEntity> QueryDbUtils(int param2, string param1)
		{
            throw new System.NotImplementedException("No implementado en .net");
		}
	}
}
