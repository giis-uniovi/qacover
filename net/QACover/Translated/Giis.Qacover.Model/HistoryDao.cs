using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////

namespace Giis.Qacover.Model
{
    public class HistoryDao
    {
        public string at; // NOSONAR public access to allow direct serialization in the .NET version
        public string key; // NOSONAR
        public IList<ParameterDao> @params = new List<ParameterDao>(); // NOSONAR
        public string result; // NOSONAR
    }
}