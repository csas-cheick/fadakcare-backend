using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class RefreshToken
{
    [Key]
    public int Id { get; set; }
    [Required]
    public int UserId { get; set; }
    [Required]
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool Revoked { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
