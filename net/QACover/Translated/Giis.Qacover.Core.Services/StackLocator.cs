using NLog;
using Giis.Portable.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////

namespace Giis.Qacover.Core.Services
{
    /// <summary>
    /// Locates the position where a query call has been originated,
    /// taking into account the inclusions and exclusions indicated in the options
    /// </summary>
    public class StackLocator
    {
        private static readonly string UNDEFINED = "undefined";
        private static readonly Logger log = Giis.Portable.Util.NLogUtil.GetLogger(typeof(StackLocator));
        private CallStack stack;
        private string className = UNDEFINED;
        private string methodName = UNDEFINED;
        private string fileName = UNDEFINED;
        private string fullFileName = UNDEFINED;
        private int lineNumber = 0;
        public StackLocator()
        {
            stack = new CallStack();
            FindLocation();
        }

        private void FindLocation()
        {
            for (int i = 0; i < stack.Size(); i++)
            {
                log.Trace("StackTrace item: " + stack.GetClassName(i) + " " + stack.GetMethodName(i) + " " + stack.GetLineNumber(i) + " " + stack.GetFileName(i));
                if (!ExcludeClassName(stack.GetClassName(i)))
                {
                    this.className = stack.GetClassName(i);
                    this.methodName = stack.GetMethodName(i);
                    this.fullFileName = stack.GetFullFileName(i);
                    this.fileName = stack.GetFileName(i);
                    this.lineNumber = stack.GetLineNumber(i);
                    return;
                }
            }

            log.Warn("StackTrace is empty because of exclusions, full call stack displayed below: " + stack.GetString());
        }

        private bool ExcludeClassName(string stackClass)
        {
            if (Configuration.ValueInProperty(stackClass, Configuration.GetInstance().GetStackInclusions(), true))
                return false;
            if (Configuration.ValueInProperty(stackClass, Configuration.GetInstance().GetStackExclusions(), true))
                return true;
            return false;
        }

        public override string ToString()
        {
            return stack.GetString();
        }

        public virtual string GetLocationAsString()
        {
            return className + "." + methodName + ":" + lineNumber + " " + fileName;
        }

        public virtual string GetClassName()
        {
            return className;
        }

        public virtual string GetMethodName()
        {
            return methodName;
        }

        public virtual string GetFileName()
        {
            return fileName;
        }

        public virtual string GetSourceFileName()
        {
            return fullFileName;
        }

        public virtual int GetLineNumber()
        {
            return lineNumber;
        }
    }
}