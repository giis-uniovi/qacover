/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Giis.Portable.Util;
using Giis.Qacover.Model;
using Giis.Qacover.Storage;
using NLog;


namespace Giis.Qacover.Reader
{
	/// <summary>Represents all information to access to a query and its coverage rules given a QueryKey.</summary>
	/// <remarks>
	/// Represents all information to access to a query and its coverage rules given a QueryKey.
	/// It is instantiated from the store folder and gets the rest of info by demand.
	/// Used to create and manage collections of queries with a lazy access the details.
	/// Additionally it can store the timestamp and parameters sent to the query
	/// (this allows to use this object for reading the current state or an execution)
	/// </remarks>
	public class QueryReader
	{
		private static readonly Logger log = NLogUtil.GetLogger(typeof(Giis.Qacover.Reader.QueryReader));

		private string folder;

		private QueryKey queryKey;

		private string timestamp = string.Empty;

		private string @params = string.Empty;

		public QueryReader(string rulesFolder, string stringKey)
		{
			// store folder with the rules
			// Next data is optional
			// fecha formato iso
			// parametros en xml
			queryKey = new QueryKey(stringKey);
			folder = rulesFolder;
		}

		public virtual void SetTimestamp(string value)
		{
			timestamp = value;
		}

		public virtual void SetParams(string value)
		{
			@params = value;
		}

		public virtual QueryKey GetKey()
		{
			return queryKey;
		}

		/// <summary>Reads the QueryModel from the store</summary>
		public virtual QueryModel GetModel()
		{
			return new LocalStore(folder).GetQueryModel(queryKey.ToString());
		}

		/// <summary>Reads the QueryModel from the stored rules</summary>
		public virtual string GetSql()
		{
			return this.GetModel().GetSql();
		}

		public virtual string GetTimestamp()
		{
			return timestamp;
		}

		public virtual SchemaModel GetSchema()
		{
			try
			{
				return new LocalStore(folder).GetSchema(queryKey.ToString());
			}
			catch (Exception)
			{
				// A QueryModel may have no schema (if the query execution failed)
				log.Warn("Schema not found for rule " + queryKey.ToString());
				return new SchemaModel();
			}
		}

		public virtual string GetParametersXml()
		{
			return @params;
		}
	}
}
