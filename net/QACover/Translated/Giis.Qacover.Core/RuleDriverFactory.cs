using Giis.Qacover.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////

namespace Giis.Qacover.Core
{
    public class RuleDriverFactory
    {
        /// <summary>
        /// Instantiation of a rule driver according with the current configuration
        /// options. At this moment, only fpc
        /// </summary>
        public virtual RuleDriver GetDriver()
        {
            if ("mutation".Equals(Configuration.GetInstance().GetRuleCriterion()))
                return new RuleDriverMutation();
            else

                // fpc by default
                return new RuleDriverFpc();
        }
    }
}