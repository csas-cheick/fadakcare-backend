namespace backend.Dtos.compte;

public class AuthResponse
{
    public int UserId { get; set; }
    // Duplicate property for frontend expecting 'id'
    public int Id { get => UserId; set => UserId = value; }
    public string? Email { get; set; }
    public string? Role { get; set; }
    public string? AccessToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? RefreshToken { get; set; }
}
