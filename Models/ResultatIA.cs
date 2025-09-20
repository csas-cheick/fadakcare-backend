namespace backend.Models;
public class ResultatIA
{
    public int Id { get; set; }
    public int Score { get; set; }
    public string Analyse { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;

    public int DepistageId { get; set; }
    public Depistage Depistage { get; set; }
}
