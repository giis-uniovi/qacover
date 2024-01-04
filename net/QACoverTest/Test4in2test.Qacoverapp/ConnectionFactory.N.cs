using Giis.Qacover.Model;
using Giis.Qacover.Driver;
using System.Data.Common;
using Giis.Portable.Util;
using Giis.Tdrules.Store.Rdb;

namespace Test4giis.Qacoverapp
{
	public class ConnectionFactory
	{
        private static readonly string SetupPath = FileUtil.GetPath(Parameters.GetProjectRoot(), "..", "setup");
        private static readonly string EnvironmentProperties = FileUtil.GetPath(SetupPath, "environment.properties");
        private static readonly string DatabaseProperties = FileUtil.GetPath(SetupPath, "database.properties");

        private readonly Variability variant;
        private readonly string url;
        private readonly string user;
		private readonly string password;

		/// <summary>
		/// Creacion de conexiones (interceptadas por p6spy o nativas) teniendo en cuenta la variabilidad.
		/// Configura todos los datos de la conexion de acuerdo con lo indicado en app.properties,
		/// donde las propiedades tienen la forma test4qacover.plataforma.dbms.
		/// </summary>
		public ConnectionFactory(Variability dbmsVariant)
		{
			variant = dbmsVariant;
			string propPrefix = "qacover." + variant.GetPlatformName() + ".qacoverdb." + variant.GetSgbdName();
			url = new JdbcProperties().GetProp(DatabaseProperties, propPrefix + ".url");
			user = new JdbcProperties().GetProp(DatabaseProperties, propPrefix + ".user");
			password = new JdbcProperties().GetEnvVar(EnvironmentProperties, "TEST_" + variant.GetSgbdName().ToUpper() + "_PWD");
		}

    /// <summary>Obtiene una conexion para uso de las aplicaciones bajo prueba</summary>
    public SampleDbConnectionWrapper GetConnection()
		{
			//a diferencia de java, aqui se debe utilizar un wrapper especifico
			return new SampleDbConnectionWrapper(GetNativeConnection());
		}

		/// <summary>Obtiene una conexion usando el driver nativo sin interceptar por p6spy para uso en carga y comprobacion de datos</summary>
		public DbConnection GetNativeConnection()
		{
			string realUrl = url;
			if (variant.IsSqlServer())
                realUrl = url + ";UID=" + user + ";PWD=" + password + ";MultipleActiveResultSets=true";
			DbConnection nativeConn = DbObjectFactory.GetDbConnection(variant, realUrl);
			nativeConn.Open();
			return nativeConn;
		}

	}
}
