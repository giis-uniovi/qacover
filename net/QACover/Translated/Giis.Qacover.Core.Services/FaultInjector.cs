/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using Giis.Portable.Util;
using NLog;


namespace Giis.Qacover.Core.Services
{
	/// <summary>
	/// To support some test situations we need to inject faults in strategic
	/// points of the evaluation process.
	/// </summary>
	/// <remarks>
	/// To support some test situations we need to inject faults in strategic
	/// points of the evaluation process.
	/// Each QueryStatement has a static instance of this class. If not null
	/// will perform the injection of faults configured in this class.
	/// WARNING: This is for testing only and not concurrency safe.
	/// </remarks>
	public class FaultInjector
	{
		private static readonly Logger log = NLogUtil.GetLogger(typeof(Giis.Qacover.Core.Services.FaultInjector));

		private string faultException = string.Empty;

		private string faultSchema = string.Empty;

		private string faultTables = string.Empty;

		private string faultInfer = string.Empty;

		private string faultRules = string.Empty;

		private string faultSingleRule = string.Empty;

		public FaultInjector()
		{
			Reset();
		}

		public virtual Giis.Qacover.Core.Services.FaultInjector Reset()
		{
			faultSchema = string.Empty;
			return this;
		}

		public virtual Giis.Qacover.Core.Services.FaultInjector SetUnexpectedException(string msg)
		{
			this.faultException = msg;
			return this;
		}

		public virtual bool IsUnexpectedException()
		{
			return !string.Empty.Equals(faultException);
		}

		public virtual string GetUnexpectedException()
		{
			log.Warn("Injecting unexpected exception, message: " + faultSchema);
			return faultException;
		}

		public virtual Giis.Qacover.Core.Services.FaultInjector SetSchemaFaulty(string sql)
		{
			this.faultSchema = sql;
			return this;
		}

		public virtual bool IsSchemaFaulty()
		{
			return !string.Empty.Equals(faultSchema);
		}

		public virtual string GetSchemaFault()
		{
			log.Warn("Injecting fault at GetSchema, sql: " + faultSchema);
			return faultSchema;
		}

		public virtual Giis.Qacover.Core.Services.FaultInjector SetTablesFaulty(string sql)
		{
			this.faultTables = sql;
			return this;
		}

		public virtual bool IsTablesFaulty()
		{
			return !string.Empty.Equals(faultTables);
		}

		public virtual string GetTablesFault()
		{
			log.Warn("Injecting fault at GetTables, message: " + faultTables);
			return faultTables;
		}

		public virtual Giis.Qacover.Core.Services.FaultInjector SetInferFaulty(string msg)
		{
			this.faultInfer = msg;
			return this;
		}

		public virtual bool IsInferFaulty()
		{
			return !string.Empty.Equals(faultInfer);
		}

		public virtual string GetInferFault()
		{
			log.Warn("Injecting fault at GetInfer, message: " + faultInfer);
			return faultInfer;
		}

		public virtual Giis.Qacover.Core.Services.FaultInjector SetRulesFaulty(string sql)
		{
			this.faultRules = sql;
			return this;
		}

		public virtual bool IsRulesFaulty()
		{
			return !string.Empty.Equals(faultRules);
		}

		public virtual string GetRulesFault()
		{
			log.Warn("Injecting fault at GetRules, message: " + faultRules);
			return faultRules;
		}

		public virtual Giis.Qacover.Core.Services.FaultInjector SetSingleRuleFaulty(string sql)
		{
			this.faultSingleRule = sql;
			return this;
		}

		public virtual bool IsSingleRuleFaulty()
		{
			return !string.Empty.Equals(faultSingleRule);
		}

		public virtual string GetSingleRuleFault()
		{
			log.Warn("Injecting fault at a rule, message: " + faultSingleRule);
			return faultSingleRule;
		}
	}
}
