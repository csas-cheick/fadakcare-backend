namespace backend.Dtos
{
    public class ParticipantDto
    {
        public int Id { get; set; }
        public int utilisateur_id { get; set; }
        public string Nom { get; set; } = "";
        public string Role { get; set; } = "patient";
    }
}
