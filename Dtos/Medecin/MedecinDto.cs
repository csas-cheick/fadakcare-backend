namespace backend.Dtos.Medecin
{
    public class MedecinDto
   {
    public string Nom { get; set; } = string.Empty;
    public string Specialite { get; set; } = string.Empty;
    public string Telephone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Service { get; set; } = string.Empty;  
    public string? PhotoUrl { get; set; }
   }
}