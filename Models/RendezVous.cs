namespace backend.Models
{
    public class RendezVous
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int MedecinId { get; set; }
        public DateTime Date { get; set; }
        public string? Motif { get; set; }
        public string Etat { get; set; } = "en_attente";
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Utilisateur? Patient { get; set; }
        public Utilisateur? Medecin { get; set; }
    }

}