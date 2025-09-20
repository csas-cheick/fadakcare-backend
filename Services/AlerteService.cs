using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using backend.Services.IServices;
using backend.Dtos.Alerte;
namespace backend.Services;

public class AlerteService : IAlerteService
{
    private readonly AppDbContext _context;
    private readonly INotificationService _notificationService;

    public AlerteService(AppDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<AlerteDto>> GetAlertesPourUtilisateur(int userId, string role)
    {
        var alertes = await _context.Alertes
            .Where(a =>
                (a.ExpediteurId == userId && a.ExpediteurRole == role) ||
                (a.DestinataireId == userId && a.DestinataireRole == role))
            .OrderByDescending(a => a.DateEnvoi)
            .ToListAsync();

        var result = new List<AlerteDto>();

        foreach (var a in alertes)
        {
            string expediteurNom = a.ExpediteurRole == "doctor"
                ? await _context.Medecins.Where(m => m.Id == a.ExpediteurId).Select(m => m.Nom).FirstOrDefaultAsync()
                : await _context.Patients.Where(p => p.Id == a.ExpediteurId).Select(p => p.Nom).FirstOrDefaultAsync();

            string destinataireNom = a.DestinataireRole == "doctor"
                ? await _context.Medecins.Where(m => m.Id == a.DestinataireId).Select(m => m.Nom).FirstOrDefaultAsync()
                : await _context.Patients.Where(p => p.Id == a.DestinataireId).Select(p => p.Nom).FirstOrDefaultAsync();

            string? expediteurPhotoUrl = await _context.Utilisateur
                .Where(u => u.Id == a.ExpediteurId)
                .Select(u => u.PhotoUrl)
                .FirstOrDefaultAsync();

            string? destinatairePhotoUrl = await _context.Utilisateur
                .Where(u => u.Id == a.DestinataireId)
                .Select(u => u.PhotoUrl)
                .FirstOrDefaultAsync();

            result.Add(new AlerteDto
            {
                Id = a.Id,
                Message = a.Message,
                DateEnvoi = a.DateEnvoi,
                ExpediteurNom = expediteurNom,
                DestinataireNom = destinataireNom,
                ExpediteurRole = a.ExpediteurRole,
                DestinataireRole = a.DestinataireRole,
                ExpediteurPhotoUrl = expediteurPhotoUrl,
                DestinatairePhotoUrl = destinatairePhotoUrl
            });
        }

        return result;
    }

    public async Task<Alerte> EnvoyerAlerte(Alerte alerte)
    {
        _context.Alertes.Add(alerte);
        await _context.SaveChangesAsync();
        
        // Get sender name for notification
        string? expediteurNom = alerte.ExpediteurRole == "doctor"
            ? await _context.Medecins.Where(m => m.Id == alerte.ExpediteurId).Select(m => m.Nom).FirstOrDefaultAsync()
            : await _context.Patients.Where(p => p.Id == alerte.ExpediteurId).Select(p => p.Nom).FirstOrDefaultAsync();
        
        // Create a professional notification message (like rendez-vous style)
        string notificationMessage = alerte.ExpediteurRole == "doctor"
            ? $"Nouvelle alerte de Dr. {expediteurNom ?? "Médecin"}"
            : $"Nouvelle alerte de {expediteurNom ?? "Patient"}";
        
        // Create a notification for the destinataire user
        var notification = new Notification
        {
            Type = "alerte",
            Message = notificationMessage,
            UtilisateurId = alerte.DestinataireId,
            DateNotif = DateTime.Now,
            Lu = false
        };
        await _notificationService.CreateAsync(notification);
        
        return alerte;
    }

    public async Task<IEnumerable<AlerteDto>> GetToutesLesAlertesAsync()
    {
        var alertes = await _context.Alertes
            .OrderByDescending(a => a.DateEnvoi)
            .ToListAsync();

        var alertesDto = new List<AlerteDto>();

        foreach (var a in alertes)
        {
            string expediteurNom = a.ExpediteurRole == "doctor"
                ? await _context.Medecins.Where(m => m.Id == a.ExpediteurId).Select(m => m.Nom).FirstOrDefaultAsync()
                : await _context.Patients.Where(p => p.Id == a.ExpediteurId).Select(p => p.Nom).FirstOrDefaultAsync();

            string destinataireNom = a.DestinataireRole == "doctor"
                ? await _context.Medecins.Where(m => m.Id == a.DestinataireId).Select(m => m.Nom).FirstOrDefaultAsync()
                : await _context.Patients.Where(p => p.Id == a.DestinataireId).Select(p => p.Nom).FirstOrDefaultAsync();

            alertesDto.Add(new AlerteDto
            {
                Id = a.Id,
                Message = a.Message,
                DateEnvoi = a.DateEnvoi,
                ExpediteurNom = expediteurNom,
                DestinataireNom = destinataireNom,
                ExpediteurRole = a.ExpediteurRole,
                DestinataireRole = a.DestinataireRole,
                ExpediteurPhotoUrl = a.ExpediteurRole == "doctor"
                    ? await _context.Medecins.Where(m => m.Id == a.ExpediteurId).Select(m => m.PhotoUrl).FirstOrDefaultAsync()
                    : await _context.Patients.Where(p => p.Id == a.ExpediteurId).Select(p => p.PhotoUrl).FirstOrDefaultAsync(),
                DestinatairePhotoUrl = a.DestinataireRole == "doctor"
                    ? await _context.Medecins.Where(m => m.Id == a.DestinataireId).Select(m => m.PhotoUrl).FirstOrDefaultAsync()
                    : await _context.Patients.Where(p => p.Id == a.DestinataireId).Select(p => p.PhotoUrl).FirstOrDefaultAsync()
            });
        }

        return alertesDto;
    }
}
