using Giis.Qacover.Core;
using System;
using System.Data.Common;

namespace Giis.Qacover.Driver
{
    public class EventListener
    {
        public void OnBeforeExecuteQuery(DbConnection nativeConn, String sql, DbParameterCollection parameters)
        {
            //log.debug("***** onBeforeExecuteQuery(PreparedStatementInformation statementInformation)");
            QueryStatement stmt = new StatementAdapter(nativeConn, sql, parameters);
            Controller ctrl = new Controller();
            ctrl.ProcessSql(stmt);
        }
    }
}
