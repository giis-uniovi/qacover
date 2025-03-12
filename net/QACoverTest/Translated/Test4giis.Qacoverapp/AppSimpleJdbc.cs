using Java.Sql;
using Giis.Qacover.Core.Services;
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
    /// Simple mock application
    /// </summary>
    public class AppSimpleJdbc : AppBase
    {
        public AppSimpleJdbc(Variability targetVariant) : base(targetVariant)
        {
        }

        //para pruebas de stacktrace
        public virtual StackLocator MyGetStackTraceTargetMethod()
        {
            return MyGetStackTraceIgnoredMethod();
        }

        public virtual ResultSet QueryNoParameters1Condition(int param1)
        {
            return ExecuteQuery("select id,num,text from test where num>=" + param1);
        }

        public virtual ResultSet QueryNoParameters2Condition(int param1, string param2)
        {
            return ExecuteQuery("select id,num,text from test where num>" + param1 + " and text='" + param2 + "'");
        }

        public override ResultSet QueryParameters(int param1, string param2)
        {
            return ExecuteQuery("select id,num,text from test where num>? and text=?", param1, param2);
        }

        //multiples queries en un metodo (misma y distinta linea)
        public virtual void QueryDifferentSingleLine(bool run1, int param1, bool run2, string param2)
        {

            //dos queries en misma linea con distintas reglas
            if (run1)
            {
                rs = ExecuteQuery("select * from test where num=" + param1);
                rs.Close();
            }

            if (run2)
            {
                rs = ExecuteQuery("select * from test where text=" + param2);
                rs.Close();
            }
        }

        public virtual void QueryEqualSingleLine(string param1, string param2)
        {
            rs = ExecuteQuery("select * from test where text=" + param1);
            rs.Close();
            ExecuteQuery("select * from test where text=" + param2);
        }

        public virtual void QueryEqualDifferentLine(string param1, string param2)
        {
            rs = ExecuteQuery("select * from test where text=" + param1);
            rs.Close();

            //la misma en otra linea
            rs = ExecuteQuery("select * from test where text=" + param2);
        }

        public virtual ResultSet QueryNoConditions()
        {
            return ExecuteQuery("select id,num,text from test");
        }

        public virtual ResultSet QueryNoParametersQuotes(string param1, bool bracketAtTable, bool bracketAtColumn)
        {
            return ExecuteQuery("select id,num,text from " + QuoteIdentifier("test", bracketAtTable) + " where " + QuoteIdentifier("text", bracketAtColumn) + "='" + param1 + "'");
        }

        public virtual ResultSet QueryNoParametersBrackets(string param1, bool bracketAtTable, bool bracketAtColumn)
        {
            return ExecuteQuery("select id,num,text from " + (bracketAtTable ? "[test]" : "test") + " where " + (bracketAtColumn ? "[text]" : "text") + "='" + param1 + "'");
        }

        private string QuoteIdentifier(string name, bool doQuote)
        {
            if (doQuote)
                return "\"" + (variant.IsOracle() || variant.IsH2() ? name.ToUpper() : name) + "\"";
            else
                return name;
        }

        public virtual ResultSet QueryParametersNamed(int param1, int param2, string param3)
        {
            return ExecuteQuery("/* params=?1?,?1?,?2? */ select id,num,text from test where id=? or num=? or text=?", param1, param2, param3);
        }

        // This is for testing mutants
        public virtual ResultSet QueryMutParameters(string param1)
        {
            return ExecuteQueryMut("select id,txt from test where txt=?", param1);
        }
    }
}