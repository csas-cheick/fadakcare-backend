namespace backend.Dtos.compte
{
    public class GoogleLoginRequest
    {
        public string GoogleId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Nom { get; set; } = string.Empty;
        public string? Prenom { get; set; }
        public string? NomFamille { get; set; }
        public string? ProfilePicture { get; set; }
    }
}
