using backend.Models.Depist.Questionnaire;
namespace backend.Models
{
    public class Reponse
    {
        public int Id { get; set; }
        public string Valeur { get; set; }

        public int QuestionId { get; set; }
        public Question Question { get; set; }

        public int DepistageId { get; set; }
        public Depistage Depistage { get; set; }
    }
}

