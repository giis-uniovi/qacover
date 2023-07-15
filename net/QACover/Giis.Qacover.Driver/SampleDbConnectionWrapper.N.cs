using System.Data.Common;

namespace Giis.Qacover.Driver
{
    /**
     * Ado net no tiene la posibilidad de manejo como en java con p6spy, por lo que para interceptar
     * las queries, se deberan usar objetos de conexion como este que se instancian a partir de
     * una DbConnection (la nativa).
     * El funcionamiento de la intercepcion es similar pero con un patron mas simple:
     * no se necesita una factoria, sino que la configuracion externa especificara directmente la clase que
     * interceptara los eventos (el event listener).
     * Justo antes de ejecutar la sql invocara al event trigger que se encarga de llamar
     * al event listener mediante reflexion (si existe)
     * El event trigger manitene en una variable compartida que es el string que se ha configurado
     * para indicar el event listener.
     */
    public class SampleDbConnectionWrapper
    {
        DbConnection nativeConn;

        public SampleDbConnectionWrapper(DbConnection connection)
        {
            this.nativeConn = connection;
        }
        public DbConnection getNativeConnection()
        {
            return this.nativeConn;
        }

        public void ExecuteUpdate(string sql)
        {
            using (DbCommand stmt = this.nativeConn.CreateCommand())
            {
                stmt.CommandText = sql;
                stmt.ExecuteNonQuery();
            }
        }

        public DbDataReader ExecuteQuery(string sql)
        {
            new EventTrigger().InvokeListener(nativeConn, sql, null);
            using (DbCommand stmt = this.nativeConn.CreateCommand())
            {
                stmt.CommandText = sql;
                return stmt.ExecuteReader();
            }
        }
        public DbDataReader ExecuteQuery(string sql, DbParameter[] parameters)
        {
            //La invocacion al listener se realizara despues de crear la stmt pues se necesita para
            //tener instanciada la coleccion de parametros
            using (DbCommand stmt = this.nativeConn.CreateCommand())
            {
                foreach(DbParameter parameter in parameters)
                    stmt.Parameters.Add(parameter);
                new EventTrigger().InvokeListener(nativeConn, sql, stmt.Parameters);
                stmt.CommandText = sql;
                return stmt.ExecuteReader();
            }
        }

        public virtual void Close()
        {
            this.nativeConn.Close();
        }

    }
}
