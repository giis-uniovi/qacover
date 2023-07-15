using Giis.Qacover.Model;
using Giis.Qacover.Driver;
using Giis.Qacover.Portable;
using Java.Sql;
using Microsoft.Data.Sqlite;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System;
using Giis.Qacover.Core.Services;

namespace Test4giis.Qacoverapp
{
	/// <summary>
	/// Encapsula los metodos comunes de manejo de BD de una aplicacion
	/// (esta clase debe quedar excluida como origen de la query, debe ser la que la llama).
	/// </summary>
	/// <remarks>
	/// Encapsula los metodos comunes de manejo de BD de una aplicacion
	/// (esta clase debe quedar excluida como origen de la query, debe ser la que la llama).
	/// Para .net se tiene una implementacion alternativa con DbConnection en vez de Connection
	/// que definira la funcion platformJava() con valor false para omitir algunos tests de aspectos no implementados en .net
	/// </remarks>
	public class AppBase
	{
        protected readonly Variability variant;
        //ademas de la variabilidad maneja una factoria para la creacion de conexiones de acuerdo con esta, y una conexion y resultset activos
        private readonly ConnectionFactory cf;
        protected readonly SampleDbConnectionWrapper conn; //la conexion usada es la del wrapper
        internal ResultSet rs;

        public AppBase(Variability targetVariant)
        {
            variant = targetVariant;
            cf = new ConnectionFactory(variant);
            conn = cf.GetConnection();
        }

		public virtual void Close()
		{
			if (rs != null)
				rs.Close();
            if (conn != null)
                conn.Close();
		}

        protected DbConnection GetNativeConnection()
        {
            return cf.GetNativeConnection();
       }

        /// <summary>Ejecucion de un conjunto de actualizaciones contra la base de datos nativa, para que no sean interceptados por p6spy.</summary>
        /// <remarks>
        /// Ejecucion de un conjunto de actualizaciones contra la base de datos nativa, para que no sean interceptados por p6spy.
        /// Utilizado para la carga de datos de test.
        /// </remarks>
        public virtual void ExecuteUpdateNative(string[] sqlArray)
        {
            DbConnection nativeConn = GetNativeConnection();
            DbCommand stmt = nativeConn.CreateCommand();
            foreach (string sql in sqlArray)
            {
                stmt.CommandText = sql;
                stmt.ExecuteNonQuery();
            }
            nativeConn.Close();
        }

        /// <summary>
        /// Borrado de una tabla, si existe
        /// </summary>
        public void DropTable(string tableName)
        {
            if (variant.IsSqlite() || variant.IsH2())
                ExecuteUpdateNative(new string[] { "drop table if exists " + tableName });
            else
            {
                try
                {
                    ExecuteUpdateNative(new string[] { "drop table " + tableName });
                }
                catch (Exception)
                {
                    //ignora excepciones si la tabla ya existe
                }
            }
        }

        //Los siguientes metodos son diferentes queries de lectura utilizando la base de datos de la aplicacion (interceptada por p6spy)

        public virtual ResultSet ExecuteQuery(string sql)
		{
            return new ResultSet(conn.ExecuteQuery(sql));
        }

        public virtual ResultSet ExecuteQueryNulls(string sql, string param1, bool useSetNull)
        {
            //en .net el equivalente a setNull es poner DBNull en el valor
            //(como depende del sgbd usa GetNetNullStringParam)
            if (param1 == null && useSetNull)
                return new ResultSet(conn.ExecuteQuery(JdbcParamsToAssert(sql, 1),
                    new DbParameter[] { GetNetParamNull(1) }));
            else
                return new ResultSet(conn.ExecuteQuery(JdbcParamsToAssert(sql, 1),
                    new DbParameter[] { GetNetParam(1, param1) }));
        }

        public virtual ResultSet ExecuteQuery(string sql, int param1, string param2)
        {
            return new ResultSet(conn.ExecuteQuery(JdbcParamsToAssert(sql, 2), 
                new DbParameter[] { GetNetParam(1,param1), GetNetParam(2,param2) }));
        }

        public virtual ResultSet ExecuteQuery(string sql, int param1, int param2, string param3)
        {
            throw new System.NotImplementedException();
        }

        public virtual ResultSet ExecuteQuery(string sql, string param1, int param2)
		{
            throw new System.NotImplementedException();
        }

        public virtual ResultSet QueryParameters(int param1, string param2)
		{
            throw new System.NotImplementedException();
        }

        public virtual ResultSet QueryParameters(int param1)
		{
            throw new System.NotImplementedException();
        }

        public virtual ResultSet QueryParameters(string param1)
		{
            throw new System.NotImplementedException();
        }

        public ResultSet ExecuteQuery(string sql, Java.Sql.Date param1)
        {
            return new ResultSet(conn.ExecuteQuery(JdbcParamsToAssert(sql, 1),
                new DbParameter[] { GetNetParam(1, param1) }));
        }
        public ResultSet ExecuteQuery(string sql, bool param1)
        {
            return new ResultSet(conn.ExecuteQuery(JdbcParamsToAssert(sql, 1),
                new DbParameter[] { GetNetParam(1, param1) }));
        }

        //Utilidades para crear parametros, especifica del dbms
        private DbParameter GetNetParam(int position, int value)
        {
            DbParameter p1;
            if (variant.IsSqlite())
                p1 = new SqliteParameter("@param" + position, SqliteType.Integer);
            else if (variant.IsSqlServer())
                p1 = new SqlParameter("@param" + position, SqlDbType.Int);
            else
                throw new QaCoverException("AppBase.GetNetParam: Variant '" + variant.GetSgbdName() + "' not supported");
            p1.DbType = DbType.Int32; //para que no muestre comillas al mostrar los parametros como string
            p1.Value = value;
            return p1;
        }
        private DbParameter GetNetParam(int position, string value)
        {
            DbParameter p1;
            if (variant.IsSqlite())
                p1 = new SqliteParameter("@param" + position, SqliteType.Text);
            else if (variant.IsSqlServer())
                p1 = new SqlParameter("@param" + position, SqlDbType.VarChar);
            else
                throw new QaCoverException("AppBase.GetNetParam: Variant '" + variant.GetSgbdName() + "' not supported");
            p1.Value = value;
            return p1;
        }
        private DbParameter GetNetParamNull(int position)
        {
            DbParameter p1;
            if (variant.IsSqlite())
                p1 = new SqliteParameter("@param" + position, System.DBNull.Value);
            else if (variant.IsSqlServer())
                p1 = new SqlParameter("@param" + position, System.DBNull.Value);
            else
                throw new QaCoverException("AppBase.GetNetParam: Variant '" + variant.GetSgbdName() + "' not supported");
            return p1;
        }
        private DbParameter GetNetParam(int position, bool value)
        {
            return GetNetParam(position, value ? 1 : 0); //asumo que los boolean son int
        }
        private DbParameter GetNetParam(int position, Java.Sql.Date value)
        {
            return GetNetParam(position, value.ToString()); //tostring devolvera el formato iso
        }

        //Los parametros de las reglas son ?<indice>? pero en ado.net tienen nombre, por convenio se usara @param<indice>
        public static string RuleParamsToAssert(string sql, int paramCount)
        {
            for (int i = 1; i <= paramCount; i++)
                sql = sql.Replace("?" + i + "?", "@param" + i);
            return sql;
        }
        //Los parametros jdbc son ? pero en ado.net tienen nombre, por convenio se usara @param<indice>
        public static string JdbcParamsToAssert(string sql, int paramCount)
        {
            for (int i = 1; i <= paramCount; i++)
                sql = ReplaceFirst(sql, "?", "@param" + i);
            return sql;
        }
        private static string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
                return text;
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
 
        //para pruebas de stacktrace, este metodo estara ignorado por la configuracion, debe localizarse el que ha llamado a este.
        public virtual StackLocator MyGetStackTraceIgnoredMethod()
		{
			return new StackLocator();
		}
	}
}
