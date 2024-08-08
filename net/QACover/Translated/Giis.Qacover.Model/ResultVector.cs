/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using System.Text;


namespace Giis.Qacover.Model
{
	/// <summary>Represents the evaluation status of each rule of a query</summary>
	public class ResultVector
	{
		public const string NotEvaluated = "NOT_EVALUATED";

		public const string Covered = "COVERED";

		public const string Uncovered = "UNCOVERED";

		public const string AlreadyCovered = "ALREADY_COVERED";

		public const string RuntimeError = "RUNTIME_ERROR";

		private string[] vector;

		public ResultVector(int ruleCount)
		{
			vector = new string[ruleCount];
			for (int i = 0; i < vector.Length; i++)
			{
				vector[i] = NotEvaluated;
			}
		}

		public virtual void SetResult(int ruleNumber, string status)
		{
			vector[ruleNumber] = status;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			foreach (string item in vector)
			{
				if (NotEvaluated.Equals(item))
				{
					sb.Append(".");
				}
				else
				{
					if (Covered.Equals(item))
					{
						sb.Append("#");
					}
					else
					{
						if (Uncovered.Equals(item))
						{
							sb.Append("o");
						}
						else
						{
							if (AlreadyCovered.Equals(item))
							{
								sb.Append("+");
							}
							else
							{
								if (RuntimeError.Equals(item))
								{
									sb.Append("!");
								}
							}
						}
					}
				}
			}
			return sb.ToString();
		}
	}
}
