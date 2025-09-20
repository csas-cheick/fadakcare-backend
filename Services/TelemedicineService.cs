using backend.Data;
using backend.Models;
using backend.Dtos.Telemedicine;
using backend.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class TelemedicineService : ITelemedicineService
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public TelemedicineService(AppDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<TelemedicineResponseDto?> CreateSessionAsync(CreateTelemedicineDto dto, int createurId)
        {
            var session = new Telemedecine
            {
                CreateurId = createurId,
                Titre = dto.Titre,
                Description = dto.Description,
                DateDebut = dto.DateDebut,
                Duree = dto.Duree,
                Type = dto.Type,
                Etat = "programmé",
                IdSalle = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.Now
            };

            _context.Telemedecines.Add(session);
            await _context.SaveChangesAsync();

            // Ajouter les participants
            if (dto.ParticipantsIds != null && dto.ParticipantsIds.Any())
            {
                foreach (var participantId in dto.ParticipantsIds)
                {
                    var participant = new ParticipantTelemedecine
                    {
                        UtilisateurId = participantId,
                        TelemedicineId = session.Id,
                        Role = "participant",
                        Etat = "en_attente"
                    };
                    _context.ParticipantsTelemedecine.Add(participant);
                }
                await _context.SaveChangesAsync();
            }

            // Envoyer des notifications
            await SendSessionCreatedNotificationsAsync(session, dto.ParticipantsIds);

            return await GetSessionByIdAsync(session.Id);
        }

        public async Task<TelemedicineResponseDto?> UpdateSessionAsync(int id, UpdateTelemedicineDto dto, int userId)
        {
            var session = await _context.Telemedecines
                .Include(t => t.Participants)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (session == null || session.CreateurId != userId)
                return null;

            if (!string.IsNullOrEmpty(dto.Titre))
                session.Titre = dto.Titre;
            
            if (!string.IsNullOrEmpty(dto.Description))
                session.Description = dto.Description;
            
            if (dto.DateDebut.HasValue)
                session.DateDebut = dto.DateDebut.Value;
            
            if (dto.Duree.HasValue)
                session.Duree = dto.Duree.Value;
            
            if (!string.IsNullOrEmpty(dto.Etat))
                session.Etat = dto.Etat;

            // Mise à jour des participants si nécessaire
            if (dto.ParticipantsIds != null)
            {
                // Supprimer les anciens participants
                var oldParticipants = await _context.ParticipantsTelemedecine
                    .Where(p => p.TelemedicineId == id)
                    .ToListAsync();
                _context.ParticipantsTelemedecine.RemoveRange(oldParticipants);

                // Ajouter les nouveaux participants
                foreach (var participantId in dto.ParticipantsIds)
                {
                    var participant = new ParticipantTelemedecine
                    {
                        UtilisateurId = participantId,
                        TelemedicineId = session.Id,
                        Role = "participant",
                        Etat = "en_attente"
                    };
                    _context.ParticipantsTelemedecine.Add(participant);
                }
            }

            await _context.SaveChangesAsync();
            return await GetSessionByIdAsync(id);
        }

        public async Task<bool> DeleteSessionAsync(int id, int userId)
        {
            var session = await _context.Telemedecines
                .FirstOrDefaultAsync(t => t.Id == id);

            if (session == null || session.CreateurId != userId)
                return false;

            // Supprimer les participants d'abord
            var participants = await _context.ParticipantsTelemedecine
                .Where(p => p.TelemedicineId == id)
                .ToListAsync();
            _context.ParticipantsTelemedecine.RemoveRange(participants);

            _context.Telemedecines.Remove(session);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSessionAsAdminAsync(int id)
        {
            var session = await _context.Telemedecines
                .FirstOrDefaultAsync(t => t.Id == id);

            if (session == null)
                return false;

            // Supprimer les participants d'abord
            var participants = await _context.ParticipantsTelemedecine
                .Where(p => p.TelemedicineId == id)
                .ToListAsync();
            _context.ParticipantsTelemedecine.RemoveRange(participants);

            _context.Telemedecines.Remove(session);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<TelemedicineResponseDto>> GetSessionsByUserAsync(int userId, string? etat = null)
        {
            var query = _context.Telemedecines
                .Include(t => t.Createur)
                .Include(t => t.Participants!)
                    .ThenInclude(p => p.Utilisateur)
                .Where(t => t.CreateurId == userId || 
                           t.Participants!.Any(p => p.UtilisateurId == userId));

            if (!string.IsNullOrEmpty(etat))
            {
                var filteredQuery = query.Where(t => t.Etat == etat);
                var sessions = await filteredQuery.OrderByDescending(t => t.DateDebut).ToListAsync();
                return sessions.Select(MapToResponseDto).ToList();
            }

            var allSessions = await query.OrderByDescending(t => t.DateDebut).ToListAsync();
            return allSessions.Select(MapToResponseDto).ToList();
        }

        public async Task<List<TelemedicineResponseDto>> GetAllSessionsAsync(string? etat = null)
        {
            var query = _context.Telemedecines
                .Include(t => t.Createur)
                .Include(t => t.Participants!)
                    .ThenInclude(p => p.Utilisateur);

            if (!string.IsNullOrEmpty(etat))
            {
                var filteredQuery = query.Where(t => t.Etat == etat);
                var sessions = await filteredQuery.OrderByDescending(t => t.DateDebut).ToListAsync();
                return sessions.Select(MapToResponseDto).ToList();
            }

            var allSessions = await query.OrderByDescending(t => t.DateDebut).ToListAsync();
            return allSessions.Select(MapToResponseDto).ToList();
        }

        public async Task<TelemedicineResponseDto?> GetSessionByIdAsync(int id)
        {
            var session = await _context.Telemedecines
                .Include(t => t.Createur)
                .Include(t => t.Participants!)
                    .ThenInclude(p => p.Utilisateur)
                .FirstOrDefaultAsync(t => t.Id == id);

            return session != null ? MapToResponseDto(session) : null;
        }

        public async Task<bool> JoinSessionAsync(int sessionId, int userId)
        {
            var participant = await _context.ParticipantsTelemedecine
                .FirstOrDefaultAsync(p => p.TelemedicineId == sessionId && p.UtilisateurId == userId);

            if (participant == null)
            {
                // Charger la session pour appliquer les règles d'auto-adhésion
                var session = await _context.Telemedecines.FirstOrDefaultAsync(t => t.Id == sessionId);
                if (session == null)
                    return false;

                // 1) Le créateur (médecin) peut rejoindre même s'il n'est pas pré-ajouté
                if (session.CreateurId == userId)
                {
                    participant = new ParticipantTelemedecine
                    {
                        UtilisateurId = userId,
                        TelemedicineId = sessionId,
                        Role = "participant",
                        Etat = "connecté",
                        HeureArrivee = DateTime.Now
                    };
                    _context.ParticipantsTelemedecine.Add(participant);
                    await _context.SaveChangesAsync();
                    return true;
                }

                // 2) Autoriser un patient à rejoindre une session 1-1 de son médecin même s'il n'a pas été pré-ajouté
                var patient = await _context.Utilisateur.OfType<Patient>()
                    .FirstOrDefaultAsync(p => p.Id == userId);

                if (patient != null
                    && patient.MedecinId.HasValue
                    && patient.MedecinId.Value == session.CreateurId
                    && string.Equals(session.Type, "medecin_patient", StringComparison.OrdinalIgnoreCase))
                {
                    // Ajouter automatiquement le patient comme participant et le marquer connecté
                    participant = new ParticipantTelemedecine
                    {
                        UtilisateurId = userId,
                        TelemedicineId = sessionId,
                        Role = "participant",
                        Etat = "connecté",
                        HeureArrivee = DateTime.Now
                    };
                    _context.ParticipantsTelemedecine.Add(participant);
                    await _context.SaveChangesAsync();
                    return true;
                }

                // Pas autorisé si non participant et conditions ci-dessus non remplies
                return false;
            }

            participant.Etat = "connecté";
            participant.HeureArrivee = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> LeaveSessionAsync(int sessionId, int userId)
        {
            var participant = await _context.ParticipantsTelemedecine
                .FirstOrDefaultAsync(p => p.TelemedicineId == sessionId && p.UtilisateurId == userId);

            if (participant == null)
                return false;

            participant.Etat = "déconnecté";
            participant.HeureDepart = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddParticipantAsync(int sessionId, int participantId, int createurId)
        {
            var session = await _context.Telemedecines
                .FirstOrDefaultAsync(t => t.Id == sessionId && t.CreateurId == createurId);

            if (session == null)
                return false;

            var existingParticipant = await _context.ParticipantsTelemedecine
                .FirstOrDefaultAsync(p => p.TelemedicineId == sessionId && p.UtilisateurId == participantId);

            if (existingParticipant != null)
                return false;

            var participant = new ParticipantTelemedecine
            {
                UtilisateurId = participantId,
                TelemedicineId = sessionId,
                Role = "participant",
                Etat = "en_attente"
            };

            _context.ParticipantsTelemedecine.Add(participant);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveParticipantAsync(int sessionId, int participantId, int createurId)
        {
            var session = await _context.Telemedecines
                .FirstOrDefaultAsync(t => t.Id == sessionId && t.CreateurId == createurId);

            if (session == null)
                return false;

            var participant = await _context.ParticipantsTelemedecine
                .FirstOrDefaultAsync(p => p.TelemedicineId == sessionId && p.UtilisateurId == participantId);

            if (participant == null)
                return false;

            _context.ParticipantsTelemedecine.Remove(participant);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateSessionStateAsync(int sessionId, string newState)
        {
            var session = await _context.Telemedecines
                .FirstOrDefaultAsync(t => t.Id == sessionId);

            if (session == null)
                return false;

            session.Etat = newState;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ParticipantResponseDto>> GetSessionParticipantsAsync(int sessionId)
        {
            var participants = await _context.ParticipantsTelemedecine
                .Include(p => p.Utilisateur)
                .Where(p => p.TelemedicineId == sessionId)
                .ToListAsync();

            return participants.Select(p => new ParticipantResponseDto
            {
                Id = p.Id,
                UtilisateurId = p.UtilisateurId,
                UtilisateurNom = p.Utilisateur?.Nom,
                UtilisateurRole = p.Utilisateur?.Role,
                Role = p.Role,
                Etat = p.Etat,
                HeureArrivee = p.HeureArrivee,
                HeureDepart = p.HeureDepart
            }).ToList();
        }

        private TelemedicineResponseDto MapToResponseDto(Telemedecine session)
        {
            return new TelemedicineResponseDto
            {
                Id = session.Id,
                CreateurId = session.CreateurId,
                CreateurNom = session.Createur?.Nom,
                Titre = session.Titre,
                Description = session.Description,
                DateDebut = session.DateDebut,
                Duree = session.Duree,
                Type = session.Type,
                Etat = session.Etat,
                IdSalle = session.IdSalle,
                CreatedAt = session.CreatedAt,
                Participants = session.Participants?.Select(p => new ParticipantResponseDto
                {
                    Id = p.Id,
                    UtilisateurId = p.UtilisateurId,
                    UtilisateurNom = p.Utilisateur?.Nom,
                    UtilisateurRole = p.Utilisateur?.Role,
                    Role = p.Role,
                    Etat = p.Etat,
                    HeureArrivee = p.HeureArrivee,
                    HeureDepart = p.HeureDepart
                }).ToList()
            };
        }

        public async Task<List<object>> GetAvailableParticipantsAsync(string sessionType, int medecinId)
        {
            var participants = new List<object>();

            switch (sessionType.ToLower())
            {
                case "medecin_patient":
                case "medecin_patients":
                    // Récupérer les patients affectés au médecin
                    var patients = await _context.Utilisateur.OfType<Patient>()
                        .Where(p => p.MedecinId == medecinId)
                        .Select(p => new
                        {
                            id = p.Id,
                            nom = p.Nom,
                            role = "patient",
                            email = p.Email,
                            telephone = p.Telephone,
                            profession = p.Profession
                        })
                        .ToListAsync();
                    
                    participants.AddRange(patients);
                    break;

                case "medecin_medecin":
                    // Récupérer tous les autres médecins (sauf le médecin connecté)
                    var medecins = await _context.Utilisateur.OfType<Medecin>()
                        .Where(m => m.Id != medecinId)
                        .Select(m => new
                        {
                            id = m.Id,
                            nom = m.Nom,
                            role = "doctor",
                            email = m.Email,
                            telephone = m.Telephone,
                            specialite = m.Specialite,
                            service = m.Service
                        })
                        .ToListAsync();
                    
                    participants.AddRange(medecins);
                    break;

                default:
                    throw new ArgumentException($"Type de session non supporté: {sessionType}");
            }

            return participants;
        }

        private async Task SendSessionCreatedNotificationsAsync(Telemedecine session, List<int>? participantsIds)
        {
            try
            {
                // Obtenir les informations du créateur
                var createur = await _context.Utilisateur.FirstOrDefaultAsync(u => u.Id == session.CreateurId);
                if (createur == null) return;

                var createurName = createur.Nom ?? "Utilisateur";
                var sessionTitle = session.Titre ?? "Session de télémédecine";
                var sessionDate = session.DateDebut.ToString("dd/MM/yyyy à HH:mm");

                // Message de notification
                var notificationMessage = $"📅 Nouvelle session programmée: \"{sessionTitle}\" par {createurName} le {sessionDate}";

                // Envoyer notification aux participants
                if (participantsIds != null && participantsIds.Any())
                {
                    foreach (var participantId in participantsIds)
                    {
                        var notification = new Notification
                        {
                            Type = "session_created",
                            Message = notificationMessage,
                            UtilisateurId = participantId,
                            DateNotif = DateTime.Now
                        };
                        await _notificationService.CreateAsync(notification);
                    }
                }

                // Envoyer notification à tous les admins
                var admins = await _context.Admins.ToListAsync();
                foreach (var admin in admins)
                {
                    var adminNotification = new Notification
                    {
                        Type = "session_created",
                        Message = $"🏥 {notificationMessage} (Admin notification)",
                        UtilisateurId = admin.Id,
                        DateNotif = DateTime.Now
                    };
                    await _notificationService.CreateAsync(adminNotification);
                }
            }
            catch (Exception ex)
            {
                // Log l'erreur mais ne pas faire échouer la création de session
                Console.WriteLine($"Erreur lors de l'envoi des notifications: {ex.Message}");
            }
        }
    }
}
