using backend.Dtos.Questionnaire;

namespace backend.Services.IServices;

public interface IQuestionnaireService
{
    Task<QuestionnaireResult> CreateQuestionnaire(CreateQuestionnaireDto dto);
    Task<IEnumerable<QuestionnaireDto>> GetQuestionnaires();
    Task<QuestionnaireResult> DeleteQuestionnaire(int id);
    Task<QuestionnaireResult> UpdateQuestionnaire(int id, CreateQuestionnaireDto dto);
}