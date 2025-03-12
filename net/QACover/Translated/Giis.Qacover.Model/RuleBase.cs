using Giis.Portable.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////

namespace Giis.Qacover.Model
{
    /// <summary>
    /// Base class to store the standard results/indicators of the evaluation of
    /// (1) individual rules and (2) aggregated information of the evaluation of queries
    /// </summary>
    public abstract class RuleBase
    {
        protected static readonly string RDEAD = "dead"; // Covered
        protected static readonly string RCOUNT = "count";
        protected static readonly string RERROR = "error";
        public static readonly string TAG_ERROR = "error";
        // The indicator are stored as attributes, the implementatin
        // is specific to the subclass (query or rule)
        protected abstract string GetAttribute(string name);
        protected abstract void SetAttribute(string name, string value);
        public override string ToString()
        {
            return "count=" + GetCount() + ",dead=" + GetDead() + (this.GetError() > 0 ? ",error=" + GetError() : "");
        }

        public virtual void Reset()
        {
            SetCount(0);
            SetDead(0);
            SetError(0);
        }

        protected virtual int GetIntAttribute(string name)
        {
            string current = GetAttribute(name);
            if (current == null || "".Equals(current))
                return 0;
            else
                return JavaCs.StringToInt(current);
        }

        protected virtual void IncrementIntAttribute(string name, int value)
        {
            string current = GetAttribute(name);
            if (current == null || "".Equals(current))
                SetAttribute(name, JavaCs.NumToString(value));
            else

                // existe, incrementa
                SetAttribute(name, JavaCs.NumToString(value + JavaCs.StringToInt(GetAttribute(name))));
        }

        public virtual int GetCount()
        {
            return GetIntAttribute(RCOUNT);
        }

        public virtual void SetCount(int count)
        {
            SetAttribute(RCOUNT, JavaCs.NumToString(count));
        }

        public virtual void AddCount(int value)
        {
            IncrementIntAttribute(RCOUNT, value);
        }

        public virtual int GetDead()
        {
            return GetIntAttribute(RDEAD);
        }

        public virtual void SetDead(int dead)
        {
            SetAttribute(RDEAD, JavaCs.NumToString(dead));
        }

        public virtual void AddDead(int value)
        {
            IncrementIntAttribute(RDEAD, value);
        }

        public virtual int GetError()
        {
            return GetIntAttribute(RERROR);
        }

        public virtual void SetError(int error)
        {
            if (GetError() > 0)
                SetAttribute(RERROR, JavaCs.NumToString(error));
        }

        public virtual void AddError(int value)
        {
            IncrementIntAttribute(RERROR, value);
        }
    }
}