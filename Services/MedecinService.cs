using backend.Data;
using backend.Models;
using backend.Services.IServices;
using Microsoft.EntityFrameworkCore;
using backend.Dtos.Medecin;
namespace backend.Services
{
    public class MedecinService : IMedecinService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<MedecinService> _logger;
        private readonly EmailService _emailService;

        public MedecinService(AppDbContext context, ILogger<MedecinService> logger, EmailService emailService)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<object> CreateMedecinAsync(Medecin medecin)
        {
            if (string.IsNullOrEmpty(medecin.Email) || string.IsNullOrEmpty(medecin.MotDePasse))
                throw new ArgumentException("Email et mot de passe sont requis.");

            var verifierExist = await _context.Utilisateur.FirstOrDefaultAsync(u => u.Email == medecin.Email);
            if (verifierExist != null)
                throw new InvalidOperationException("Cet email est déjà utilisé");

            // Sauvegarder le mot de passe en clair pour l'email
            var motDePasseClair = medecin.MotDePasse;
            
            medecin.Role = "doctor";
            // Hasher le mot de passe pour la base de données
            medecin.MotDePasse = BCrypt.Net.BCrypt.HashPassword(medecin.MotDePasse);

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Utilisateur.Add(medecin);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendConfirmationEmailMedecinWithCredentials(medecin.Email, medecin.Nom ?? "Médecin", medecin.Email, motDePasseClair);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erreur lors de l'envoi de l'email de confirmation");
                    }
                });

                return new
                {
                    message = "Médecin créé avec succès ! Un email avec les paramètres de connexion a été envoyé.",
                    medecinId = medecin.Id
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Erreur lors de la création du médecin");
                throw;
            }
        }

        public async Task<IEnumerable<object>> GetAllMedecinsAsync()
        {
            return await _context.Utilisateur.OfType<Medecin>()
                .Include(m => m.Patients)
                .Select(m => new
                {
                    m.Id,
                    m.Nom,
                    m.Email,
                    m.Telephone,
                    m.Specialite,
                    m.NumeroOrdre,
                    m.Service,
                    m.PhotoUrl,
                    m.EstBloque,
                    NombrePatients = m.Patients.Count,
                    Patients = m.Patients.Select(p => new
                    {
                        p.Id,
                        p.Nom,
                        p.Email,
                        p.DateNaissance,
                        p.Profession,
                        p.Adresse
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<object?> GetMedecinByIdAsync(int id)
        {
            var medecin = await _context.Utilisateur.OfType<Medecin>()
                .Include(m => m.Patients)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medecin == null)
                return null;

            return new
            {
                medecin.Id,
                medecin.Nom,
                medecin.DateNaissance,
                medecin.Email,
                medecin.Telephone,
                medecin.Specialite,
                medecin.NumeroOrdre,
                medecin.Service,
                medecin.PhotoUrl,
                Patients = medecin.Patients.Select(p => new
                {
                    p.Id,
                    p.Nom,
                    p.Email,
                    p.Telephone,
                    Profession = p.Profession
                })
            };
        }

        public async Task<Medecin?> GetMedecinByIdAsync2(int id)
        {
            return await _context.Medecins
            .Include(m => m.Patients)
            .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task UpdateMedecinAsync(Medecin medecin)
        {
            _context.Medecins.Update(medecin);
            await _context.SaveChangesAsync();
        }

        public async Task<MedecinDto?> GetMedecinDuPatient(int patientId)
        {
            var patient = await _context.Patients
                .Include(p => p.Medecin)
                .FirstOrDefaultAsync(p => p.Id == patientId);

            if (patient == null || patient.Medecin == null)
                return null;

            return new MedecinDto
            {
                Nom = patient.Medecin.Nom,
                Specialite = patient.Medecin.Specialite,
                Telephone = patient.Medecin.Telephone,
                Email = patient.Medecin.Email,
                Service = patient.Medecin.Service,
                PhotoUrl = patient.Medecin.PhotoUrl
            };
        }

        public async Task<List<PatientResultatDto>> GetPatientsWithDepistageCountAsync(int medecinId)
        {
            return await _context.Patients
                .Where(p => p.MedecinId == medecinId)
                .Select(p => new PatientResultatDto
                {
                    Id = p.Id,
                    Nom = p.Nom,
                    DateNaissance = p.DateNaissance,
                    Email = p.Email,
                    Adresse = p.Adresse,
                    PhotoUrl = p.PhotoUrl,
                    NombreDepistages = p.Depistages.Count()
                })
                .ToListAsync();
        }

    public async Task<bool> BloquerMedecinAsync(int medecinId)
    {
        try
        {
            var medecin = await _context.Utilisateur.OfType<Medecin>()
                .FirstOrDefaultAsync(m => m.Id == medecinId);

            if (medecin == null)
                return false;

            medecin.EstBloque = true;
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Médecin {medecin.Email} bloqué avec succès");

            // Envoyer l'email de notification de blocage de manière asynchrone
            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendAccountBlockedNotification(medecin.Email!, medecin.Nom ?? "Médecin", medecin.Role!);
                    _logger.LogInformation($"Email de notification de blocage envoyé à {medecin.Email}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Erreur lors de l'envoi de l'email de notification de blocage à {medecin.Email}");
                }
            });

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erreur lors du blocage du médecin {medecinId}");
            throw;
        }
    }

    public async Task<bool> DebloquerMedecinAsync(int medecinId)
    {
        try
        {
            var medecin = await _context.Utilisateur.OfType<Medecin>()
                .FirstOrDefaultAsync(m => m.Id == medecinId);

            if (medecin == null)
                return false;

            medecin.EstBloque = false;
            await _context.SaveChangesAsync();
            
            // Envoyer email d'approbation de compte
            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendAccountApprovedEmail(medecin.Email, medecin.Nom ?? "Utilisateur", "doctor");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de l'envoi de l'email d'approbation de compte");
                }
            });
            
            _logger.LogInformation($"Médecin {medecin.Email} débloqué avec succès");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erreur lors du déblocage du médecin {medecinId}");
            throw;
        }
    }

    }
}
