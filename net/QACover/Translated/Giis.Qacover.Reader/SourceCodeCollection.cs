/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using Giis.Portable.Util;
using NLog;


namespace Giis.Qacover.Reader
{
	/// <summary>
	/// Collects all info about source code and coverage of queries executed at each
	/// line.
	/// </summary>
	/// <remarks>
	/// Collects all info about source code and coverage of queries executed at each
	/// line. Indexed by line number, each can contain source code or queries or both
	/// </remarks>
	public class SourceCodeCollection
	{
		private static readonly Logger log = NLogUtil.GetLogger(typeof(SourceCodeCollection));

		private SortedDictionary<int, SourceCodeLine> lines = new SortedDictionary<int, SourceCodeLine>();

		public virtual SortedDictionary<int, SourceCodeLine> GetLines()
		{
			return lines;
		}

		public virtual IList<int> GetLineNumbers()
		{
			IList<int> ret = new List<int>();
			foreach (int key in lines.Keys)
			{
				ret.Add(key);
			}
			return ret;
		}

		// Gets the indicated Line object, creates a new if not exists
		private SourceCodeLine GetLine(int lineNumber)
		{
			if (lines.ContainsKey(lineNumber))
			{
				return lines[lineNumber];
			}
			SourceCodeLine newLine = new SourceCodeLine();
			lines[lineNumber] = newLine;
			return newLine;
		}

		/// <summary>Adds a all queries to this collection.</summary>
		/// <remarks>
		/// Adds a all queries to this collection. Each will be inserted at the line that
		/// corresponds with the line where the query was run
		/// </remarks>
		public virtual void AddQueries(QueryCollection queries)
		{
			for (int i = 0; i < queries.Size(); i++)
			{
				AddQuery(queries.Get(i));
			}
		}

		/// <summary>Adds a query to this collection.</summary>
		/// <remarks>
		/// Adds a query to this collection. This will be inserted at the line that
		/// corresponds with the line where the query was run
		/// </remarks>
		public virtual void AddQuery(QueryReader query)
		{
			int lineNumber = System.Convert.ToInt32(query.GetKey().GetClassLine());
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
				if (string.Empty.Equals(resolvedFile))
				{
					// skip: empty return means that path can't be resolved
					continue;
				}
				IList<string> allLines = FileUtil.FileReadLines(resolvedFile, false);
				if (!JavaCs.IsEmpty(allLines))
				{
					AddSources(allLines);
					return;
				}
				else
				{
					log.Debug("File can't be read or is empty: " + resolvedFile);
				}
			}
		}

		private void AddSources(IList<string> sources)
		{
			for (int i = 0; i < sources.Count; i++)
			{
				SourceCodeLine line = GetLine(i + 1);
				// line numbers begin with 1
				line.SetSource(sources[i]);
			}
		}

		public virtual string ResolveSourcePath(string sourceFolder, string projectFolder, string sourceFile)
		{
			log.Debug("Try to resolve: Source: [" + sourceFolder + "] Project: [" + projectFolder + "] File: [" + sourceFile + "]");
			sourceFolder = JavaCs.IsEmpty(sourceFolder) ? string.Empty : sourceFolder.Trim();
			projectFolder = JavaCs.IsEmpty(projectFolder) ? string.Empty : projectFolder.Trim();
			sourceFile = JavaCs.IsEmpty(sourceFile) ? string.Empty : sourceFile.Trim();
			// Source folder and source file are required, if not, the empty return means that source can't be fount
			if (string.Empty.Equals(sourceFolder) || string.Empty.Equals(sourceFile))
			{
				log.Debug("Not resolved, source folder or file are empty");
				return string.Empty;
			}
			// If projectFolder specified (for net generated coverage), it is expected a full path source File
			// that is converted to relative to projectFolder
			if (!string.Empty.Equals(projectFolder))
			{
				// unifies separators (linux/windows) and simplifies double separators that appear sometimes
				sourceFile = FileUtil.GetFullPath(sourceFile.Replace("\\", "/")).Replace("\\", "/").Replace("//", "/");
				string prefix = FileUtil.GetFullPath(projectFolder.Replace("\\", "/")).Replace("\\", "/").Replace("//", "/");
				if (!prefix.EndsWith("/"))
				{
					prefix = prefix + "/";
				}
				if (sourceFile.StartsWith(prefix))
				{
					sourceFile = JavaCs.Substring(sourceFile, prefix.Length, sourceFile.Length);
					log.Debug("Using project folder, resolved absolute file name to relative: " + sourceFile);
				}
				else
				{
					log.Debug("Using project folder, not resolved: prefix [" + prefix + "] is not part of source file [" + sourceFile + "]");
					return string.Empty;
				}
			}
			string resolvedFile = FileUtil.GetPath(sourceFolder, sourceFile);
			log.Debug("Resolved to: " + resolvedFile);
			return resolvedFile;
		}
	}
}
