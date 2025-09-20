using System.ComponentModel.DataAnnotations;
namespace backend.Models;

public class Admin : Utilisateur
{
    [Required]
    public string? Grade { get; set; }

    public Admin()
    {
        Role = "admin";
    }
}