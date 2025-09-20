using System.ComponentModel.DataAnnotations;
namespace backend.Models;

public class Medecin : Utilisateur
{
    [Required]
    public string? Specialite { get; set; }

    [Required]
    public string? NumeroOrdre { get; set; }

    public string? Service { get; set; }

    public ICollection<Patient>? Patients { get; set; }

    public Medecin()
    {
        Role = "doctor";
    }
}