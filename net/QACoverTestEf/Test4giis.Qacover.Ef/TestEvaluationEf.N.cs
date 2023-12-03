using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Test4giis.Qacoverapp.Ef;

namespace Test4giis.Qacover.Ef
{
    public class TestEvaluationEf : Base
    {
        [SetUp]
        public void SetUpTestData()
        {
            using (EfModel db = new EfModel())
            {
                db.DisableInterceptor(); //evita evaluar estas queries que no son de la aplicacion
                db.Database.EnsureCreated();
                //ojo, estamos usando el contexto interceptado, no pasa nada si son sentencias de actualizacion
                //pero habria que permitir crear un contexto sin interceptar
                //este si hace lectura
                db.SimpleEntitys.RemoveRange(from c in db.SimpleEntitys select c);

                db.Add(new SimpleEfEntity { Id = 1, Num = 0, Text = "abc" });
                db.Add(new SimpleEfEntity { Id = 2, Num = 99, Text = "xyz" });
                db.Add(new SimpleEfEntity { Id = 3, Num = 0, Text = null });
                db.SaveChanges();
            }
        }

        [Test()]
        public virtual void TestEvalEfNoParameters()
        {
            AppSimpleEf app = new AppSimpleEf();
            List<SimpleEfEntity> pojo = app.QueryEfNoParams();
            ClassicAssert.AreEqual(1, pojo.Count);
            ClassicAssert.AreEqual(2, pojo[0].Id);
            ClassicAssert.AreEqual(99, pojo[0].Num);
            ClassicAssert.AreEqual("xyz", pojo[0].Text);
            //compara eliminando las comillas dobles que inserta EntityFramework en tablas y columnas
            String efSql= "SELECT s.Id, s.Num, s.Text FROM SimpleEntitys AS s WHERE (s.Text = 'xyz') AND (s.Num = 99) ORDER BY s.Id";
            AssertEvalResults(efSql, 
                string.Empty, string.Empty,
                  "COVERED   SELECT s.Id , s.Num , s.Text FROM SimpleEntitys AS s WHERE ((s.Text = 'xyz')) AND ((s.Num = 99))\n"
                + "UNCOVERED SELECT s.Id , s.Num , s.Text FROM SimpleEntitys AS s WHERE NOT((s.Text = 'xyz')) AND ((s.Num = 99))\n"
                + "UNCOVERED SELECT s.Id , s.Num , s.Text FROM SimpleEntitys AS s WHERE (s.Text IS NULL) AND ((s.Num = 99))\n"
                + "UNCOVERED SELECT s.Id , s.Num , s.Text FROM SimpleEntitys AS s WHERE (s.Num = 100) AND ((s.Text = 'xyz'))\n"
                + "COVERED   SELECT s.Id , s.Num , s.Text FROM SimpleEntitys AS s WHERE (s.Num = 99) AND ((s.Text = 'xyz'))\n"
                + "UNCOVERED SELECT s.Id , s.Num , s.Text FROM SimpleEntitys AS s WHERE (s.Num = 98) AND ((s.Text = 'xyz'))", 
                  "{}", true, false);
        }
        [Test()]
        public virtual void TestEvalEfParameters()
        {
            options.SetFpcServiceOptions("noboundaries");
            AppSimpleEf app = new AppSimpleEf();
            List<SimpleEfEntity> pojo = app.QueryEfParams(99, "xyz");
            string efSql = "SELECT s.Id, s.Num, s.Text FROM SimpleEntitys AS s WHERE (s.Text = @__param1_0) AND (s.Num > @__param2_1) ORDER BY s.Id";
            AssertEvalResults(efSql, string.Empty, string.Empty,
                "UNCOVERED SELECT s.Id , s.Num , s.Text FROM SimpleEntitys AS s WHERE ((s.Text = 'xyz')) AND ((s.Num > 99))\n"
                + "UNCOVERED SELECT s.Id , s.Num , s.Text FROM SimpleEntitys AS s WHERE NOT((s.Text = 'xyz')) AND ((s.Num > 99))\n"
                + "UNCOVERED SELECT s.Id , s.Num , s.Text FROM SimpleEntitys AS s WHERE (s.Text IS NULL) AND ((s.Num > 99))\n"
                + "COVERED   SELECT s.Id , s.Num , s.Text FROM SimpleEntitys AS s WHERE NOT((s.Num > 99)) AND ((s.Text = 'xyz'))",
                "{@__param1_0='xyz', @__param2_1=99}", true, false);
        }
    }
}
