namespace backend.Dtos.compte;

public class RefreshRequest
{
    public int UserId { get; set; }
    public string? RefreshToken { get; set; }
}
