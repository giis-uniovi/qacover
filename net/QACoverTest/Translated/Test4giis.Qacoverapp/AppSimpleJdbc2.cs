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
    /// Simple mock application only to test evaluation in different queries
    /// </summary>
    public class AppSimpleJdbc2 : AppBase
    {
        public AppSimpleJdbc2(Variability targetVariant) : base(targetVariant)
        {
        }

        public virtual ResultSet QueryNoParameters1Condition(int param1)
        {
            return ExecuteQuery("select id,num,text from test where num>=" + param1);
        }

        public virtual ResultSet QueryNoParameters2Condition(int param1, string param2)
        {
            return ExecuteQuery("select id,num,text from test where num>" + param1 + " and text='" + param2 + "'");
        }
    }
}