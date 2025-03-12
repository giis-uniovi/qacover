package giis.qacover.portable;

/**
 * Utilizar esta clase cuando se va a hacer downgrade de codigo a jdk 1.4, ya
 * que no tiene StringBuilder: El codigo con el downgrade implmentara otra
 * version de esta clase.
 */
public class Jdk14StringBuilder {
	StringBuilder s = new StringBuilder();

	public int length() {
		return s.length();
	}

	public void append(String str) {
		s.append(str);
	}

	@Override
	public String toString() {
		return s.toString();
	}
}
