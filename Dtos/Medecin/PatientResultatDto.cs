namespace backend.Dtos.Medecin 
{
    public class PatientResultatDto
   {
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public DateTime DateNaissance { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Adresse { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public int NombreDepistages { get; set; }
   } 
}