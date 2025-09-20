namespace backend.Dtos.Patient
{
    public class ResultatPatient
    {
        public int Id { get; set; }
    public int DepistageId { get; set; }
        public int NumeroDepistage { get; set; }
        public string? DateDepistage { get; set; }
        public int Score { get; set; }
        public string? Analyse { get; set; }
    }
}