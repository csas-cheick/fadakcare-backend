namespace backend.Models;

public class Message
{
    public int Id { get; set; }
    public int ExpediteurId { get; set; }
    public int DestinataireId { get; set; }
    public string Contenu { get; set; } = string.Empty;
    public DateTime DateEnvoi { get; set; } = DateTime.Now;

    public Utilisateur? Expediteur { get; set; }
    public Utilisateur? Destinataire { get; set; }
}
