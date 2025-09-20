using backend.Dtos.Depistage;
using backend.Models;
using backend.Dtos.Questionnaire;

namespace backend.Services.IServices;

public interface IDepistageService
{
    Task SoumettreDepistageAsync(SoumissionDepistageDto dto);
    Task<Depistage?> GetDernierDepistageAsync(int patientId);
    Task<IEnumerable<QuestionnaireWithReponsesDto>> GetQuestionnairesAvecDernieresReponsesAsync(int patientId);
    Task<DepistageReponsesDto?> GetDepistageReponsesAsync(int depistageId);

}