namespace backend.Dtos.compte
{
    public class UpdateProfileDto
    {
        public string? Nom { get; set; }
        public string? Adresse { get; set; }
        public string? Telephone { get; set; }
        public DateTime DateNaissance { get; set; }
        public string? Grade { get; set; }
        public string? Specialite { get; set; }
        public string? NumeroOrdre { get; set; }
        public string? Service { get; set; }
        public string? Profession { get; set; }
        public string? Email { get; set; }
    }
}