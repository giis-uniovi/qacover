using Giis.Portable.Util;
using Giis.Qacover.Report;
using System;

namespace QACoverReport
{
    static class Program
    {
        static void Main(string[] args)
        {
            Parameters.SetProjectRoot(".");
            Console.Out.WriteLine("Generating reports from root path: " + FileUtil.GetFullPath(Parameters.GetProjectRoot()));
            ReportMain.Main(args);
        }
    }
}
