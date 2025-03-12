using Java.Sql;
using Java.Util;

using NLog;
using Giis.Portable.Util;
using Giis.Qacover.Core;
using Giis.Qacover.Core.Services;
using Giis.Qacover.Model;
using Test4giis.Qacoverapp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////

namespace Test4giis.Qacover
{
    /// <summary>
    /// Base class for all tests
    /// Includes the setup for the objects that are used by tests:
    /// - Variability to consider the platform and SGBD
    /// - Setup of the mock test application (qacoverapp package) that
    ///   executes queries under the QACover control
    /// - Store object for rules
    /// - Configuration
    /// 
    /// Implementation of tests is under the qacover package (with Sqlite) that extends this class.
    /// Test for other platforms are in subpackages, inherit the Sqlite tests
    /// and customize the appropriate variables to reuse tests from their superclass.
    /// 
    /// Most of tests are at the integration level with a DBMS, the jdbcdriver, p6spy and the rules generated.
    /// Integration tests according the maven convention (IT) are those that
    /// execute outside of maven using the generated jars.
    /// </summary>
    public class Base
    {
        private static readonly Logger log = Giis.Portable.Util.NLogUtil.GetLogger(typeof(Base));
        protected Variability variant;
        protected ResultSet rs; // the result of the execution of the mock test application
        protected Configuration options;
        protected FaultInjector faultInjector;
        private static readonly int MAX_PARAMS = 3;
        static Base() { Giis.Qacover.Driver.EventTrigger.SetListenerClassName("Giis.Qacover.Driver.EventListener"); }
        
        [NUnit.Framework.SetUp]
        public virtual void SetUp()
        {
            log.Info("*** CURRENT TEST - " + NUnit.Framework.TestContext.CurrentContext.Test.Name);
            variant = GetVariant();
            log.Info("*** CURRENT DBMS - " + variant.GetSgbdName());
            rs = null;
            options = ConfigureTestOptions();
            new StoreService(options).DropRules().DropLast();
            QueryStatement.SetFaultInjector(null);
            //Locale.SetDefault(new Locale("en", "US"));
        }

        // Default configuration for tests
        public static Configuration ConfigureTestOptions()
        {
            return Configuration.GetInstance().Reset().SetName("qacovertest").SetRuleCacheFolder(FileUtil.GetPath(Parameters.GetProjectRoot(), ".tdrules-cache"));
        }

        [NUnit.Framework.TearDown]
        public virtual void TearDown()
        {
            if (rs != null)
                rs.Close();
        }

        protected virtual Variability GetVariant()
        {
            if (new Variability().IsJava4())
                return new Variability("h2");
            else
                return new Variability("sqlite");
        }

        protected virtual void AssertEqualsIgnoreCase(string expected, string actual)
        {
            NUnit.Framework.Legacy.ClassicAssert.AreEqual(expected.ToLower(), actual.ToLower());
        }

        //for compatibility with .NET
        protected virtual void AssertEqualsCs(string expected, string actual)
        {
            if (new Variability().IsNetCore())
                AssertEqualsIgnoreCase(expected, actual);
            else
                NUnit.Framework.Legacy.ClassicAssert.AreEqual(expected, actual);
        }

        protected virtual void AssertContains(string expected, string actual)
        {
            if (!actual.Contains(expected))
                NUnit.Framework.Legacy.ClassicAssert.Fail("Expected not contained in actual: " + actual);
        }

        /// <summary>
        /// Custom asserts to check the results of evaluation of rules
        /// </summary>
        protected virtual void AssertEvalResults(string expSql, string expOutput, string actOutput, string expRule)
        {
            AssertEvalResults(expSql, expOutput, actOutput, expRule, null, false, false);
        }

        protected virtual void AssertEvalResults(string expSql, string expOutput, string actOutput, string expRule, string expParams)
        {
            AssertEvalResults(expSql, expOutput, actOutput, expRule, expParams, false, false);
        }

        protected virtual void AssertEvalResults(string expSql, string expOutput, string actOutput, string expRule, string expParams, bool removeQuotesAndLines, bool convertNetParams)
        {
            AssertEvalResults(true, expSql, expOutput, actOutput, expRule, expParams, removeQuotesAndLines, convertNetParams);
        }

        protected virtual void AssertEvalResults(bool expSuccess, string expSql, string expOutput, string actOutput, string expRule, string expParams, bool removeQuotesAndLines, bool convertNetParams)
        {
            StoreService store = StoreService.GetLast();
            NUnit.Framework.Legacy.ClassicAssert.AreEqual(expSuccess ? "success" : "", store.GetLastGenStatus());
            string sql = store.GetLastSqlRun();

            // needed to test Entity Framework
            sql = removeQuotesAndLines ? sql.Replace("\"", "").Replace("\n", " ").Replace("\r", "") : sql;

            // to compare parametrized queries in ADO.NET
            expSql = convertNetParams ? AppBase.JdbcParamsToAssert(expSql, MAX_PARAMS) : expSql;
            log.Debug("Tested sql: " + sql);
            NUnit.Framework.Legacy.ClassicAssert.AreEqual(expSql, sql);
            NUnit.Framework.Legacy.ClassicAssert.AreEqual(expOutput, actOutput);
            string rules = removeQuotesAndLines ? store.GetLastRulesLog().Replace("\"", "") : store.GetLastRulesLog();
            NUnit.Framework.Legacy.ClassicAssert.AreEqual(expRule, rules);
            if (expParams != null)
            {
                expParams = convertNetParams ? AppBase.RuleParamsToAssert(expParams, MAX_PARAMS) : expParams;
                NUnit.Framework.Legacy.ClassicAssert.AreEqual(expParams, store.GetLastParametersRun());
            }
        }
    }
}