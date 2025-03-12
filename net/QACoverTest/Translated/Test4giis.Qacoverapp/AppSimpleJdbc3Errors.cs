using Java.Sql;
using Giis.Qacover.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////

namespace Test4giis.Qacoverapp
{
    /// <summary>
    /// Simple mock application to test errors
    /// </summary>
    public class AppSimpleJdbc3Errors : AppBase
    {
        public AppSimpleJdbc3Errors(Variability targetVariant) : base(targetVariant)
        {
        }

        public virtual ResultSet Query0Errors()
        {
            return ExecuteQuery("select id,num,text from test where num<9");
        }

        public virtual ResultSet Query1ErrorAtQuery()
        {
            return ExecuteQuery("select id,num,text from test where num<10");
        }

        public virtual ResultSet Query1ErrorAtRule()
        {
            return ExecuteQuery("select id,num,text from test where num < 11");
        }

        public virtual ResultSet QueryMultipleErrors()
        {
            return ExecuteQuery("select id,num,text from test where num<9");
        }
    }
}