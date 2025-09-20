namespace backend.Models;
public class Depistage
{
    public int Id { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public int PatientId { get; set; }

    public Patient Patient { get; set; }
    public ICollection<Reponse> Reponses { get; set; }
    public ResultatIA ResultatIA { get; set; }
}
