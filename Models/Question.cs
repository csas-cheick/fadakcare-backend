namespace backend.Models.Depist.Questionnaire
{
    public class Question
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Type { get; set; } = "texte";
        public List<string>? Options { get; set; } // uniquement si type = "select"


        public int QuestionnaireId { get; set; }
        public Questionnaire? Questionnaire { get; set; }
    }
}
