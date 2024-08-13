using System.Collections.Generic;
using Giis.Qacover.Model;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Test4giis.Qacoverapp.Ef;

namespace Test4giis.Qacover.Ef
{
    /**
     * This test requires the same SQLServer database that the one used in ADO.NET tests,
     * the setup of test data and query execution are done using Entity Framework
     * thorugh the mock app defined in AppSimpleEf
     */
    public class TestEvaluationEf : Base
    {
        protected override Variability GetVariant()
        {
            return new Variability("sqlserver");
        }

        [SetUp]
        public void SetUpTestData()
        {
            using (EfModel db = new EfModel())
            {
                // Deactivate the interceptor to forget the setup queries
                // (although in this case queries are only updates)
                db.DisableInterceptor(); //evita evaluar estas queries que no son de la aplicacion
                // Setup a fresh table to hold the test data
                try
                {
                    db.Database.ExecuteSqlRaw("drop table TestEfTable");
                }
                catch (Microsoft.Data.SqlClient.SqlException) { }
                db.Database.ExecuteSqlRaw("CREATE TABLE TestEfTable (Id  integer NOT NULL CONSTRAINT PK_TestEfTable PRIMARY KEY , Num INTEGER NOT NULL, Txt varchar(32) NULL)");
                //db.TestEfTable.RemoveRange(from c in db.TestEfTable select c);

                // Setup test data for all tests to cover some rules and do not cover oters
                db.Add(new TestEfEntity { Id = 1, Num = 0, Txt = "abc" });
                db.Add(new TestEfEntity { Id = 2, Num = 99, Txt = "xyz" });
                db.Add(new TestEfEntity { Id = 3, Num = 0, Txt = null });
                db.SaveChanges();
            }
        }

        [Test()]
        public virtual void TestEvalEfNoParameters()
        {
            AppSimpleEf app = new AppSimpleEf();
            List<TestEfEntity> pojo = app.QueryEfNoParams();
            ClassicAssert.AreEqual(1, pojo.Count);
            ClassicAssert.AreEqual(2, pojo[0].Id);
            ClassicAssert.AreEqual(99, pojo[0].Num);
            ClassicAssert.AreEqual("xyz", pojo[0].Txt);
            //compara eliminando las comillas dobles que inserta EntityFramework en tablas y columnas
            string efSql= "SELECT [t].[Id], [t].[Num], [t].[Txt] FROM [TestEfTable] AS [t] WHERE [t].[Txt] = 'xyz' AND [t].[Num] = 99 ORDER BY [t].[Id]";
            AssertEvalResults(efSql,
                string.Empty, string.Empty,
                    "COVERED   SELECT [t].[Id] , [t].[Num] , [t].[Txt] FROM [TestEfTable] AS [t] WHERE ([t].[Txt] = 'xyz') AND ([t].[Num] = 99)\n"
                + "UNCOVERED SELECT [t].[Id] , [t].[Num] , [t].[Txt] FROM [TestEfTable] AS [t] WHERE NOT([t].[Txt] = 'xyz') AND ([t].[Num] = 99)\n"
                + "UNCOVERED SELECT [t].[Id] , [t].[Num] , [t].[Txt] FROM [TestEfTable] AS [t] WHERE ([t].[Txt] IS NULL) AND ([t].[Num] = 99)\n"
                + "UNCOVERED SELECT [t].[Id] , [t].[Num] , [t].[Txt] FROM [TestEfTable] AS [t] WHERE ([t].[Num] = 100) AND ([t].[Txt] = 'xyz')\n"
                + "COVERED   SELECT [t].[Id] , [t].[Num] , [t].[Txt] FROM [TestEfTable] AS [t] WHERE ([t].[Num] = 99) AND ([t].[Txt] = 'xyz')\n"
                + "UNCOVERED SELECT [t].[Id] , [t].[Num] , [t].[Txt] FROM [TestEfTable] AS [t] WHERE ([t].[Num] = 98) AND ([t].[Txt] = 'xyz')",
                  "{}", true, false);
        }
        [Test()]
        public virtual void TestEvalEfParameters()
        {
            options.SetRuleOptions("noboundaries");
            AppSimpleEf app = new AppSimpleEf();
            List<TestEfEntity> pojo = app.QueryEfParams(99, "xyz");
            string efSql = "SELECT [t].[Id], [t].[Num], [t].[Txt] FROM [TestEfTable] AS [t] WHERE [t].[Txt] = @__param1_0 AND [t].[Num] > @__param2_1 ORDER BY [t].[Id]";
            AssertEvalResults(efSql, string.Empty, string.Empty,
                "UNCOVERED SELECT [t].[Id] , [t].[Num] , [t].[Txt] FROM [TestEfTable] AS [t] WHERE ([t].[Txt] = 'xyz') AND ([t].[Num] > 99)\n"
                + "UNCOVERED SELECT [t].[Id] , [t].[Num] , [t].[Txt] FROM [TestEfTable] AS [t] WHERE NOT([t].[Txt] = 'xyz') AND ([t].[Num] > 99)\n"
                + "UNCOVERED SELECT [t].[Id] , [t].[Num] , [t].[Txt] FROM [TestEfTable] AS [t] WHERE ([t].[Txt] IS NULL) AND ([t].[Num] > 99)\n"
                + "COVERED   SELECT [t].[Id] , [t].[Num] , [t].[Txt] FROM [TestEfTable] AS [t] WHERE NOT([t].[Num] > 99) AND ([t].[Txt] = 'xyz')",
                "{@__param1_0='xyz', @__param2_1=99}", true, false);
        }
    }
}
