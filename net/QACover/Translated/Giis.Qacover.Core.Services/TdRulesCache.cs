/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Giis.Portable.Util;
using Giis.Tdrules.Model.IO;
using NLog;


namespace Giis.Qacover.Core.Services
{
	/// <summary>
	/// Temporal implementation of a local storage cache for payloads sent to
	/// TdRules, to be moved to tdrules-client
	/// </summary>
	public class TdRulesCache
	{
		private static readonly Logger log = NLogUtil.GetLogger(typeof(Giis.Qacover.Core.Services.TdRulesCache));

		private ModelJsonSerializer serializer;

		internal string endpoint;

		internal string payload;

		internal string hash;

		internal string cacheFile;

		internal string hit;

		public TdRulesCache(string cacheFolder, string endpoint, object request)
		{
			serializer = new ModelJsonSerializer();
			this.endpoint = endpoint;
			this.payload = serializer.Serialize(request, true);
			this.hash = JavaCs.GetHash(payload);
			this.EnsureCacheFolder(cacheFolder, endpoint);
			this.cacheFile = GetCacheFile(cacheFolder, endpoint, hash);
			this.hit = FileUtil.FileRead(cacheFile, false);
			log.Debug("Cache {} {} hit: {}", endpoint, hash, this.hit != null);
		}

		/// <summary>
		/// Gets the payload of a given request from cache, if does not exists returns
		/// null
		/// </summary>
		public virtual bool Hit()
		{
			return this.hit != null;
		}

		public virtual object GetPayload(Type clazz)
		{
			return serializer.Deserialize(hit, clazz);
		}

		/// <summary>Saves the response payload of a given request to cache</summary>
		public virtual void PutPayload(object result)
		{
			FileUtil.FileWrite(cacheFile, serializer.Serialize(result, true));
			log.Debug("Cache {} {} update: {}", endpoint, hash, result);
		}

		private void EnsureCacheFolder(string cacheFolder, string endpoint)
		{
			FileUtil.CreateDirectory(FileUtil.GetPath(cacheFolder, endpoint));
		}

		private string GetCacheFile(string cacheFolder, string endpoint, string hash)
		{
			return FileUtil.GetPath(cacheFolder, endpoint, hash + ".json");
		}
	}
}
