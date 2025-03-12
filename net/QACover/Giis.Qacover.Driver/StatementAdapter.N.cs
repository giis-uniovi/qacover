using Giis.Qacover.Core;
using System;
using System.Data;
using System.Data.Common;
using Giis.Qacover.Portable;
using System.Linq;
using System.Data.SqlClient;
using Java.Sql;

namespace Giis.Qacover.Driver
{
    public class StatementAdapter : QueryStatement
    {
        private readonly DbConnection nativeConn;
        //Los parametros (independientes de la plataforma java/net) se mantienen en la clase base,
        //pero en .net tambien se guardan aqui los parametros nativos de ado net
        private readonly DbParameterCollection nativeParams = null;

        public StatementAdapter(DbConnection db, string sql, DbParameterCollection dbParams)
        {
            try
            {
                if (base.GetFaultInjector() != null && base.GetFaultInjector().IsUnexpectedException())
                    throw new QaCoverException(base.GetFaultInjector().GetUnexpectedException());
                nativeConn = db;
                base.sql = sql;
                //anyade los parametros nativos y en la clase base, si existen
                this.nativeParams = dbParams;
                if (dbParams == null)
                    return;
                foreach (DbParameter param in dbParams)
                {
                    string value;
                    if (param.Value == null || param.Value == DBNull.Value)
                    {
                        value = "NULL";
                        param.Value = DBNull.Value; //si no se hace causara error que dice que se requiere un valor
                    }
                    else
                    {
                        value = IsStringLike(param.DbType) ? "'" + param.Value.ToString() + "'" : param.Value.ToString();
                    }
                    base.parameters.PutItem(param.ParameterName.ToString(), value);
                }
            }
            catch (Exception e)
            {
                this.exception = e;
            }
        }
        private bool IsStringLike(DbType tp)
        {
            return tp == DbType.AnsiString || tp == DbType.AnsiStringFixedLength
                || tp == DbType.Date || tp == DbType.DateTime || tp == DbType.DateTime2
                || tp == DbType.Guid || tp == DbType.String || tp == DbType.StringFixedLength
                || tp == DbType.Time || tp == DbType.Xml;
        }
        protected override string GetDatabaseDialectFormat()
        {
            throw new NotSupportedException("SpyStatementAdapter.getDatabaseDialectFormat not implemented in netcore platform");
        }

        public override Connection GetConnection()
        {
            return new Connection(nativeConn);
        }

        public override IQueryStatementReader GetReader(String sql)
        {
                DbCommand cmd = this.nativeConn.CreateCommand();
                //El remplazo de parametros tiene tres situaciones:
                //-no hay parametros
                //-los parametros son inferidos (habra parametros en la clase base y no en esta):
                // se remplazan igual que en Java con su valor string modificando la sql
                //-hay parametros especificados en la query, se aplican directamente a la query
                // (no es fiable en este caso modificar la sql, se ha visto p.e. en sqlite enteros 
                // que tienen como tipo interno un string). Esto causa que otros metodos que 
                // utilicen GetSqlWithValues para hacer logs puedan no mostrar correctamente la sql.
                cmd.CommandText = sql;
                if (cmd is SqlCommand && this.nativeParams != null)
                    AddParameters(cmd, this.nativeParams);
                else if (this.parameters != null && this.parameters.GetSize() > 0)
                    cmd.CommandText = GetSqlWithValues(sql);

                return new QueryStatementReader(cmd);
        }
        private void AddParameters(DbCommand cmd, DbParameterCollection parameters)
        {
            //Con sqlserver al anyadir parametros causa la excepcion:
            //The SqlParameter is already contained by another SqlParameterCollection
            //Es por que los parametros recuerdan que han sido utilizados en otro command (el de la ejecucion de la query principal)
            //En este caso utiliza un metodo para clonarlos y anyadirlos al comand
            //https://stackoverflow.com/questions/4778775/copy-parameters-from-dbcommand-to-another-dbcommand
            if (cmd is SqlCommand)
            {
                var nsp = parameters.Cast<ICloneable>().Select(x => x.Clone() as SqlParameter).Where(x => x != null).ToArray();
                cmd.Parameters.AddRange(nsp);
            } 
            else
            {
                foreach (DbParameter param in this.nativeParams)
                    cmd.Parameters.Add(param);
            }
        }

    }

}
