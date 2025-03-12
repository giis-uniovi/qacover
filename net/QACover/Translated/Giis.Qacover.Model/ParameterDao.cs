using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////

namespace Giis.Qacover.Model
{
    public class ParameterDao
    {
        public string name; // NOSONAR public access to allow direct serialization in the .NET version
        public string value; // NOSONAR
        public ParameterDao()
        {
        }

        public ParameterDao(string name, string value)
        {
            this.name = name;
            this.value = value;
        }

        public override string ToString()
        {
            return this.name + "=" + this.value;
        }
    }
}