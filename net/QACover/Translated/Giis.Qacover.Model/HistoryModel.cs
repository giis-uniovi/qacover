using Java.Util;
using Giis.Portable.Util;
using Giis.Portable.Xml.Tiny;
using Giis.Tdrules.Model.IO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////

namespace Giis.Qacover.Model
{
    /// <summary>
    /// The history store holds sequentially the reference to each query evaluation;
    /// Each query evaluation is represented in an instance of this class that
    /// is instantiated from a line read from the history store.
    /// 
    /// There are two formats that can be read:
    /// V1 (legacy): a csv separated with a vertical bar: timestamp | query key | parameters in xml
    /// V2 (current): a json string with timestam, query key and parameters
    /// 
    /// Currently QACover writes in V2 format, but V1 is keep for compatibility
    /// with legacy stores created before QACover version 2.0.0
    /// </summary>
    public class HistoryModel
    {
        private HistoryDao dao; // data is kept in the dao for easier serialization
        /// <summary>
        /// Creates an history model with the given parameters from a query that has been evaluated
        /// </summary>
        public HistoryModel(DateTime timestamp, string key, QueryParameters @params, ResultVector resultVector)
        {
            this.dao = new HistoryDao();
            this.dao.at = JavaCs.GetIsoDate(timestamp);
            this.dao.key = key;
            this.dao.@params = @params.ToDao();
            this.dao.result = resultVector.ToString();
        }

        /// <summary>
        /// Create an history model from a string that is read from the history storage
        /// (supports V1 and V2 formats)
        /// </summary>
        public HistoryModel(string item)
        {
            if (item.StartsWith("{"))
                LoadHistoryItemV2(item);
            else
                LoadHistoryItemV1(item);
        }

        private void LoadHistoryItemV2(string item)
        {
            this.dao = (HistoryDao)new ModelJsonSerializer().Deserialize(item, typeof(HistoryDao));
        }

        private void LoadHistoryItemV1(string item)
        {
            string[] splitted = JavaCs.SplitByBar(item); // should have 3 at least
            this.dao = new HistoryDao();
            this.dao.at = JavaCs.GetIsoDate(JavaCs.ParseIsoDate(splitted[0]));
            this.dao.key = splitted[1];

            // If more than 3, joins the remaining items
            string paramStr = "";
            for (int i = 2; i < splitted.Length; i++)
                paramStr += (i == 2 ? "" : "|") + splitted[i]; // NOSONAR
            this.dao.@params = ParamsFromXml(paramStr);
        }

        public virtual string GetKey()
        {
            return dao.key;
        }

        public virtual string GetResult()
        {
            return dao.result;
        }

        public virtual IList<ParameterDao> GetParams()
        {
            return dao.@params;
        }

        public virtual string GetParamsJson()
        {
            return new ModelJsonSerializer().Serialize(dao.@params, false);
        }

        public virtual string GetParamsXml()
        {
            return ParamsToXml(dao.@params);
        }

        private string ParamsToXml(IList<ParameterDao> @params)
        {
            StringBuilder sb = new StringBuilder();
            foreach (ParameterDao param in @params)
                sb.Append("<parameter name=\"").Append(XNodeAbstract.EncodeAttribute(param.name)).Append("\" value=\"").Append(XNodeAbstract.EncodeAttribute(param.value)).Append("\" />");
            return "<parameters>" + sb.ToString() + "</parameters>";
        }

        private IList<ParameterDao> ParamsFromXml(string paramXml)
        {
            IList<ParameterDao> paramDao = new List<ParameterDao>();
            IList<XNode> paramNodes = new XNode(paramXml).GetChildren("parameter");
            foreach (XNode paramNode in paramNodes)
                paramDao.Add(new ParameterDao(XNodeAbstract.DecodeAttribute(paramNode.GetAttribute("name")), XNodeAbstract.DecodeAttribute(paramNode.GetAttribute("value"))));
            return paramDao;
        }

        public virtual string ToStringV2()
        {
            return new ModelJsonSerializer().Serialize(dao, false);
        }

        public virtual string ToStringV1()
        {
            return dao.at + "|" + dao.key + "|" + GetParamsXml();
        }
    }
}