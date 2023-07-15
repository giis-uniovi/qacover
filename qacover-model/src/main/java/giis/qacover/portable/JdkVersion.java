package giis.qacover.portable;

/**
 * Indica la version del jdk con el que esta compilada la aplicacion. Como no es
 * trivial conocer este valor, utiliza estas constantes que cambiaran en
 * proyectos compilados con versiones diferentes
 */
public class JdkVersion {
	public int getVersion() {
		return 8; // NOSONAR
	}
}
