using backend.Data;
using backend.Dtos.Depistage;
using backend.Models;
using backend.Services.IServices;
using Microsoft.EntityFrameworkCore;
using backend.Dtos.Questionnaire;

namespace backend.Services;


public class DepistageService : IDepistageService
{
    private readonly AppDbContext _context;
    private readonly IIAService _iaService;
    private readonly IAlerteService _alerteService;

    public DepistageService(AppDbContext context, IIAService iaService, IAlerteService alerteService)
    {
        _context = context;
        _iaService = iaService;
        _alerteService = alerteService;
    }

    public async Task SoumettreDepistageAsync(SoumissionDepistageDto dto)
    {
        var depistage = new Depistage
        {
            Date = DateTime.Now,
            PatientId = dto.PatientId
        };

        _context.Depistage.Add(depistage);
        await _context.SaveChangesAsync();

        var reponses = (dto.Reponses ?? new List<ReponseDto>()).Select(r => new Reponse
        {
            Valeur = r.Valeur ?? string.Empty,
            QuestionId = r.QuestionId,
            DepistageId = depistage.Id
        }).ToList();

        _context.Reponse.AddRange(reponses);
        await _context.SaveChangesAsync();

        var questionsEtReponses = new List<(string Question, string Reponse)>();

        foreach (var r in dto.Reponses ?? new List<ReponseDto>())
        {
            var question = await _context.Questions
                .Where(q => q.Id == r.QuestionId)
                .Select(q => q.Text)
                .FirstOrDefaultAsync();

            if (question != null)
            {
                questionsEtReponses.Add((question, r.Valeur ?? string.Empty));
            }
        }

        var (scoreFinal, analyse) = await _iaService.EvaluerDepistageAsync(questionsEtReponses);
        Console.WriteLine("Resultat : ", scoreFinal, analyse);

        var resultat = new ResultatIA
        {
            Score = scoreFinal,
            Analyse = analyse,
            Date = DateTime.Now,
            DepistageId = depistage.Id
        };

        _context.ResultatIA.Add(resultat);
        await _context.SaveChangesAsync();

        // --- Génération automatique d'alerte IA si score élevé ---
        int seuil = 6;
        if (scoreFinal > seuil)
        {
            var patient = await _context.Patients.Include(p => p.Medecin).FirstOrDefaultAsync(p => p.Id == dto.PatientId);
            string messageAlerte = $"Alerte IA : Score de dépistage élevé ({scoreFinal}/10). {analyse}";
            DateTime now = DateTime.Now;
            // Seulement du patient vers le médecin
            if (patient != null && patient.MedecinId.HasValue)
            {
                var alerteMedecin = new Alerte
                {
                    Message = messageAlerte,
                    DateEnvoi = now,
                    ExpediteurId = patient.Id,
                    ExpediteurRole = "patient",
                    DestinataireId = patient.MedecinId.Value,
                    DestinataireRole = "doctor"
                };
                await _alerteService.EnvoyerAlerte(alerteMedecin);
            }
        }
    }
    public async Task<Depistage?> GetDernierDepistageAsync(int patientId)
    {
        return await _context.Depistage
            .Where(d => d.PatientId == patientId)
            .OrderByDescending(d => d.Date)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<QuestionnaireWithReponsesDto>> GetQuestionnairesAvecDernieresReponsesAsync(int patientId)
    {
        // Récupérer le dernier dépistage avec ses réponses
        var dernierDepistage = await _context.Depistage
            .Where(d => d.PatientId == patientId)
            .OrderByDescending(d => d.Date)
            .Include(d => d.Reponses)
            .FirstOrDefaultAsync();

        var dictReponses = dernierDepistage?.Reponses
            .GroupBy(r => r.QuestionId)
            .Select(g => g.OrderByDescending(r => r.Id).First()) // sécurité si doublons
            .ToDictionary(r => r.QuestionId, r => r.Valeur) ?? new Dictionary<int, string>();

        // Charger tous les questionnaires avec questions
        var questionnaires = await _context.Questionnaires
            .Include(q => q.Questions)
            .ToListAsync();

        var result = questionnaires.Select(q => new QuestionnaireWithReponsesDto
        {
            Id = q.Id,
            Title = q.Title,
            Questions = q.Questions.Select(qu => new QuestionWithReponseDto
            {
                Id = qu.Id,
                Text = qu.Text,
                Type = qu.Type,
                Options = qu.Options,
                QuestionnaireId = q.Id,
                DerniereReponse = dictReponses.TryGetValue(qu.Id, out var val) ? val : null
            }).ToList()
        });

        return result;
    }

    public async Task<DepistageReponsesDto?> GetDepistageReponsesAsync(int depistageId)
    {
        var depistage = await _context.Depistage
            .Include(d => d.Reponses)
            .ThenInclude(r => r.Question)
            .Include(d => d.ResultatIA)
            .FirstOrDefaultAsync(d => d.Id == depistageId);

        if (depistage == null) return null;

        var dto = new DepistageReponsesDto
        {
            DepistageId = depistage.Id,
            PatientId = depistage.PatientId,
            DateDepistage = depistage.Date.ToString("dd/MM/yyyy HH:mm"),
            ResultatId = depistage.ResultatIA?.Id,
            Score = depistage.ResultatIA?.Score,
            Reponses = depistage.Reponses
                .OrderBy(r => r.QuestionId)
                .Select(r => new ReponseDetailDto
                {
                    QuestionId = r.QuestionId,
                    QuestionText = r.Question?.Text,
                    Type = r.Question?.Type,
                    Valeur = r.Valeur
                }).ToList()
        };

        return dto;
    }

}
