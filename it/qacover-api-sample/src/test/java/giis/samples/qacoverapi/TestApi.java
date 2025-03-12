package giis.samples.qacoverapi;

import giis.tdrules.model.io.TdSchemaXmlSerializer;
import giis.portable.util.FileUtil;
import giis.qacover.model.QueryModel;
import giis.qacover.reader.CoverageCollection;
import giis.qacover.reader.CoverageReader;
import giis.qacover.reader.QueryCollection;
import giis.qacover.reader.QueryReader;
import junit.framework.TestCase;

/**
 * Utiliza los datos de las reglas generadas en ut desde TestReport
 * para acceder al api que expone qacover-model.
 * A diferencia de los ut, este test que se ejecutara desde it de froma standalone
 * fuera del contexto y classpath del maven principal.
 * Los resultados obtenidos seran comparados en TestIntegration
 */
public class TestApi extends TestCase
{
	private String outPath="target/qacover/reports";
	// Input is a copy of the rules that were generaged by TestReport (copied from qacover-core/target/qacover-report/rules)
	private String inpPath="../../qacover-core/src/test/resources/qacover-api-sample-rules-from-test-report";

    public void testByClass() {
    	String allQueries = getReaderByClass(inpPath);
		FileUtil.fileWrite(outPath, "by-class.txt", allQueries);
    }
    /**
     * Ejemplo de uso de un CoverageReader para obtener toda la informacion sobre las 
     * queries y reglas generadas durante la evaluacion de la cobertura con QACover
     * (sin tener en cuenta las diferentes ejecuciones de cada query)
     * @param rulesFolder Carpeta donde se encuentra el resultado de la evaluacion
     * @return un string con toda la informacion obtenida
     */
	private String getReaderByClass(String rulesFolder) {
		StringBuilder allData=new StringBuilder();
		//Obtiene toda la estructua de las queries agrupadas por clases y sus reglas
		//Esta informacion es la que se obtiene en formato html en los reports
		CoverageReader cr=new CoverageReader(rulesFolder);
		CoverageCollection cc=cr.getByClass();
		for (int i=0; i<cc.getSize(); i++) {
			allData.append("***** Class: " + cc.getItem(i).getName() + "\n");
			QueryCollection qc=cc.getItem(i);
			for (int j=0; j<qc.getSize(); j++) {
				//Se obtienen query items como en el ejemplo anterior, pero en este caso
				//no tienen datos de una ejecucion concreta (parametros o timestamp)
				QueryReader qi=qc.getItem(j);
				allData.append("* method: " + qi.getKey().getMethodName(true) + " - " + qi.getSql() + "\n");
			}
		}
		return allData.toString();
	}

}
