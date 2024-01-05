using Microsoft.EntityFrameworkCore;
using Giis.Qacover.Ef2driver;
using Giis.Portable.Util;
using Giis.Tdrules.Store.Rdb;
using System.Linq;

namespace Test4giis.Qacoverapp.Ef
{
    public class EfModel : Ef2InterceptorContext
    {
        public DbSet<TestEfEntity> TestEfTable { get; set; }

        // Connection string uses the same configuration parameters that are used for ADO.NET tests
        private string GetConnectionString()
        {
            string SetupPath = FileUtil.GetPath(Parameters.GetProjectRoot(), "..", "setup");
            string DatabaseProperties = FileUtil.GetPath(SetupPath, "database.properties");
            string url = new JdbcProperties().GetProp(DatabaseProperties, "qacover.netcore.qacoverdb.sqlserver.url");
            string user = new JdbcProperties().GetProp(DatabaseProperties, "qacover.netcore.qacoverdb.sqlserver.user");
            string EnvironmentProperties = FileUtil.GetPath(SetupPath, "environment.properties");
            string password = new JdbcProperties().GetEnvVar(EnvironmentProperties, "TEST_" + "sqlserver".ToUpper() + "_PWD");
            return url + ";UID=" + user + ";PWD=" + password;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            base.OnConfiguring(options); // base class (Ef2InterceptorContext) will install the interceptor
            options.UseSqlServer(GetConnectionString());
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Disable generated unicode literals
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                         .SelectMany(e => e.GetProperties().Where(p => p.ClrType == typeof(string))))
            {
                property.SetIsUnicode(false);
            }
        }
    }

    public class TestEfEntity
    {
        public int Id { get; set; }
        public int Num { get; set; }
        public string Txt { get; set; }
    }
}
