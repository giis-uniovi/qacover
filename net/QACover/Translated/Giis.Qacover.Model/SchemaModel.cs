using Giis.Tdrules.Openapi.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////

namespace Giis.Qacover.Model
{
    /// <summary>
    /// Wrapper of the TdRules schema model
    /// </summary>
    public class SchemaModel
    {
        private TdSchema model;
        public SchemaModel()
        {
            model = new TdSchema();
        }

        public SchemaModel(TdSchema schemaModel)
        {
            model = schemaModel;
        }

        public virtual TdSchema GetModel()
        {
            return model;
        }

        public virtual string GetDbms()
        {
            return model.GetStoretype();
        }

        public override string ToString()
        {
            return model.ToString();
        }
    }
}