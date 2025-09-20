namespace backend.Dtos.Depistage
{
    public class ReponseDetailDto
    {
        public int QuestionId { get; set; }
        public string? QuestionText { get; set; }
        public string? Type { get; set; }
        public string? Valeur { get; set; }
    }

    public class DepistageReponsesDto
    {
        public int DepistageId { get; set; }
        public int PatientId { get; set; }
        public string DateDepistage { get; set; } = string.Empty;
        public int? ResultatId { get; set; }
        public int? Score { get; set; }
        public List<ReponseDetailDto> Reponses { get; set; } = new();
    }
}
