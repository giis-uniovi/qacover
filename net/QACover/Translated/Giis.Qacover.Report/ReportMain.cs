using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////

namespace Giis.Qacover.Report
{
    /// <summary>
    /// When packaged in an executable jar, this is the entry point to generate the
    /// report from the commandline with two args: rules folder and reports folder
    /// </summary>
    public class ReportMain
    {
        public static void Main(string[] args)
        {
            if (args.Length == 2)
            {
                new ReportManager().Run(args[0], args[1]);
            }
            else if (args.Length == 3)
            {
                new ReportManager().Run(args[0], args[1], args[2], "");
            }
            else if (args.Length == 4)
            {
                new ReportManager().Run(args[0], args[1], args[2], args[3]);
            }
            else
            {
                ReportManager.ConsoleWrite("At least two parameters are required: rulesFolder reportFolder [sourceFolders [projecFolder]]");
            }
        }
    }
}