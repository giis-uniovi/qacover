package giis.qacover.portable;

import java.io.File;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.net.URL;
import java.nio.charset.Charset;

/**
 * Creacion de Properties para compatibilidad Java 1.6 y 1.4 En
 * getPropertiesFromSystemProperty, Java 1.4 no permite cargar con un Reader.
 * Esta clase es compatible en tiempo de compilacion pero usa compatibles en
 * tiempo de ejecucion para java 1.4
 */
public class Jdk14PropertiesFactory {

	public java.util.Properties getPropertiesFromSystemProperty(boolean isJava14, String fileName,
			String defaultFileName) {
		try {
			String propsFileName = System.getProperty(fileName, defaultFileName);
			java.util.Properties prop = new java.util.Properties();
			File fp;
			URL url = null;
			fp = new File(propsFileName);
			if (fp.exists()) {
				url = fp.toURI().toURL();
				if (isJava14) {
					InputStream in = url.openStream(); // NOSONAR
					prop.load(in);
				} else {
					String optionsFileCharsetProperty = fileName.concat(".charset");
					String charsetName = System.getProperty(optionsFileCharsetProperty,
							Charset.defaultCharset().name());
					InputStream in = url.openStream(); // NOSONAR
					prop.load(new InputStreamReader(in, charsetName)); // NOSONAR
				}
				return prop;
			}
			return null;
		} catch (IOException e) {
			return null;
		}
	}

}
