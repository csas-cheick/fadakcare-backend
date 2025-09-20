using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backend.Models
{
    public class Telemedecine
    {
        [Key]
        public int Id { get; set; }
        public int CreateurId { get; set; }

        [Required]
        public string? Titre { get; set; }
        public string? Description { get; set; }

        [Required]
        public DateTime DateDebut { get; set; }

        [Required]
        public int Duree { get; set; }

        [Required]
        public string? Type { get; set; }

        [Required]
        public string Etat { get; set; } = "programmé";

        public string? IdSalle { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [JsonIgnore]
        public virtual Utilisateur? Createur { get; set; }
        public virtual ICollection<ParticipantTelemedecine>? Participants { get; set; }
    }
}