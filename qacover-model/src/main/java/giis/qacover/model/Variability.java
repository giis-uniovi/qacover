package giis.qacover.model;

import giis.portable.util.JavaCs;
import giis.portable.util.Parameters;
import giis.portable.util.Versions;
import giis.qacover.portable.JdkVersion;

/**
 * Management of the variability of different versions, DBMS and platforms
 */
public class Variability {
	private String currentSgbd = ""; // identificador textual del DBMS

	public Variability() {
	}

	public Variability(String sgbdName) {
		currentSgbd = sgbdName;
	}

	// Variantes relativas a la plataforma

	public boolean isJava() {
		return Parameters.isJava();
	}

	// La version estandar es java 8
	public boolean isJava8() {
		return this.isJava() && new JdkVersion().getVersion() == 8;
	}

	public boolean isJava4() { // java 1.4
		return this.isJava() && new JdkVersion().getVersion() == 4;
	}

	// no contemplado NetFramework
	public boolean isNetCore() {
		return Parameters.isNetCore();
	}

	/**
	 * Identificador corto de la variante
	 */
	public String getVariantId() {
		return isJava() ? "j"+JavaCs.numToString(new JdkVersion().getVersion()) : "nc";
	}

	/**
	 * Nombre de la variante correspondiente a la plataforma
	 */
	public String getPlatformName() {
		if (isJava4())
			return "java4";
		if (isNetCore())
			return "netcore";
		return "java"; // por defecto
	}

	// Variantes relativas al sgbd

	public boolean isSqlite() {
		return "sqlite".equals(currentSgbd);
	}

	public boolean isH2() {
		return "h2".equals(currentSgbd);
	}

	public boolean isOracle() {
		return "oracle".equals(currentSgbd);
	}

	public boolean isSqlServer() {
		return "sqlserver".equals(currentSgbd);
	}

	public boolean isPostgres() {
		return "postgres".equals(currentSgbd);
	}

	/**
	 * Nombre de la variante correspondiente al sgbd
	 */
	public String getSgbdName() {
		return this.currentSgbd; // no hay valor por defecto, seria "" que significa no variante conocida
	}

	// Variantes relativas a la version de la aplicacion

	public String getVersion() {
		return new Versions(this.getClass(), "io.github.giis-uniovi", "qacover-model").getVersion();
	}

	public String getServiceDeskAddress() {
		return "https://github.com/giis-uniovi/qacover/issues/new";
	}

}
