using backend.Models.Depist.Questionnaire;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Services.IServices;
using backend.Dtos.Questionnaire;

namespace backend.Services
{
    public class QuestionnaireService : IQuestionnaireService
    {
        private readonly AppDbContext _context;

        public QuestionnaireService(AppDbContext context)
        {
            _context = context;
        }

        // Création d'un questionnaire avec questions multi-types
        public async Task<QuestionnaireResult> CreateQuestionnaire(CreateQuestionnaireDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title) || dto.Questions.Count == 0)
            {
                return new QuestionnaireResult { Success = false, Message = "Titre ou questions manquants" };
            }

            var questionnaire = new Questionnaire
            {
                Title = dto.Title,
                Questions = dto.Questions.Select(q => new Question
                {
                    Text = q.Text,
                    Type = q.Type,
                    Options = q.Options != null ? q.Options : null
                }).ToList()
            };

            _context.Questionnaires.Add(questionnaire);
            await _context.SaveChangesAsync();

            return new QuestionnaireResult
            {
                Success = true,
                Message = "Questionnaire enregistré avec succès",
                QuestionnaireId = questionnaire.Id
            };
        }

        // Récupération de tous les questionnaires
        public async Task<IEnumerable<QuestionnaireDto>> GetQuestionnaires()
        {
            var questionnaires = await _context.Questionnaires
                .Include(q => q.Questions)
                .ToListAsync();

            return questionnaires.Select(q => new QuestionnaireDto
            {
                Id = q.Id,
                Title = q.Title,
                Questions = q.Questions.Select(qq => new QuestionDto
                {
                    Id = qq.Id,
                    Text = qq.Text,
                    Type = qq.Type,
                    Options = qq.Options,
                    QuestionnaireId = q.Id
                }).ToList()
            }).ToList();
        }

        // Suppression d'un questionnaire
        public async Task<QuestionnaireResult> DeleteQuestionnaire(int id)
        {
            var questionnaire = await _context.Questionnaires
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (questionnaire == null)
            {
                return new QuestionnaireResult { Success = false, Message = "Questionnaire introuvable" };
            }

            _context.Questions.RemoveRange(questionnaire.Questions);
            _context.Questionnaires.Remove(questionnaire);
            await _context.SaveChangesAsync();

            return new QuestionnaireResult { Success = true, Message = "Questionnaire supprimé avec succès" };
        }

        // Mise à jour d'un questionnaire existant
        public async Task<QuestionnaireResult> UpdateQuestionnaire(int id, CreateQuestionnaireDto dto)
        {
            var questionnaire = await _context.Questionnaires
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (questionnaire == null)
            {
                return new QuestionnaireResult { Success = false, Message = "Questionnaire non trouvé" };
            }

            questionnaire.Title = dto.Title;

            // Supprimer les anciennes questions
            _context.Questions.RemoveRange(questionnaire.Questions);

            // Ajouter les nouvelles questions avec options si nécessaire
            questionnaire.Questions = dto.Questions.Select(q => new Question
            {
                Text = q.Text,
                Type = q.Type,
                Options = q.Options != null ? q.Options : null
            }).ToList();

            await _context.SaveChangesAsync();

            return new QuestionnaireResult { Success = true, Message = "Questionnaire modifié avec succès" };
        }
    }
}
