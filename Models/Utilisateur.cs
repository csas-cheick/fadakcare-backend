using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class Utilisateur
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? Nom { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime DateNaissance { get; set; }

        [Required, Phone]
        public string? Telephone { get; set; }

        [Required, EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string? MotDePasse { get; set; }

        [Required]
        public string? Role { get; set; }

        [Required]
        public string? Adresse { get; set; }
        public string? GoogleId { get; set; }
        public bool? isOnline { get; set; } = false;
        public bool EstBloque { get; set; } = false;
        public string? PhotoUrl { get; set; }

    }
}
