using Newtonsoft.Json;
using System;

namespace Giis.Tdrules.Model.IO
{
    // Temporal location, to be moved to tdrules-model
    public class ModelJsonSerializer
    {
        /**
         * Recommended serialization for models (exclude null and empty attributes)
         */
        public string Serialize(object model, bool prettyPrint)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            JsonSerializer serializer = new JsonSerializer();
            string result = JsonConvert.SerializeObject(model, prettyPrint ? Formatting.Indented : Formatting.None, settings);
            return result;
        }

        /**
         * Recommended deserialization for models
         */
        public object Deserialize(string json, Type clazz)
        {
            return JsonConvert.DeserializeObject(json, clazz);
        }
    }
}
