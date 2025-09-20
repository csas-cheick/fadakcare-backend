namespace backend.Dtos.Message;

public class UtilisateurMessageDto
{
    public int Id { get; set; }
    public string? Nom { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool? isOnline { get; set; }
    public string? PhotoUrl { get; set; }
}
