using backend.Data;
using backend.Models;
using backend.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context, ILogger<DashboardService> logger)
        {
            _context = context;
        }

        public async Task<object> GetDashboardAdminAsync()
        {
            var depistagesParJour = await _context.Depistage
                .GroupBy(d => d.Date.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count()
                }).ToListAsync();
            var scoreGrave = await _context.ResultatIA
                .CountAsync(s => s.Score > 5);

            var patientNonAffectes = await _context.Patients
                .CountAsync(p => p.MedecinId == null);
            
            var moyenneScore = await _context.ResultatIA
                .AverageAsync(r => (double?)r.Score) ?? 0;


            var totalPatients = await _context.Patients.CountAsync();
            var totalMedecins = await _context.Medecins.CountAsync();
            var totalAdmins = await _context.Admins.CountAsync();
            var totalConseils = await _context.Conseils.CountAsync();
            var totalAlertes = await _context.Alertes.CountAsync();
            var totalDepistages = await _context.Depistage.CountAsync();

            return new
            {
                DepistagesParJour = depistagesParJour,
                MoyenneScore = moyenneScore,
                TotalPatients = totalPatients,
                TotalMedecins = totalMedecins,
                TotalAdmins = totalAdmins,
                TotalConseils = totalConseils,
                TotalAlertes = totalAlertes,
                TotalDepistages = totalDepistages,
                ScoreGraves = scoreGrave,
                PatientNonAffectes = patientNonAffectes
            };
        }

        public async Task<object> GetDashboardMedecinAsync(int medecinId)
        {
            // 1. Dépistages par jour (de ses patients)
            var depistagesParJour = await _context.Depistage
                .Where(d => d.Patient.MedecinId == medecinId)
                .GroupBy(d => d.Date.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count()
                }).ToListAsync();

            // 2. Moyenne des scores de ses patients
            var moyenneScore = await _context.ResultatIA
                .Where(r => r.Depistage.Patient.MedecinId == medecinId)
                .AverageAsync(r => (double?)r.Score) ?? 0;

            // 3. Nombre total de ses patients
            var totalPatients = await _context.Patients
                .Where(p => p.MedecinId == medecinId)
                .CountAsync();

            // 4. Nombre total de conseils envoyés par ce médecin
            var totalConseils = await _context.Conseils
                .Where(c => c.MedecinId == medecinId)
                .CountAsync();

            // 5. Nombre total d’alertes envoyées et reçues par ce médecin
            var totalAlertesEnvoyees = await _context.Alertes
                .Where(a => a.ExpediteurId == medecinId)
                .CountAsync();

            var totalAlertesRecues = await _context.Alertes
                .Where(a => a.DestinataireId == medecinId)
                .CountAsync();

            var totalAlertes = totalAlertesEnvoyees + totalAlertesRecues;

            return new
            {
                DepistagesParJour = depistagesParJour,
                MoyenneScore = moyenneScore,
                TotalPatients = totalPatients,
                TotalConseils = totalConseils,
                TotalAlertesEnvoyees = totalAlertesEnvoyees,
                TotalAlertesRecues = totalAlertesRecues,
                TotalAlertes = totalAlertes // champ attendu par le frontend (totalAlertes)
            };
        }

        public async Task<object> GetDashboardPatientAsync(int patientId)
        {
            var depistages = await _context.Depistage
                .Where(d => d.PatientId == patientId)
                .ToListAsync();

            // Grouper par semaine (calculée en C#)
            var depistagesParSemaine = depistages
                .GroupBy(d =>
                {
                    var dfi = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
                    return System.Globalization.CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(d.Date, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
                })
                .Select(g => new
                {
                    Semaine = g.Key,
                    Count = g.Count()
                }).ToList();

            var depistagesParMois = depistages
                .GroupBy(d => d.Date.ToString("yyyy-MM"))
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count()
                }).ToList();

            // Moyenne des scores
            var moyenneScore = await _context.ResultatIA
                .Where(r => r.Depistage.PatientId == patientId)
                .AverageAsync(r => (double?)r.Score) ?? 0;

            var totalConseilsRecus = await _context.Conseils
                .Where(c => c.PatientId == patientId)
                .CountAsync();

            var totalDepistages = await _context.Depistage
                .Where(d => d.PatientId == patientId)
                .CountAsync();

            var dernierScore = await _context.ResultatIA
                .Where(r => r.Depistage.PatientId == patientId)
                .OrderByDescending(r => r.Date)
                .Select(r => (double?)r.Score)
                .FirstOrDefaultAsync();

            var totalAlertesRec = await _context.Alertes
                .Where(a => a.DestinataireId == patientId)
                .CountAsync();
            var totalAlertesEnv = await _context.Alertes
                .Where(a => a.ExpediteurId == patientId)
                .CountAsync();

            return new
            {
                DepistagesParSemaine = depistagesParSemaine,
                DepistagesParMois = depistagesParMois,
                DernierScore = dernierScore,
                TotalDepistages = totalDepistages,
                MoyenneScore = moyenneScore,
                TotalConseilsRecus = totalConseilsRecus,
                TotalAlertes = totalAlertesRec + totalAlertesEnv
            };
        }



    }
}
