using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backend.Models
{
    public class ParticipantTelemedecine
    {
        [Key]
        public int Id { get; set; }
        public int UtilisateurId { get; set; }
        public int TelemedicineId { get; set; }
        public string? Role { get; set; }
        public string Etat { get; set; } = "en_attente";
        public DateTime? HeureArrivee { get; set; }
        public DateTime? HeureDepart { get; set; }

        [JsonIgnore]
        public virtual Utilisateur? Utilisateur { get; set; }

        [JsonIgnore]
        public virtual Telemedecine? Telemedecine { get; set; }
    }
}