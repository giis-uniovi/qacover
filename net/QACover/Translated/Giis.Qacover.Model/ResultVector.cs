using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////

namespace Giis.Qacover.Model
{
    /// <summary>
    /// Represents the evaluation status of each rule of a query
    /// </summary>
    public class ResultVector
    {
        public static readonly string NOT_EVALUATED = "NOT_EVALUATED";
        public static readonly string COVERED = "COVERED";
        public static readonly string UNCOVERED = "UNCOVERED";
        public static readonly string ALREADY_COVERED = "ALREADY_COVERED";
        public static readonly string RUNTIME_ERROR = "RUNTIME_ERROR";
        private string[] vector;
        public ResultVector(int ruleCount)
        {
            vector = new string[ruleCount];
            for (int i = 0; i < vector.Length; i++)
                vector[i] = NOT_EVALUATED;
        }

        public virtual void SetResult(int ruleNumber, string status)
        {
            vector[ruleNumber] = status;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (string item in vector)
                if (NOT_EVALUATED.Equals(item))
                    sb.Append(".");
                else if (COVERED.Equals(item))
                    sb.Append("#");
                else if (UNCOVERED.Equals(item))
                    sb.Append("o");
                else if (ALREADY_COVERED.Equals(item))
                    sb.Append("+");
                else if (RUNTIME_ERROR.Equals(item))
                    sb.Append("!");
            return sb.ToString();
        }
    }
}