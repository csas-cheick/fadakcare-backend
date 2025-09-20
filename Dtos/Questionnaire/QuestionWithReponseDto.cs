namespace backend.Dtos.Questionnaire
{
    public class QuestionWithReponseDto
    {
        public int Id { get; set; }
        public string? Text { get; set; }
        public string Type { get; set; } = "texte";
        public List<string>? Options { get; set; }
        public int QuestionnaireId { get; set; }
        public string? DerniereReponse { get; set; }
    }

    public class QuestionnaireWithReponsesDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public List<QuestionWithReponseDto> Questions { get; set; } = new();
    }
}
