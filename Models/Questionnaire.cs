namespace backend.Models.Depist.Questionnaire
{
    public class Questionnaire
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;

        public ICollection<Question> Questions { get; set; } = new List<Question>();
    }
}