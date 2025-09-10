using System.Text.Json.Serialization;

namespace WebApi_Normal.Dominio
{
    public class Incidente
    {
        [JsonPropertyName("timestamp")]
        public string Tiempo { get; set; }


        [JsonPropertyName("torre")]
        public string Torre { get; set; }


        [JsonPropertyName("code")]
        public string Codigo { get; set; }


        [JsonPropertyName("severity")]
        public string Severidad { get; set; }


        [JsonPropertyName("message")]
        public string Mensaje { get; set; }

    }
}
