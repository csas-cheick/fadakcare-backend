namespace backend.Dtos.Alerte
{
    public class AlerteDto
    {
        public int Id { get; set; }
        public string? Message { get; set; }
        public DateTime DateEnvoi { get; set; }
        public string? ExpediteurNom { get; set; }
        public string? DestinataireNom { get; set; }
        public string? ExpediteurRole { get; set; }
        public string? DestinataireRole { get; set; }
        public string? ExpediteurPhotoUrl { get; set; }
        public string? DestinatairePhotoUrl { get; set; }
    }
}