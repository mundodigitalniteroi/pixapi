using NegocioCielo;
using System.Net.Http;

namespace NegocioCielo
{
    /// <summary>
    /// Classe para Serializar e Deserializar JSON.
    /// </summary>
    public class SerializerJSON : ISerializerJSON
    {
        public string Serialize<T>(T value)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(value);
        }

        public T Deserialize<T>(HttpContent content)
        {
             return Deserialize<T>(content.ReadAsStringAsync().Result);
        }

        public T Deserialize<T>(string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }
    }
}