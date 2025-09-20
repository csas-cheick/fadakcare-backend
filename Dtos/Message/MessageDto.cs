namespace backend.Dtos.Message;
public class MessageDto
{
    public int ExpediteurId { get; set; }
    public int DestinataireId { get; set; }
    public string Contenu { get; set; } = string.Empty;
}
