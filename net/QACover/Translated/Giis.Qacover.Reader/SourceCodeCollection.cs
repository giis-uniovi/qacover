using Java.Util;
using NLog;
using Giis.Portable.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////

namespace Giis.Qacover.Reader
{
    /// <summary>
    /// Collects all info about source code and coverage of queries executed at each
    /// line. Indexed by line number, each can contain source code or queries or both
    /// </summary>
    public class SourceCodeCollection
    {
        private static readonly Logger log = Giis.Portable.Util.NLogUtil.GetLogger(typeof(SourceCodeCollection));
        private Map<int, SourceCodeLine> lines = new TreeMap<int, SourceCodeLine>();
        public virtual Map<int, SourceCodeLine> GetLines()
        {
            return lines;
        }

        public virtual IList<int> GetLineNumbers()
        {
            IList<int> ret = new List<int>();
            foreach (int key in lines.KeySet())
                ret.Add(key);
            return ret;
        }

        // Gets the indicated Line object, creates a new if not exists
        private SourceCodeLine GetLine(int lineNumber)
        {
            if (lines.ContainsKey(lineNumber))
                return lines[lineNumber];
            SourceCodeLine newLine = new SourceCodeLine();
            lines.Put(lineNumber, newLine);
            return newLine;
        }

        /// <summary>
        /// Adds a all queries to this collection. Each will be inserted at the line that
        /// corresponds with the line where the query was run
        /// </summary>
        public virtual void AddQueries(QueryCollection queries)
        {
            for (int i = 0; i < queries.GetSize(); i++)
                AddQuery(queries.GetItem(i));
        }

        /// <summary>
        /// Adds a query to this collection. This will be inserted at the line that
        /// corresponds with the line where the query was run
        /// </summary>
        public virtual void AddQuery(QueryReader query)
        {
            int lineNumber = JavaCs.StringToInt(query.GetKey().GetClassLine());
            SourceCodeLine line = GetLine(lineNumber);
            line.GetQueries().Add(query);
        }

        /// <summary>
        /// Adds the source file that can be found from the comma separated
        /// list of source folders at the source path indicated by query
        /// </summary>
        public virtual void AddSources(QueryReader query, string sourceFolders, string projectFolder)
        {
            string[] locations = JavaCs.SplitByChar(sourceFolders, ',');
            foreach (string sourceFolder in locations)
            {
                string sourceFile = query.GetModel().GetSourceLocation();
                string resolvedFile = ResolveSourcePath(sourceFolder, projectFolder, sourceFile);
                if ("".Equals(resolvedFile))
                    continue;
                IList<string> allLines = FileUtil.FileReadLines(resolvedFile, false);
                if (!JavaCs.IsEmpty(allLines))
                {
                    AddSources(allLines);
                    return;
                }
                else
                {
                    log.Debug("Source file can't be read or is empty: " + resolvedFile);
                }
            }


            // if sources have not added, no file could found, inform the error
            log.Error("Source file can't be read from any of the source folders: " + sourceFolders);
        }

        private void AddSources(IList<string> sources)
        {
            for (int i = 0; i < sources.Count; i++)
            {
                SourceCodeLine line = GetLine(i + 1); // line numbers begin with 1
                line.SetSource(sources[i]);
            }
        }

        public virtual string ResolveSourcePath(string sourceFolder, string projectFolder, string sourceFile)
        {
            log.Debug("Using project folder - Try to resolve: Source: [" + sourceFolder + "] Project: [" + projectFolder + "] File: [" + sourceFile + "]");
            sourceFolder = JavaCs.IsEmpty(sourceFolder) ? "" : sourceFolder.Trim();
            projectFolder = JavaCs.IsEmpty(projectFolder) ? "" : projectFolder.Trim();
            sourceFile = JavaCs.IsEmpty(sourceFile) ? "" : sourceFile.Trim();

            // Source folder and source file are required, if not, the empty return means that source can't be fount
            if ("".Equals(sourceFolder) || "".Equals(sourceFile))
            {
                log.Error("Can not resolve source code location: Source folder or file are empty");
                return "";
            }


            // If projectFolder specified (for net generated coverage), it is expected a full path source File
            // that is converted to relative to projectFolder
            if (!"".Equals(projectFolder))
            {

                // unifies separators (linux/windows) and simplifies double separators that appear sometimes
                sourceFile = FileUtil.GetFullPath(sourceFile.Replace("\\", "/")).Replace("\\", "/").Replace("//", "/");
                string prefix = FileUtil.GetFullPath(projectFolder.Replace("\\", "/")).Replace("\\", "/").Replace("//", "/");
                if (!prefix.EndsWith("/"))
                    prefix = prefix + "/";
                if (sourceFile.StartsWith(prefix))
                {
                    sourceFile = JavaCs.Substring(sourceFile, prefix.Length, sourceFile.Length);
                    log.Info("Using project folder - Resolved absolute source file name to relative: " + sourceFile);
                }
                else
                {
                    log.Error("Using project folder - Not resolved: prefix [" + prefix + "] is not part of source file [" + sourceFile + "]");
                    return "";
                }
            }

            string resolvedFile = FileUtil.GetPath(sourceFolder, sourceFile);
            log.Debug("Resolved to: " + resolvedFile);
            return resolvedFile;
        }
    }
}