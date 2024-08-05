/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;


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
		private SortedDictionary<int, SourceCodeLine> lines = new SortedDictionary<int, SourceCodeLine>();

		public virtual SortedDictionary<int, SourceCodeLine> GetLines()
		{
			return lines;
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
	}
}
