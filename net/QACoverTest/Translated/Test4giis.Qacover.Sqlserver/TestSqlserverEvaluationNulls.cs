using Giis.Qacover.Model;
using Test4giis.Qacover;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////

namespace Test4giis.Qacover.Sqlserver
{
    public class TestSqlserverEvaluationNulls : TestEvaluationNulls
    {
        protected override Variability GetVariant()
        {
            return new Variability("sqlserver");
        }
    }
}