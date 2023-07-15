using System;
using Microsoft.EntityFrameworkCore;
using Giis.Qacover.Ef2driver;
using Giis.Portable.Util;

namespace Test4giis.Qacoverapp.Ef
{
    public class EfModel : Ef2InterceptorContext
    {
        public DbSet<SimpleEfEntity> SimpleEntitys { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            base.OnConfiguring(options); //para que se instale el interceptor
            options.UseSqlite("Data Source=" + FileUtil.GetPath(Parameters.GetProjectRoot(), "TestEf.db"));
        }

    }

    public class SimpleEfEntity
    {
        public int Id { get; set; }
        public Int16 Num { get; set; }
        public string Text { get; set; }
    }
}
