using Giis.Qacover.Core;
using Giis.Qacover.Driver;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Data.Common;

//Prueba de concepto de un interceptor EF Core
//Este interceptor se anyade en OnConfiguring del contexto y se puede deshabilitar
//para no capturar determinadas queries, p.e. cuando se crean datos de prueba
namespace Giis.Qacover.Ef2driver
{
    public class Ef2EventListener : DbCommandInterceptor
    {
        public bool Enabled { get; set; } = true;
 
        public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            if (!Enabled)
                return result;
            //base.ReaderExecuting(command, interceptionContext);
            LogInfo("\n***** EFCommandInterceptor.ReaderExecuting *****", command);
            string sql = command.CommandText;

            //utiliza la conexion del command para ejecutar las reglas (esto no provoca una llamada recursiva al interceptor)
            DbConnection nativeConn = command.Connection;
            DbParameterCollection parameters = command.Parameters;
            QueryStatement stmt = new StatementAdapter(nativeConn, sql, parameters);
            Controller ctrl = new Controller();
            ctrl.ProcessSql(stmt);
            return result;
        }

        private void LogInfo(string commandKind, DbCommand command)
        {
             Console.WriteLine(string.Format("{0}: {1}", commandKind, command.CommandText));
        }

    }
}
