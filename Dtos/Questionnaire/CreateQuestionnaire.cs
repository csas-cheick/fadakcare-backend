namespace backend.Dtos.Questionnaire
{
    public class CreateQuestionnaireDto
    {
        public string Title { get; set; } = string.Empty;
        public List<CreateQuestionDto> Questions { get; set; } = new();
    }

    public class CreateQuestionDto
    {
        public string Text { get; set; } = string.Empty;
        public string Type { get; set; } = "texte";
        public List<string>? Options { get; set; } // uniquement si type = "select"

    }
}
