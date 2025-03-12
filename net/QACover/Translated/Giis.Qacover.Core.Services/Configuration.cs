using Java.Util;
using NLog;
using Giis.Portable.Util;
using Giis.Qacover.Model;
using Giis.Qacover.Portable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////

namespace Giis.Qacover.Core.Services
{
    /// <summary>
    /// Global configuration options (singleton), read from properties file
    /// </summary>
    public class Configuration
    {
        public static readonly string OPTIONS_FILE_PROPERTY = "qacover.properties";
        private static readonly string QACOVER_NAME = "qacover";
        private static readonly Logger log = Giis.Portable.Util.NLogUtil.GetLogger(typeof(Configuration));
        private Properties properties;
        private static Configuration instance;
        private string name = QACOVER_NAME;
        private string storeRulesLocation;
        private string ruleCache;
        private string storeReportsLocation;
        private string ruleCriterion;
        private string ruleUrl;
        private string ruleOptions;
        private bool optimizeRuleEvaluation;
        private bool inferQueryParameters;
        private IList<string> stackExclusions;
        private IList<string> stackInclusions;
        private IList<string> classExclusions; // do not generate for classes with this name (exact match)
        private IList<string> tableExclusionsExact; // do not generate for queries using any of this tables (exact match)
        private string dbStoretype = null; // null means not configured, once configured will not be changed
        private Configuration()
        {
            Reset();
        }

        public static Configuration GetInstance()
        {
            if (instance == null)
                instance = new Configuration();
            return instance;
        }

        public virtual Configuration Reset()
        {
            this.properties = GetProperties(OPTIONS_FILE_PROPERTY);

            // First verification that there are no old properties
            VerifyRemovedProperty("qacover.optimize.rule.evaluation", "It MUST be renamed to: qacover.rule.optimize.evaluation");
            VerifyRemovedProperty("qacover.fpc.url", "It MUST be renamed to: qacover.rule.url");
            VerifyRemovedProperty("qacover.fpc.options", "It MUST be renamed to: qacover.rule.options");
            string storeRootLocation = GetProperty("qacover.store.root", Parameters.GetProjectRoot());
            string defaultRulesSubDir = FileUtil.GetPath(Parameters.GetReportSubdir(), QACOVER_NAME, "rules");
            string defaultReportsSubDir = FileUtil.GetPath(Parameters.GetReportSubdir(), QACOVER_NAME, "reports");
            storeRulesLocation = FileUtil.GetPath(storeRootLocation, GetProperty("qacover.store.rules", defaultRulesSubDir));
            storeReportsLocation = FileUtil.GetPath(storeRootLocation, GetProperty("qacover.store.reports", defaultReportsSubDir));
            ruleCriterion = GetProperty("qacover.rule.criterion", "fpc");
            if (!"fpc".Equals(ruleCriterion) && !"mutation".Equals(ruleCriterion))
            {
                log.Warn("Invalid value " + ruleCriterion + ". Using fpc as fallback");
                ruleCriterion = "fpc";
            }

            ruleUrl = GetProperty("qacover.rule.url", "https://in2test.lsi.uniovi.es/tdrules/api/v4");
            ruleOptions = GetProperty("qacover.rule.options", "");
            ruleCache = GetProperty("qacover.rule.cache", "");
            optimizeRuleEvaluation = JavaCs.EqualsIgnoreCase("true", GetProperty("qacover.rule.optimize.evaluation", "false"));
            inferQueryParameters = JavaCs.EqualsIgnoreCase("true", GetProperty("qacover.query.infer.parameters", "false"));

            // Includes some predefined stack exclusions
            stackExclusions = new List<string>();
            stackExclusions.Add("giis.portable.");
            stackExclusions.Add("giis.qacover.");
            stackExclusions.Add("com.p6spy.");
            stackExclusions.Add("java.");
            stackExclusions.Add("javax.");
            stackExclusions.Add("jdk.internal."); // for jdk11 (not needed in jdk8)
            stackExclusions.Add("System."); // for .net
            stackExclusions.Add("InvokeStub_EventListener"); // since .net8.0
            foreach (string item in GetMultiValueProperty("qacover.stack.exclusions"))
                stackExclusions.Add(item);
            stackInclusions = GetMultiValueProperty("qacover.stack.inclusions");
            classExclusions = GetMultiValueProperty("qacover.class.exclusions");
            tableExclusionsExact = GetMultiValueProperty("qacover.table.exclusions.exact");
            return this;
        }

        /// <summary>
        /// Loads the properties in the order:
        /// - system properties
        /// - class path
        /// - root folder
        /// </summary>
        public virtual Properties GetProperties(string propFileName)
        {
            Properties prop = null;
            if (new Variability().IsJava())
            {

                // Uses a custom method with custom implementation for java 1.4
                // (the general method raises exception when searching system.properties, GitLab #7
                log.Info(propFileName + " search in system property");
                prop = new Jdk14PropertiesFactory().GetPropertiesFromSystemProperty(new Variability().IsJava4(), propFileName, propFileName);
                if (prop != null)
                {
                    log.Info(propFileName + " found in system property");
                    return prop;
                }

                log.Warn(propFileName + " not found in system property, trying classpath");
            }

            if (new Variability().IsJava())
            {
                log.Info(propFileName + " search in classpath");
                prop = new PropertiesFactory().GetPropertiesFromClassPath(propFileName);
                if (prop != null)
                {
                    log.Info(propFileName + " found in classpath");
                    return prop;
                }
            }

            log.Warn(propFileName + " not found in classpath, trying root path");
            string propFileNameWithPath = FileUtil.GetPath(Parameters.GetProjectRoot(), propFileName);
            log.Info(propFileName + " search in root path: " + propFileNameWithPath);
            prop = new PropertiesFactory().GetPropertiesFromFilename(propFileNameWithPath);
            if (prop != null)
            {
                log.Info(propFileName + " found in root path");
                return prop;
            }

            log.Error(propFileName + " not found in root path. Can't continue");
            throw new QaCoverException(propFileName + " not found in root path: " + FileUtil.GetFullPath(propFileNameWithPath) + " - Can't continue");
        }

        /// <summary>
        /// Reads a property with a default value.
        /// On net, makes a first read from an environment variable
        /// </summary>
        private string GetProperty(string name, string defaultValue)
        {
            string value = null;

            // On .net docker environment variables replace configuration files
            if (new Variability().IsNetCore())
            {
                value = JavaCs.GetEnvironmentVariable(name);
                if (value != null)
                {
                    log.Debug("Get parameter from environment: " + name + "=" + value); // NOSONAR for docker console
                    return value;
                }
            }


            // Avoid exception when running in docker (no properties files)
            try
            {
                value = properties.GetProperty(name, defaultValue);
                log.Debug("Get parameter from qacover.properties: " + name + "=" + value); // NOSONAR for docker console
            }
            catch (Exception e)
            {
                value = defaultValue;
                log.Debug("qacover.properties not found, using default value: " + name + "=" + value); // NOSONAR for docker console
            }

            return value;
        }

        private void VerifyRemovedProperty(string name, string message)
        {
            string value = GetProperty(name, "");
            if (!"".Equals(value))
                throw new QaCoverException("Property " + name + " has been removed. " + message);
        }

        /// <summary>
        /// Reads a property that represents a list of comma separated values
        /// </summary>
        private IList<string> GetMultiValueProperty(string name)
        {
            IList<string> values = new List<string>();
            string str = GetProperty(name, "");
            if (!"".Equals(str))
            {
                string[] arr = JavaCs.SplitByChar(str, ',');
                foreach (string item in arr)
                    if (!"".Equals(item.Trim()))
                        values.Add(item.Trim());
            }

            return values;
        }

        /// <summary>
        /// Checks if a value matches any of the values of a list (case insensitive).
        /// If matchStartsWith=true checks only that the value is a prefix.
        /// </summary>
        public static bool ValueInProperty(string value, IList<string> searchIn, bool matchStartsWith)
        {
            foreach (string item in searchIn)
            {
                if (matchStartsWith && value.ToLower().StartsWith(item.ToLower()) || !matchStartsWith && JavaCs.EqualsIgnoreCase(value, item))
                    return true;
            }

            return false;
        }

        public virtual Configuration SetName(string name)
        {
            this.name = name;
            return this;
        }

        public virtual string GetName()
        {
            return this.name;
        }

        public virtual string GetStoreRulesLocation()
        {
            return storeRulesLocation;
        }

        public virtual void SetStoreRulesLocation(string location)
        {
            storeRulesLocation = location;
        }

        public virtual string GetStoreReportsLocation()
        {
            return storeReportsLocation;
        }

        public virtual void SetStoreReportsLocation(string location)
        {
            storeReportsLocation = location;
        }

        public virtual string GetRuleCriterion()
        {
            return ruleCriterion;
        }

        public virtual string GetRuleServiceUrl()
        {
            return ruleUrl;
        }

        public virtual string GetRuleOptions()
        {
            return ruleOptions;
        }

        public virtual Configuration SetRuleCriterion(string ruleCriterion)
        {
            this.ruleCriterion = ruleCriterion;
            return this;
        }

        public virtual Configuration SetRuleOptions(string ruleOptions)
        {
            this.ruleOptions = ruleOptions;
            return this;
        }

        public virtual Configuration SetRuleServiceUrl(string ruleUrl)
        {
            this.ruleUrl = ruleUrl;
            return this;
        }

        public virtual string GetRuleCacheFolder()
        {
            return ruleCache;
        }

        public virtual Configuration SetRuleCacheFolder(string folder)
        {
            ruleCache = folder;
            return this;
        }

        public virtual bool GetOptimizeRuleEvaluation()
        {
            return optimizeRuleEvaluation;
        }

        public virtual Configuration SetOptimizeRuleEvaluation(bool optimizeRuleEvaluation)
        {
            this.optimizeRuleEvaluation = optimizeRuleEvaluation;
            return this;
        }

        public virtual bool GetInferQueryParameters()
        {
            return inferQueryParameters;
        }

        public virtual Configuration SetInferQueryParameters(bool inferQueryParameters)
        {
            this.inferQueryParameters = inferQueryParameters;
            return this;
        }

        public virtual IList<string> GetStackExclusions()
        {
            return stackExclusions;
        }

        public virtual IList<string> GetStackInclusions()
        {
            return stackInclusions;
        }

        public virtual Configuration AddStackExclusion(string exclusion)
        {

            // for testing
            stackExclusions.Add(exclusion);
            return this;
        }

        public virtual IList<string> GetClassExclusions()
        {
            return classExclusions;
        }

        public virtual Configuration AddClassExclusion(string exclusion)
        {

            // for testing
            classExclusions.Add(exclusion);
            return this;
        }

        public virtual IList<string> GetTableExclusionsExact()
        {
            return tableExclusionsExact;
        }

        public virtual Configuration AddTableExclusionExact(string exclusion)
        {

            // for testing
            tableExclusionsExact.Add(exclusion);
            return this;
        }

        public virtual string GetDbStoretype()
        {
            return this.dbStoretype;
        }

        public virtual void SetDbStoretype(string storetype)
        {
            this.dbStoretype = storetype;
            log.Info("Configure db store type as: {}", storetype);
        }
    }
}