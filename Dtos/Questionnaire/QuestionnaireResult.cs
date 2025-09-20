namespace backend.Dtos.Questionnaire
{
    public class QuestionnaireResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int? QuestionnaireId { get; set; }
    }
}