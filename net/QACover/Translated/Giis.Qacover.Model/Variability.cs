/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using Giis.Portable.Util;
using Giis.Qacover.Portable;


namespace Giis.Qacover.Model
{
	/// <summary>Management of the variability of different versions, DBMS and platforms</summary>
	public class Variability
	{
		private string currentSgbd = string.Empty;

		public Variability()
		{
		}

		public Variability(string sgbdName)
		{
			// identificador textual del DBMS
			currentSgbd = sgbdName;
		}

		// Variantes relativas a la plataforma
		public virtual bool IsJava()
		{
			return Parameters.IsJava();
		}

		// La version estandar es java 8
		public virtual bool IsJava8()
		{
			return this.IsJava() && new JdkVersion().GetVersion() == 8;
		}

		public virtual bool IsJava4()
		{
			// java 1.4
			return this.IsJava() && new JdkVersion().GetVersion() == 4;
		}

		// no contemplado NetFramework
		public virtual bool IsNetCore()
		{
			return Parameters.IsNetCore();
		}

		/// <summary>Identificador corto de la variante</summary>
		public virtual string GetVariantId()
		{
			return IsJava() ? "j" + JavaCs.NumToString(new JdkVersion().GetVersion()) : "nc";
		}

		/// <summary>Nombre de la variante correspondiente a la plataforma</summary>
		public virtual string GetPlatformName()
		{
			if (IsJava4())
			{
				return "java4";
			}
			if (IsNetCore())
			{
				return "netcore";
			}
			return "java";
		}

		// por defecto
		// Variantes relativas al sgbd
		public virtual bool IsSqlite()
		{
			return "sqlite".Equals(currentSgbd);
		}

		public virtual bool IsH2()
		{
			return "h2".Equals(currentSgbd);
		}

		public virtual bool IsOracle()
		{
			return "oracle".Equals(currentSgbd);
		}

		public virtual bool IsSqlServer()
		{
			return "sqlserver".Equals(currentSgbd);
		}

		public virtual bool IsPostgres()
		{
			return "postgres".Equals(currentSgbd);
		}

		/// <summary>Nombre de la variante correspondiente al sgbd</summary>
		public virtual string GetSgbdName()
		{
			return this.currentSgbd;
		}

		// no hay valor por defecto, seria "" que significa no variante conocida
		// Variantes relativas a la version de la aplicacion
		public virtual string GetVersion()
		{
			return new Versions(this.GetType(), "io.github.giis-uniovi", "qacover-model").GetVersion();
		}

		public virtual string GetServiceDeskAddress()
		{
			return "https://github.com/giis-uniovi/qacover/issues/new";
		}
	}
}
