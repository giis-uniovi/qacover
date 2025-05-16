using Giis.Qacover.Model;
using Giis.Qacover.Portable;
using System.Data;
using System.Data.Common;

namespace Test4giis.Qacoverapp
{
    /// <summary>
    /// This is the only class that refers dbms dependent objects
    /// </summary>
    public class DbObjectFactory
    {
        public static DbConnection GetDbConnection(Variability variant, string url)
        {
            if (variant.IsSqlite())
                //return new Microsoft.Data.Sqlite.SqliteConnection(url);
                return new System.Data.SQLite.SQLiteConnection(url);
            else if (variant.IsSqlServer())
                return new Microsoft.Data.SqlClient.SqlConnection(url);
            else
                throw new QaCoverException("ConnectionFactory.GetNativeConnection: Variant '" + variant.GetSgbdName() + "' not supported");
        }

        public static DbParameter GetDbParameter(Variability variant, string name, int value)
        {
            DbParameter p1;
            if (variant.IsSqlite())
                //p1 = new Microsoft.Data.Sqlite.SqliteParameter(name, Microsoft.Data.Sqlite.SqliteType.Integer);
                p1 = new System.Data.SQLite.SQLiteParameter(name, DbType.Int32);
            else if (variant.IsSqlServer())
                p1 = new Microsoft.Data.SqlClient.SqlParameter(name, SqlDbType.Int);
            else
                throw new QaCoverException("AppBase.GetNetParam: Variant '" + variant.GetSgbdName() + "' not supported");
            p1.DbType = DbType.Int32; //para que no muestre comillas al mostrar los parametros como string
            p1.Value = value;
            return p1;
        }
        public static DbParameter GetDbParameter(Variability variant, string name, string value)
        {
            DbParameter p1;
            if (variant.IsSqlite())
                //p1 = new Microsoft.Data.Sqlite.SqliteParameter(name, Microsoft.Data.Sqlite.SqliteType.Text);
                p1 = new System.Data.SQLite.SQLiteParameter(name, DbType.String);
            else if (variant.IsSqlServer())
                p1 = new Microsoft.Data.SqlClient.SqlParameter(name, SqlDbType.VarChar);
            else
                throw new QaCoverException("AppBase.GetNetParam: Variant '" + variant.GetSgbdName() + "' not supported");
            p1.Value = value;
            return p1;
        }
        public static DbParameter GetDbParameterNull(Variability variant, string name)
        {
            DbParameter p1;
            if (variant.IsSqlite())
                //p1 = new Microsoft.Data.Sqlite.SqliteParameter(name, System.DBNull.Value);
                p1 = new System.Data.SQLite.SQLiteParameter(name, System.DBNull.Value);
            else if (variant.IsSqlServer())
                p1 = new Microsoft.Data.SqlClient.SqlParameter(name, System.DBNull.Value);
            else
                throw new QaCoverException("AppBase.GetNetParam: Variant '" + variant.GetSgbdName() + "' not supported");
            return p1;
        }

    }
}
