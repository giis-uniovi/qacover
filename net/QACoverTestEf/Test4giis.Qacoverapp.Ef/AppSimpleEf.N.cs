using System.Collections.Generic;
using System.Linq;

namespace Test4giis.Qacoverapp.Ef
{
    class AppSimpleEf
    {
        public AppSimpleEf()
        {
            using (var db = new EfModel())
            {
                db.Database.EnsureCreated();
            }
        }
        public List<TestEfEntity> QueryEfNoParams()
        {
            //String sql = "select id,num,text from test where text=? and num>?";
            using (var db = new EfModel())
            {
                return db.TestEfTable.Where(b => b.Txt == "xyz").Where(b => b.Num == 99)
                    .OrderBy(b => b.Id).ToList<TestEfEntity>();

            }
        }
        public List<TestEfEntity> QueryEfParams(int param2, string param1)
        {
            //String sql = "select id,num,text from test where text=? and num>?";
            using (var db = new EfModel())
            {
                return db.TestEfTable.Where(b => b.Txt == param1).Where(b => b.Num > param2)
                    .OrderBy(b => b.Id).ToList<TestEfEntity>();
            }
        }

    }
}
