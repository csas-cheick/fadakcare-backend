using backend.Data;
using backend.Models;
using backend.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class RendezVousService : IRendezVousService
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public RendezVousService(AppDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<RendezVous?> CreerRendezVousAsync(RendezVous rdv)
        {
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.Id == rdv.PatientId);

            if (patient == null || patient.MedecinId == null)
                return null;

            rdv.MedecinId = patient.MedecinId.Value;

            _context.RendezVous.Add(rdv);
            await _context.SaveChangesAsync();

            // Create notification for medecin when patient creates a new appointment
            var notification = new Notification
            {
                Type = "rendez-vous",
                Message = $"Nouvelle demande de rendez-vous de {patient.Nom} pour le {rdv.Date:dd/MM/yyyy à HH:mm}. Motif: {rdv.Motif}",
                UtilisateurId = rdv.MedecinId,
                DateNotif = DateTime.Now,
                Lu = false
            };
            await _notificationService.CreateAsync(notification);

            return rdv;
        }


        public async Task<IEnumerable<RendezVous>> GetRendezVousParPatientAsync(int patientId)
        {
            return await _context.RendezVous
                .Where(r => r.PatientId == patientId)
                .Include(r => r.Medecin)
                .OrderByDescending(r => r.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<RendezVous>> GetRendezVousParMedecinAsync(int medecinId)
        {
            return await _context.RendezVous
                .Where(r => r.MedecinId == medecinId)
                .Include(r => r.Patient)
                .OrderByDescending(r => r.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<RendezVous>> GetAllRendezVousAsync()
        {
            return await _context.RendezVous
                .Include(r => r.Patient)
                .Include(r => r.Medecin)
                .OrderByDescending(r => r.Date)
                .ToListAsync();
        }

        public async Task<bool> ModifierEtatAsync(int id, string nouvelEtat)
        {
            var rdv = await _context.RendezVous
                .Include(r => r.Patient)
                .Include(r => r.Medecin)
                .FirstOrDefaultAsync(r => r.Id == id);
            
            if (rdv == null) return false;
            
            string ancienEtat = rdv.Etat;
            rdv.Etat = nouvelEtat;
            await _context.SaveChangesAsync();

            // Create notification for patient when appointment status changes
            if (ancienEtat != nouvelEtat && rdv.Patient != null)
            {
                string message = nouvelEtat switch
                {
                    "accepté" => $"Votre rendez-vous du {rdv.Date:dd/MM/yyyy à HH:mm} a été accepté par Dr. {rdv.Medecin?.Nom}",
                    "refusé" => $"Votre rendez-vous du {rdv.Date:dd/MM/yyyy à HH:mm} a été refusé par Dr. {rdv.Medecin?.Nom}",
                    _ => $"Statut de votre rendez-vous mis à jour: {nouvelEtat}"
                };

                var notification = new Notification
                {
                    Type = "rendez-vous",
                    Message = message,
                    UtilisateurId = rdv.PatientId,
                    DateNotif = DateTime.Now,
                    Lu = false
                };
                await _notificationService.CreateAsync(notification);
            }

            return true;
        }

        public async Task<RendezVous?> ModifierRendezVousAsync(RendezVous rdv)
        {
            var existant = await _context.RendezVous.FindAsync(rdv.Id);
            if (existant == null) return null;

            existant.Date = rdv.Date;
            existant.Motif = rdv.Motif;
            await _context.SaveChangesAsync();
            return existant;
        }

        public async Task<bool> SupprimerRendezVousAsync(int id)
        {
            var rdv = await _context.RendezVous.FindAsync(id);
            if (rdv == null) return false;

            rdv.Etat = "annulé";
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<RendezVous?> GetByIdAsync(int id)
        {
            return await _context.RendezVous
               .Include(r => r.Patient)
               .Include(r => r.Medecin)
               .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<object> GetStatistiquesAsync(string role, int? userId = null)
        {
            var now = DateTime.UtcNow;
            var query = _context.RendezVous.AsQueryable();

            if (role == "patient")
                query = query.Where(r => r.PatientId == userId);
            else if (role == "medecin")
                query = query.Where(r => r.MedecinId == userId);

            var data = await query.ToListAsync();

            return new
            {
                enAttente = data.Count(r => r.Etat == "en_attente"),
                acceptes = data.Count(r => r.Etat == "accepté"),
                refuses = data.Count(r => r.Etat == "refusé"),
                aVenir = data.Count(r => r.Date > now),
                passes = data.Count(r => r.Date <= now)
            };
        }

        public async Task<RendezVous?> GetProchainRendezVousAsync(int patientId)
        {
            return await _context.RendezVous
                .Where(r => r.PatientId == patientId && r.Date >= DateTime.Now && r.Etat == "accepté")
                .OrderBy(r => r.Date)
                .FirstOrDefaultAsync();
        }

    }
}