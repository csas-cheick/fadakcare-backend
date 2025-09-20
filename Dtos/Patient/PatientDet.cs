namespace backend.Dtos.Patient
{
    public class PatientDet
    {
        public int Id { get; set; }
        public string? Nom { get; set; }
        public string? Email { get; set; }
        public string? DateNaissance { get; set; }
        public string? Telephone { get; set; }
        public string? Profession { get; set; }
        public List<ResultatPatient>? Resultats { get; set; }
    }
}