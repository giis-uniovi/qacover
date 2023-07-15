package giis.qacover.portable;

public class QaCoverException extends RuntimeException {
	private static final long serialVersionUID = 1062540062446421938L;

	public QaCoverException(Throwable e) {
		super("Schema Exception", e);
	}

	public QaCoverException(String message) {
		super(message);
	}

	public QaCoverException(String message, Throwable cause) {
		super(message + (cause == null ? "" : ". Caused by: " + getString(cause)), cause);
	}

	/**
	 * En java toString devuelve el nombre de clase y mensaje, pero en .net incluye
	 * el stacktrace. Utilizando este metodo en cada plataforma se genera un mensaje
	 * similar al de java
	 */
	public static String getString(Throwable e) {
		return e.toString();
	}
}
