namespace backend.Models;

public class Patient : Utilisateur
{
    public string? Profession { get; set; }

    public int? MedecinId { get; set; }

    public Medecin? Medecin { get; set; }
    public ICollection<Depistage>? Depistages { get; set; }

    public Patient()
    {
        Role = "patient";
    }
}