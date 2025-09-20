namespace backend.Dtos.Questionnaire
{
    public class QuestionnaireDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public List<QuestionDto>? Questions { get; set; }
    }

    public class QuestionDto
    {
        public int Id { get; set; }
        public string? Text { get; set; }
        public string Type { get; set; } = "texte";
        public List<string>? Options { get; set; } // uniquement si type = "select"
        public int QuestionnaireId { get; set; }
    }

}
