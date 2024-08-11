/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using Giis.Portable.Util;
using Giis.Qacover.Model;
using Giis.Qacover.Portable;
using Java.Util;
using NLog;


namespace Giis.Qacover.Core.Services
{
	/// <summary>Global configuration options (singleton), read from properties file</summary>
	public class Configuration
	{
		public const string OptionsFileProperty = "qacover.properties";

		private const string QacoverName = "qacover";

		private static readonly Logger log = NLogUtil.GetLogger(typeof(Giis.Qacover.Core.Services.Configuration));

		private Properties properties;

		private static Giis.Qacover.Core.Services.Configuration instance;

		private string name = QacoverName;

		private string storeRulesLocation;

		private string cacheRulesLocation;

		private string storeReportsLocation;

		private string ruleServiceType;

		private string fpcServiceUrl;

		private string fpcServiceOptions;

		private bool optimizeRuleEvaluation;

		private bool inferQueryParameters;

		private IList<string> stackExclusions;

		private IList<string> stackInclusions;

		private IList<string> classExclusions;

		private IList<string> tableExclusionsExact;

		private string dbStoretype = null;

		private Configuration()
		{
			// NOSONAR singleton allowed
			// do not generate for classes with this name (exact match)
			// do not generate for queries using any of this tables (exact match)
			// null means not configured, once configured will not be changed
			Reset();
		}

		public static Giis.Qacover.Core.Services.Configuration GetInstance()
		{
			if (instance == null)
			{
				instance = new Giis.Qacover.Core.Services.Configuration();
			}
			return instance;
		}

		public virtual Giis.Qacover.Core.Services.Configuration Reset()
		{
			this.properties = GetProperties(OptionsFileProperty);
			string storeRootLocation = GetProperty("qacover.store.root", Parameters.GetProjectRoot());
			string defaultRulesSubDir = FileUtil.GetPath(Parameters.GetReportSubdir(), QacoverName, "rules");
			string defaultReportsSubDir = FileUtil.GetPath(Parameters.GetReportSubdir(), QacoverName, "reports");
			storeRulesLocation = FileUtil.GetPath(storeRootLocation, GetProperty("qacover.store.rules", defaultRulesSubDir));
			storeReportsLocation = FileUtil.GetPath(storeRootLocation, GetProperty("qacover.store.reports", defaultReportsSubDir));
			ruleServiceType = "fpc";
			fpcServiceUrl = GetProperty("qacover.fpc.url", "https://in2test.lsi.uniovi.es/tdrules/api/v4");
			fpcServiceOptions = GetProperty("qacover.fpc.options", string.Empty);
			optimizeRuleEvaluation = JavaCs.EqualsIgnoreCase("true", GetProperty("qacover.optimize.rule.evaluation", "false"));
			inferQueryParameters = JavaCs.EqualsIgnoreCase("true", GetProperty("qacover.query.infer.parameters", "false"));
			// Includes some predefined stack exclusions
			stackExclusions = new List<string>();
			stackExclusions.Add("giis.portable.");
			stackExclusions.Add("giis.qacover.");
			stackExclusions.Add("com.p6spy.");
			stackExclusions.Add("java.");
			stackExclusions.Add("javax.");
			stackExclusions.Add("jdk.internal.");
			// for jdk11 (not needed in jdk8)
			stackExclusions.Add("System.");
			// for .net
			foreach (string item in GetMultiValueProperty("qacover.stack.exclusions"))
			{
				stackExclusions.Add(item);
			}
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

		/// <summary>Reads a property with a default value.</summary>
		/// <remarks>
		/// Reads a property with a default value.
		/// On net, makes a first read from an environment variable
		/// </remarks>
		private string GetProperty(string name, string defaultValue)
		{
			string value = null;
			// On .net docker environment variables replace configuration files
			if (new Variability().IsNetCore())
			{
				value = JavaCs.GetEnvironmentVariable(name);
				if (value != null)
				{
					log.Debug("Get parameter from environment: " + name + "=" + value);
					// NOSONAR for docker console
					return value;
				}
			}
			// Avoid exception when running in docker (no properties files)
			try
			{
				value = properties.GetProperty(name, defaultValue);
				log.Debug("Get parameter from qacover.properties: " + name + "=" + value);
			}
			catch (Exception)
			{
				// NOSONAR for docker console
				value = defaultValue;
				log.Debug("qacover.properties not found, using default value: " + name + "=" + value);
			}
			// NOSONAR for docker console
			return value;
		}

		/// <summary>Reads a property that represents a list of comma separated values</summary>
		private IList<string> GetMultiValueProperty(string name)
		{
			IList<string> values = new List<string>();
			string str = GetProperty(name, string.Empty);
			if (!string.Empty.Equals(str))
			{
				string[] arr = JavaCs.SplitByChar(str, ',');
				foreach (string item in arr)
				{
					if (!string.Empty.Equals(item.Trim()))
					{
						values.Add(item.Trim());
					}
				}
			}
			return values;
		}

		/// <summary>Checks if a value matches any of the values of a list (case insensitive).</summary>
		/// <remarks>
		/// Checks if a value matches any of the values of a list (case insensitive).
		/// If matchStartsWith=true checks only that the value is a prefix.
		/// </remarks>
		public static bool ValueInProperty(string value, IList<string> searchIn, bool matchStartsWith)
		{
			foreach (string item in searchIn)
			{
				if (matchStartsWith && value.ToLowerInvariant().StartsWith(item.ToLowerInvariant()) || !matchStartsWith && JavaCs.EqualsIgnoreCase(value, item))
				{
					return true;
				}
			}
			return false;
		}

		public virtual Giis.Qacover.Core.Services.Configuration SetName(string name)
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

		public virtual string GetCacheRulesLocation()
		{
			return cacheRulesLocation;
		}

		public virtual Giis.Qacover.Core.Services.Configuration SetCacheRulesLocation(string location)
		{
			// Ensures that location is relative to project root and not null
			cacheRulesLocation = location != null && !string.Empty.Equals(location) ? FileUtil.GetPath(Parameters.GetProjectRoot(), location) : string.Empty;
			return this;
		}

		public virtual string GetStoreReportsLocation()
		{
			return storeReportsLocation;
		}

		public virtual void SetStoreReportsLocation(string location)
		{
			storeReportsLocation = location;
		}

		public virtual string GetRuleServiceType()
		{
			return ruleServiceType;
		}

		public virtual string GetFpcServiceUrl()
		{
			return fpcServiceUrl;
		}

		public virtual string GetFpcServiceOptions()
		{
			return fpcServiceOptions;
		}

		public virtual Giis.Qacover.Core.Services.Configuration SetRuleServiceType(string ruleServiceType)
		{
			this.ruleServiceType = ruleServiceType;
			return this;
		}

		public virtual Giis.Qacover.Core.Services.Configuration SetFpcServiceOptions(string fpcServiceOptions)
		{
			this.fpcServiceOptions = fpcServiceOptions;
			return this;
		}

		public virtual Giis.Qacover.Core.Services.Configuration SetFpcServiceUrl(string fpcServiceUrl)
		{
			this.fpcServiceUrl = fpcServiceUrl;
			return this;
		}

		public virtual bool GetOptimizeRuleEvaluation()
		{
			return optimizeRuleEvaluation;
		}

		public virtual Giis.Qacover.Core.Services.Configuration SetOptimizeRuleEvaluation(bool optimizeRuleEvaluation)
		{
			this.optimizeRuleEvaluation = optimizeRuleEvaluation;
			return this;
		}

		public virtual bool GetInferQueryParameters()
		{
			return inferQueryParameters;
		}

		public virtual Giis.Qacover.Core.Services.Configuration SetInferQueryParameters(bool inferQueryParameters)
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

		public virtual Giis.Qacover.Core.Services.Configuration AddStackExclusion(string exclusion)
		{
			// for testing
			stackExclusions.Add(exclusion);
			return this;
		}

		public virtual IList<string> GetClassExclusions()
		{
			return classExclusions;
		}

		public virtual Giis.Qacover.Core.Services.Configuration AddClassExclusion(string exclusion)
		{
			// for testing
			classExclusions.Add(exclusion);
			return this;
		}

		public virtual IList<string> GetTableExclusionsExact()
		{
			return tableExclusionsExact;
		}

		public virtual Giis.Qacover.Core.Services.Configuration AddTableExclusionExact(string exclusion)
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
