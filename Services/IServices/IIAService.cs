namespace backend.Services.IServices;
public interface IIAService
{
    Task<(int score, string analyse)> EvaluerDepistageAsync(List<(string Question, string Reponse)> questionsEtReponses);
}