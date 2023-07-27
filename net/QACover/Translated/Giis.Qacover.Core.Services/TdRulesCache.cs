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
	/// TdRules, to be moved to tdrules-client.
	/// </summary>
	/// <remarks>
	/// Temporal implementation of a local storage cache for payloads sent to
	/// TdRules, to be moved to tdrules-client.
	/// A call to an api endpoint instantiates this class for a given request and a
	/// folder that will store the responses for each request. Then, checks if a
	/// response for this request already exists in the cache using 'hit':
	/// - If true, it will return the object 'getPayload' with the cached response
	/// (a cast may be necessary).
	/// - If false, call to the real endpoint and save the response to
	/// the cache by calling 'putPayload'
	/// </remarks>
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

		/// <summary>Determines if there is a cached response for the request indicated at the instantiation</summary>
		public virtual bool Hit()
		{
			return this.hit != null;
		}

		/// <summary>Gets the cached response of a given request stored in the cache ('hit' should be true)</summary>
		public virtual object GetPayload(Type clazz)
		{
			return serializer.Deserialize(hit, clazz);
		}

		/// <summary>Saves to the cache the response payload of a given request</summary>
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
