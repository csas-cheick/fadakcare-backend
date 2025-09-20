using System.Text.Json.Serialization;

namespace backend.Dtos.Depistage
{
    public class SoumissionDepistageDto
    {
        [JsonPropertyName("idPatient")]
        public int PatientId { get; set; }
        public List<ReponseDto>? Reponses { get; set; }
    }
}