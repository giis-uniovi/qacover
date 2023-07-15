using System;


namespace Giis.Qacover.Portable
{
	[System.Serializable]
	public class QaCoverException : Exception
	{
		public QaCoverException(Exception e)
			: base("Schema Exception", e)
		{
		}

		public QaCoverException(string message)
			: base(message)
		{
		}

		public QaCoverException(string message, Exception cause)
			: base(message + (cause == null ? string.Empty : ". Caused by: " + GetString(cause)), cause)
		{
		}

        public override string ToString()
        {
            return GetString(this);
        }
        /// <summary>En java toString devuelve el nombre de clase y mensaje, pero en .net incluye el stacktrace.</summary>
        /// <remarks>
        /// En java toString devuelve el nombre de clase y mensaje, pero en .net incluye el stacktrace.
        /// Utilizando este metodo en cada plataforma se genera un mensjae similar al de java
        /// </remarks>
        public static string GetString(Exception e)
		{
			string eString = e.GetType().ToString() + ": " + e.Message;
			if (e is Giis.Tdrules.Openapi.Client.ApiException exception)
				eString += " Code " + exception.ErrorCode;
			return eString;
		}
	}
}
