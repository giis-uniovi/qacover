using Giis.Qacover.Eval.Query;
using Giis.Qacover.Portable;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Giis.Qacover.Eval.Reader
{
    public class NetQueryStatementReader : IQueryStatementReader
    {
        DbCommand cmd;

        // This implementation does not takes a connection and sql,
        // included both and was created by the StatementAdapter
        // because the way in which parameters were replaced
        public NetQueryStatementReader(DbCommand cmd)
        {
            this.cmd = cmd;
        }
        public bool HasRows()
        {
            try
            {
                DbDataReader reader = cmd.ExecuteReader();
                bool hasNext = reader.Read();
                reader.Close();
                return hasNext;
            }
            catch (Exception e) // just to produce same exception than java version
            {
                throw new QaCoverException("IQueryStatementReader.hasRows", e);
            }
        }

        // Not implemented mutation for .net

        public IList<string[]> GetRows()
        {
            throw new NotImplementedException();
        }

        public bool EqualRows(IList<string[]> expected)
        {
            throw new NotImplementedException();
        }

    }
}