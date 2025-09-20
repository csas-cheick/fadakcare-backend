namespace backend.Models;
public class Conseil
{
    public int Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime DateEnvoi { get; set; } = DateTime.Now;

    public int MedecinId { get; set; }
    public int PatientId { get; set; }

    public Medecin? Medecin { get; set; }
    public Patient? Patient { get; set; }
}
