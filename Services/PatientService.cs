using backend.Data;
using backend.Dtos.Patient;
using backend.Models;
using backend.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class PatientService : IPatientService
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;

        public PatientService(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<IEnumerable<object>> GetAllPatientsWithDetailsAsync()
        {
            return await _context.Utilisateur.OfType<Patient>()
                .Include(p => p.Medecin)
                .Select(p => new
                {
                    p.Id,
                    p.Nom,
                    p.Email,
                    p.Profession,
                    p.Telephone,
                    p.DateNaissance,
                    p.Adresse,
                    p.PhotoUrl,
                    p.EstBloque,
                    EstAffecte = p.MedecinId != null,
                    Medecin = p.Medecin != null ? new
                    {
                        p.Medecin.Id,
                        p.Medecin.Nom,
                        p.Medecin.Specialite
                    } : null
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetPatientsNonAffectesAsync()
        {
            return await _context.Utilisateur.OfType<Patient>()
                .Where(p => p.MedecinId == null)
                .Select(p => new
                {
                    p.Id,
                    p.Nom,
                    p.Email,
                    p.Profession,
                    p.Telephone,
                    p.DateNaissance,
                    p.PhotoUrl
                })
                .ToListAsync();
        }

        public async Task<bool> AffecterPatientAsync(int patientId, int medecinId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var patient = await _context.Utilisateur.OfType<Patient>()
                    .FirstOrDefaultAsync(p => p.Id == patientId);

                if (patient == null) return false;

                var medecinExists = await _context.Utilisateur.OfType<Medecin>()
                    .AnyAsync(m => m.Id == medecinId);

                if (!medecinExists) return false;

                patient.MedecinId = medecinId;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DesaffecterPatientAsync(int patientId)
        {
            var patient = await _context.Utilisateur.OfType<Patient>()
                .FirstOrDefaultAsync(p => p.Id == patientId);

            if (patient == null || patient.MedecinId == null) return false;

            patient.MedecinId = null;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<object>> GetPatientsByMedecinAsync(int medecinId)
        {
            return await _context.Utilisateur.OfType<Patient>()
                .Where(p => p.MedecinId == medecinId)
                .Select(p => new
                {
                    p.Id,
                    p.Nom,
                    p.Email,
                    p.Profession,
                    p.Telephone,
                    p.DateNaissance,
                    p.Adresse,
                    p.PhotoUrl
                })
                .ToListAsync();
        }

        public async Task<Patient?> GetPatientByIdAsync(int id)
        {
            return await _context.Patients
                .Include(p => p.Medecin)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task UpdatePatientAsync(Patient patient)
        {
            _context.Patients.Update(patient);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ResultatPatient>> GetResultatParPatient(int patientId)
        {
            var resultats = await _context.ResultatIA
                .Include(r => r.Depistage)
                .Where(r => r.Depistage.PatientId == patientId)
                .OrderByDescending(r => r.Date)
                .Select(r => new ResultatPatient
                {
                    Id = r.Id,
                    DepistageId = r.Depistage.Id,
                    NumeroDepistage = _context.ResultatIA
                        .Count(r2 => r2.Depistage.PatientId == patientId && r2.Date <= r.Date),
                    DateDepistage = r.Depistage.Date.ToString("dd/MM/yyyy HH:mm"),
                    Score = r.Score,
                    Analyse = r.Analyse
                })
                .ToListAsync();

            return resultats;
        }

        public async Task<PatientDet?> GetResultatParPatientDetails(int id)
        {
            var patient = await _context.Patients
                 .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null) return null;

            var resultats = await GetResultatParPatient(id);

            var patientDetails = new PatientDet
            {
                Id = patient.Id,
                Nom = patient.Nom,
                Email = patient.Email,
                DateNaissance = patient.DateNaissance.ToString("dd/MM/yyyy HH:mm"),
                Telephone = patient.Telephone,
                Profession = patient.Profession,
                Resultats = resultats.ToList()
            };

            return patientDetails;
        }

        public async Task<bool> BloquerPatientAsync(int patientId)
        {
            try
            {
                var patient = await _context.Utilisateur.OfType<Patient>()
                    .FirstOrDefaultAsync(p => p.Id == patientId);

                if (patient == null)
                    return false;

                patient.EstBloque = true;
                await _context.SaveChangesAsync();

                // Envoyer l'email de notification de blocage de manière asynchrone
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendAccountBlockedNotification(patient.Email!, patient.Nom ?? "Patient", patient.Role!);
                    }
                    catch (Exception ex)
                    {
                        // Log l'erreur mais ne pas faire échouer l'opération de blocage
                        Console.WriteLine($"Erreur lors de l'envoi de l'email de notification de blocage à {patient.Email}: {ex.Message}");
                    }
                });

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> DebloquerPatientAsync(int patientId)
        {
            try
            {
                var patient = await _context.Utilisateur.OfType<Patient>()
                    .FirstOrDefaultAsync(p => p.Id == patientId);

                if (patient == null)
                    return false;

                patient.EstBloque = false;
                await _context.SaveChangesAsync();
                
                // Envoyer email d'approbation de compte
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendAccountApprovedEmail(patient.Email ?? "", patient.Nom ?? "Utilisateur", "patient");
                    }
                    catch (Exception ex)
                    {
                        // Log l'erreur mais ne pas faire échouer l'opération de déblocage
                        Console.WriteLine($"Erreur lors de l'envoi de l'email d'approbation: {ex.Message}");
                    }
                });
                
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}