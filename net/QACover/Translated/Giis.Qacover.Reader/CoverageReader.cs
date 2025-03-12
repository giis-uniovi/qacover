using Java.Util;
using NLog;
using Giis.Qacover.Model;
using Giis.Qacover.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////

namespace Giis.Qacover.Reader
{
    /// <summary>
    /// This is the API to access to all information about the evaluation, grouped by
    /// different criteria. Creates collections (CoverageCollection or QueryCollection)
    /// to lazy access the stored information.
    /// </summary>
    public class CoverageReader
    {
        private static readonly Logger log = Giis.Portable.Util.NLogUtil.GetLogger(typeof(CoverageReader));
        private string rulesFolder;
        /// <summary>
        /// Instantiation from the store location
        /// </summary>
        public CoverageReader(string rulesFolder)
        {
            this.rulesFolder = rulesFolder;
        }

        /// <summary>
        /// Gets a CoverageCollection with an item for each class,
        /// the QueryCollections will have all queries in this class
        /// </summary>
        public virtual CoverageCollection GetByClass()
        {
            log.Trace("CoverageReader.GetByClass, Processing files in folder: " + rulesFolder);
            IList<string> files = new LocalStore(rulesFolder).GetRuleItems();

            // saves the query collection for each class
            Map<string, QueryCollection> index = new TreeMap<string, QueryCollection>();
            CoverageCollection target = new CoverageCollection(); // collection to return
            foreach (string fileName in files)
            {
                log.Trace("Processing file: " + fileName);
                string className = new QueryKey(fileName).GetClassName();
                QueryCollection current;
                if (index.ContainsKey(className))
                {

                    // already indexed
                    current = index[className];
                } // new, adds to map of query collections
                else
                {

                    // new, adds to map of query collections
                    current = new QueryCollection(rulesFolder, className);
                    index.Put(className, current);
                    target.Add(current);
                }

                current.Add(new QueryReader(rulesFolder, fileName));
            }

            return target;
        }

        /// <summary>
        /// Gets a list of History models with data of the executions in time order
        /// </summary>
        public virtual HistoryReader GetHistory()
        {
            log.Trace("CoverageReader.getHistory, Processing history for folder: " + rulesFolder);
            LocalStore storage = new LocalStore(rulesFolder);
            IList<HistoryModel> hitems = storage.GetHistoryItems();
            return new HistoryReader(hitems);
        }
    }
}