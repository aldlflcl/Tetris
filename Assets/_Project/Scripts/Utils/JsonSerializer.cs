using Newtonsoft.Json.Linq;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace Tetris.Utils
{
    public class JsonSerializer
    {
        public string Serialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public string GetNestedProperty(string json, string parentPropertyName, string childPropertyName)
        {
            return JObject.Parse(json)[parentPropertyName]?[childPropertyName]?.ToString();
        }
    }
}