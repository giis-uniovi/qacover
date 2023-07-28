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
    public void testByRunOrder() {
    	String allRuns = getReaderByRunOrderAll(inpPath);
		FileUtil.fileWrite(outPath, "by-run-order.txt", allRuns);
    }
    public void testByClass() {
    	String allQueries = getReaderByClass(inpPath);
		FileUtil.fileWrite(outPath, "by-class.txt", allQueries);
    }
    /**
     * Ejemplo de uso de un CoverageReader para obtener toda la informacion sobre 
     * cada una de las ejecuciones de las 
     * queries y reglas generadas durante la evaluacion de la cobertura con QACover
     * @param rulesFolder Carpeta donde se encuentra el resultado de la evaluacion
     * @return un string con toda la informacion obtenida
     */
	private String getReaderByRunOrderAll(String rulesFolder) {
		StringBuilder allData=new StringBuilder();
		//Obtiene cada una de las ejecuciones de reglas de cobertura (en orden temporal)
		//creando una coleccion QueryCollection que contiene objetos QueryReader
		CoverageReader cr=new CoverageReader(rulesFolder);
		QueryCollection qc=cr.getByRunOrder();
		for (int i=0; i<qc.size(); i++) {
			QueryReader qi=qc.get(i);
			//todos los datos de la query ejecutada en un momento dado
			allData.append("\n***** Execution " + qi.getKey() + "\n");
			allData.append("* timestamp: " + qi.getTimestamp() + "\n");
			allData.append("* class: " + qi.getKey().getClassName() + "\n");
			allData.append("* line: " + qi.getKey().getClassLine() + "\n");
			allData.append("* method: " + qi.getKey().getMethodName() + "\n");
			allData.append("* sql: " + qi.getSql() + "\n");
			allData.append("* parameters: " + qi.getParametersXml() + "\n");
			//muestra el esquema solo para la primera regla
			if (i==0)
				allData.append("* schema xml: " + new TdSchemaXmlSerializer().serialize(qi.getSchema().getModel()) + "\n");
			//las reglas, solo muestra el sql
			QueryModel model=qi.getModel();
			allData.append("* Using the model to iterate over " + model.getRules().size() + " rules\n");
			for (int j=0; j<model.getRules().size(); j++)
				allData.append("rule" + j + ": " + model.getRules().get(j).getSql() + "\n");
		}
		return allData.toString();
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
		for (int i=0; i<cc.size(); i++) {
			allData.append("***** Class: " + cc.get(i).getName() + "\n");
			QueryCollection qc=cc.get(i);
			for (int j=0; j<qc.size(); j++) {
				//Se obtienen query items como en el ejemplo anterior, pero en este caso
				//no tienen datos de una ejecucion concreta (parametros o timestamp)
				QueryReader qi=qc.get(j);
				allData.append("* method: " + qi.getKey().getMethodName(true) + " - " + qi.getSql() + "\n");
			}
		}
		return allData.toString();
	}

}
