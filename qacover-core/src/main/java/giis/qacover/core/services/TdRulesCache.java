package giis.qacover.core.services;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import giis.portable.util.FileUtil;
import giis.portable.util.JavaCs;
import giis.tdrules.model.io.ModelJsonSerializer;

/**
 * Temporal implementation of a local storage cache for payloads sent to
 * TdRules, to be moved to tdrules-client
 */
public class TdRulesCache {
	private static final Logger log = LoggerFactory.getLogger(TdRulesCache.class);

	private ModelJsonSerializer serializer;
	String endpoint;
	String payload;
	String hash;
	String cacheFile;
	String hit;

	public TdRulesCache(String cacheFolder, String endpoint, Object request) {
		serializer = new ModelJsonSerializer();
		this.endpoint = endpoint;
		this.payload = serializer.serialize(request, true);
		this.hash = JavaCs.getHash(payload);
		this.ensureCacheFolder(cacheFolder, endpoint);
		this.cacheFile = getCacheFile(cacheFolder, endpoint, hash);
		this.hit = FileUtil.fileRead(cacheFile, false);
		log.debug("Cache {} {} hit: {}", endpoint, hash, this.hit != null);
	}

	/**
	 * Gets the payload of a given request from cache, if does not exists returns
	 * null
	 */
	public boolean hit() {
		return this.hit != null;
	}

	@SuppressWarnings("rawtypes")
	public Object getPayload(Class clazz) {
		return serializer.deserialize(hit, clazz);
	}

	/**
	 * Saves the response payload of a given request to cache
	 */
	public void putPayload(Object result) {
		FileUtil.fileWrite(cacheFile, serializer.serialize(result, true));
		log.debug("Cache {} {} update: {}", endpoint, hash, result);
	}

	private void ensureCacheFolder(String cacheFolder, String endpoint) {
		FileUtil.createDirectory(FileUtil.getPath(cacheFolder, endpoint));
	}
	private String getCacheFile(String cacheFolder, String endpoint, String hash) {
		return FileUtil.getPath(cacheFolder, endpoint, hash + ".json");
	}

}
