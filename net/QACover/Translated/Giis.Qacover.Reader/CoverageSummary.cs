using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////

namespace Giis.Qacover.Reader
{
    /// <summary>
    /// The contents of a summary of an evaluation of a query
    /// </summary>
    public class CoverageSummary
    {
        // Overall counters
        protected int count = 0; // total rules
        protected int dead = 0; // total covered
        protected int error = 0; // total with error
        // Query related
        private int qcount = 0; // number of queries
        private int qrun = 0; // number of times executed
        private int qerror = 0; // number of errors in queries
        public override string ToString()
        {
            return "qcount=" + qcount + ",qerror=" + qerror + ",count=" + count + ",dead=" + dead + ",error=" + error;
        }

        public virtual int GetCount()
        {
            return count;
        }

        public virtual int GetDead()
        {
            return dead;
        }

        public virtual int GetError()
        {
            return error;
        }

        public virtual void AddRuleCounters(int countValue, int deadValue, int errorValue)
        {
            this.count += countValue;
            this.dead += deadValue;
            this.error += errorValue;
        }

        public virtual int GetQcount()
        {
            return qcount;
        }

        public virtual int GetQrun()
        {
            return qrun;
        }

        public virtual int GetQerror()
        {
            return qerror;
        }

        public virtual void AddQueryCounters(int countValue, int runValue, int errorValue)
        {
            this.qcount += countValue;
            this.qrun += runValue;
            this.qerror += errorValue;
        }
    }
}