using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.Telemedicine
{
    public class CreateTelemedicineDto
    {
        [Required]
        public string? Titre { get; set; }
        
        public string? Description { get; set; }
        
        [Required]
        public DateTime DateDebut { get; set; }
        
        [Required]
        public int Duree { get; set; } // En minutes
        
        [Required]
        public string? Type { get; set; } // "medecin_patient", "medecin_patients", "medecin_medecin"
        
        public List<int>? ParticipantsIds { get; set; }
    }

    public class UpdateTelemedicineDto
    {
        public string? Titre { get; set; }
        public string? Description { get; set; }
        public DateTime? DateDebut { get; set; }
        public int? Duree { get; set; }
        public string? Etat { get; set; } // "programmé", "en_cours", "terminé", "annulé"
        public List<int>? ParticipantsIds { get; set; }
    }

    public class TelemedicineResponseDto
    {
        public int Id { get; set; }
        public int CreateurId { get; set; }
        public string? CreateurNom { get; set; }
        public string? Titre { get; set; }
        public string? Description { get; set; }
        public DateTime DateDebut { get; set; }
        public int Duree { get; set; }
        public string? Type { get; set; }
        public string? Etat { get; set; }
        public string? IdSalle { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ParticipantResponseDto>? Participants { get; set; }
    }

    public class ParticipantResponseDto
    {
        public int Id { get; set; }
        public int UtilisateurId { get; set; }
        public string? UtilisateurNom { get; set; }
        public string? UtilisateurRole { get; set; }
        public string? Role { get; set; }
        public string? Etat { get; set; }
        public DateTime? HeureArrivee { get; set; }
        public DateTime? HeureDepart { get; set; }
    }

    public class JoinSessionDto
    {
        [Required]
        public int TelemedicineId { get; set; }
    }

    public class LeaveSessionDto
    {
        [Required]
        public int TelemedicineId { get; set; }
    }

    public class AddParticipantDto
    {
        [Required]
        public int TelemedicineId { get; set; }
        
        [Required]
        public int UtilisateurId { get; set; }
        
        public string? Role { get; set; } = "participant";
    }
}
