using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////

namespace Giis.Qacover.Core.Services
{
    /// <summary>
    /// To support some test situations we need to inject faults in strategic
    /// points of the evaluation process.
    /// 
    /// Each QueryStatement has a static instance of this class. If not null
    /// will perform the injection of faults configured in this class.
    /// WARNING: This is for testing only and not concurrency safe.
    /// </summary>
    public class FaultInjector
    {
        private static readonly Logger log = Giis.Portable.Util.NLogUtil.GetLogger(typeof(FaultInjector));
        private string faultException = "";
        private string faultSchema = "";
        private string faultTables = "";
        private string faultInfer = "";
        private string faultRules = "";
        private string faultSingleRule = "";
        public FaultInjector()
        {
            Reset();
        }

        public virtual FaultInjector Reset()
        {
            faultSchema = "";
            return this;
        }

        public virtual FaultInjector SetUnexpectedException(string msg)
        {
            this.faultException = msg;
            return this;
        }

        public virtual bool IsUnexpectedException()
        {
            return !"".Equals(faultException);
        }

        public virtual string GetUnexpectedException()
        {
            log.Warn("Injecting unexpected exception, message: " + faultSchema);
            return faultException;
        }

        public virtual FaultInjector SetSchemaFaulty(string sql)
        {
            this.faultSchema = sql;
            return this;
        }

        public virtual bool IsSchemaFaulty()
        {
            return !"".Equals(faultSchema);
        }

        public virtual string GetSchemaFault()
        {
            log.Warn("Injecting fault at GetSchema, sql: " + faultSchema);
            return faultSchema;
        }

        public virtual FaultInjector SetTablesFaulty(string sql)
        {
            this.faultTables = sql;
            return this;
        }

        public virtual bool IsTablesFaulty()
        {
            return !"".Equals(faultTables);
        }

        public virtual string GetTablesFault()
        {
            log.Warn("Injecting fault at GetTables, message: " + faultTables);
            return faultTables;
        }

        public virtual FaultInjector SetInferFaulty(string msg)
        {
            this.faultInfer = msg;
            return this;
        }

        public virtual bool IsInferFaulty()
        {
            return !"".Equals(faultInfer);
        }

        public virtual string GetInferFault()
        {
            log.Warn("Injecting fault at GetInfer, message: " + faultInfer);
            return faultInfer;
        }

        public virtual FaultInjector SetRulesFaulty(string sql)
        {
            this.faultRules = sql;
            return this;
        }

        public virtual bool IsRulesFaulty()
        {
            return !"".Equals(faultRules);
        }

        public virtual string GetRulesFault()
        {
            log.Warn("Injecting fault at GetRules, message: " + faultRules);
            return faultRules;
        }

        public virtual FaultInjector SetSingleRuleFaulty(string sql)
        {
            this.faultSingleRule = sql;
            return this;
        }

        public virtual bool IsSingleRuleFaulty()
        {
            return !"".Equals(faultSingleRule);
        }

        public virtual string GetSingleRuleFault()
        {
            log.Warn("Injecting fault at a rule, message: " + faultSingleRule);
            return faultSingleRule;
        }
    }
}