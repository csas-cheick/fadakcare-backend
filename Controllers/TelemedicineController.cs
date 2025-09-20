using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using backend.Dtos.Telemedicine;
using backend.Services.IServices;


namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TelemedicineController : ControllerBase
    {
        private readonly ITelemedicineService _telemedicineService;

        public TelemedicineController(ITelemedicineService telemedicineService)
        {
            _telemedicineService = telemedicineService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "";
        }

        /// <summary>
        /// Récupérer les participants possibles selon le type de session
        /// </summary>
        [HttpGet("participants/{sessionType}")]
        public async Task<IActionResult> GetAvailableParticipants(string sessionType)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized();

            var userRole = GetCurrentUserRole();
            if (userRole != "doctor")
                return Forbid("Seuls les médecins peuvent accéder à cette ressource");

            try
            {
                var participants = await _telemedicineService.GetAvailableParticipantsAsync(sessionType, userId);
                return Ok(participants);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "Erreur lors de la récupération des participants");
            }
        }

        /// <summary>
        /// Créer une nouvelle session de télémedecine
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateSession([FromBody] CreateTelemedicineDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized();

            var userRole = GetCurrentUserRole();
            if (userRole != "doctor")
                return Forbid("Seuls les médecins peuvent créer des sessions");

            var session = await _telemedicineService.CreateSessionAsync(dto, userId);
            if (session == null)
                return BadRequest("Erreur lors de la création de la session");

            return CreatedAtAction(nameof(GetSession), new { id = session.Id }, session);
        }

        /// <summary>
        /// Mettre à jour une session de télémedecine
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSession(int id, [FromBody] UpdateTelemedicineDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized();

            var userRole = GetCurrentUserRole();
            if (userRole != "doctor")
                return Forbid("Seuls les médecins peuvent modifier des sessions");

            var session = await _telemedicineService.UpdateSessionAsync(id, dto, userId);
            if (session == null)
                return NotFound("Session non trouvée ou accès non autorisé");

            return Ok(session);
        }

        /// <summary>
        /// Supprimer une session de télémedecine
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSession(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized();

            var role = GetCurrentUserRole();
            bool success;
            if (role == "admin" || role == "Admin")
            {
                success = await _telemedicineService.DeleteSessionAsAdminAsync(id);
            }
            else
            {
                success = await _telemedicineService.DeleteSessionAsync(id, userId);
            }
            if (!success)
                return NotFound("Session non trouvée ou accès non autorisé");

            return NoContent();
        }

        /// <summary>
        /// Récupérer une session par son ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSession(int id)
        {
            var session = await _telemedicineService.GetSessionByIdAsync(id);
            if (session == null)
                return NotFound();

            return Ok(session);
        }

        /// <summary>
        /// Récupérer les sessions de l'utilisateur connecté
        /// </summary>
        [HttpGet("my-sessions")]
        public async Task<IActionResult> GetMySessions([FromQuery] string? etat = null)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized();

            var sessions = await _telemedicineService.GetSessionsByUserAsync(userId, etat);
            return Ok(sessions);
        }

        /// <summary>
        /// Récupérer toutes les sessions (admin seulement)
        /// </summary>
        [HttpGet("all")]
        [Authorize(Roles = "admin,Admin")]
        public async Task<IActionResult> GetAllSessions([FromQuery] string? etat = null)
        {
            var sessions = await _telemedicineService.GetAllSessionsAsync(etat);
            return Ok(sessions);
        }

        /// <summary>
        /// Rejoindre une session
        /// </summary>
        [HttpPost("{id}/join")]
        public async Task<IActionResult> JoinSession(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized();

            // Patients ne peuvent rejoindre qu'à l'heure programmée; médecins plus souples
            var role = GetCurrentUserRole();
            var session = await _telemedicineService.GetSessionByIdAsync(id);
            if (session == null)
                return NotFound();

            if (role == "patient")
            {
                var now = DateTime.Now;
                var start = session.DateDebut;
                var end = session.DateDebut.AddMinutes(session.Duree);
                if (now < start.AddMinutes(-5) || now > end)
                {
                    return BadRequest("Vous ne pouvez rejoindre que pendant la période prévue");
                }
            }
            // Médecin: autorisé si programmé ou en_cours (peut démarrer une session programmée)
            if (role == "doctor")
            {
                if (session.Etat != "programmé" && session.Etat != "en_cours")
                {
                    return BadRequest("La session n'est pas disponible");
                }
            }

            var success = await _telemedicineService.JoinSessionAsync(id, userId);
            if (!success)
                return BadRequest("Impossible de rejoindre la session");

            // Mettre à jour l'état de la session si c'est le premier participant et la session était programmée
            if (session.Etat == "programmé")
            {
                await _telemedicineService.UpdateSessionStateAsync(id, "en_cours");
            }

            return Ok(new { message = "Session rejointe avec succès" });
        }

        /// <summary>
        /// Quitter une session
        /// </summary>
        [HttpPost("{id}/leave")]
        public async Task<IActionResult> LeaveSession(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized();

            var success = await _telemedicineService.LeaveSessionAsync(id, userId);
            if (!success)
                return BadRequest("Impossible de quitter la session");

            return Ok(new { message = "Session quittée avec succès" });
        }

        /// <summary>
        /// Ajouter un participant à une session
        /// </summary>
        [HttpPost("{id}/participants")]
        public async Task<IActionResult> AddParticipant(int id, [FromBody] AddParticipantDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized();

            var userRole = GetCurrentUserRole();
            if (userRole != "doctor")
                return Forbid("Seuls les médecins peuvent ajouter des participants");

            var success = await _telemedicineService.AddParticipantAsync(id, dto.UtilisateurId, userId);
            if (!success)
                return BadRequest("Impossible d'ajouter le participant");

            return Ok(new { message = "Participant ajouté avec succès" });
        }

        /// <summary>
        /// Retirer un participant d'une session
        /// </summary>
        [HttpDelete("{id}/participants/{participantId}")]
        public async Task<IActionResult> RemoveParticipant(int id, int participantId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized();

            var userRole = GetCurrentUserRole();
            if (userRole != "doctor")
                return Forbid("Seuls les médecins peuvent retirer des participants");

            var success = await _telemedicineService.RemoveParticipantAsync(id, participantId, userId);
            if (!success)
                return BadRequest("Impossible de retirer le participant");

            return Ok(new { message = "Participant retiré avec succès" });
        }

        /// <summary>
        /// Récupérer les participants d'une session
        /// </summary>
        [HttpGet("{id}/participants")]
        public async Task<IActionResult> GetSessionParticipants(int id)
        {
            var participants = await _telemedicineService.GetSessionParticipantsAsync(id);
            return Ok(participants);
        }

        /// <summary>
        /// Mettre à jour l'état d'une session
        /// </summary>
        [HttpPut("{id}/state")]
        public async Task<IActionResult> UpdateSessionState(int id, [FromBody] UpdateSessionStateDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized();

            var userRole = GetCurrentUserRole();
            if (userRole != "doctor")
                return Forbid("Seuls les médecins peuvent modifier l'état des sessions");

            var success = await _telemedicineService.UpdateSessionStateAsync(id, dto.Etat);
            if (!success)
                return BadRequest("Impossible de mettre à jour l'état de la session");

            return Ok(new { message = "État de la session mis à jour" });
        }

        /// <summary>
        /// Récupérer les sessions à venir (dans les prochaines 24h)
        /// </summary>
        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingSessions()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized();

            var userRole = GetCurrentUserRole();
            List<TelemedicineResponseDto> sessions;

            if (userRole == "admin" || userRole == "Admin")
            {
                // Admin voit toutes les sessions
                sessions = await _telemedicineService.GetAllSessionsAsync("programmé");
            }
            else
            {
                // Utilisateurs normaux voient seulement leurs sessions
                sessions = await _telemedicineService.GetSessionsByUserAsync(userId, "programmé");
            }

            var upcomingSessions = sessions
                .Where(s => s.DateDebut <= DateTime.Now.AddDays(1) && s.DateDebut > DateTime.Now)
                .OrderBy(s => s.DateDebut)
                .ToList();

            return Ok(upcomingSessions);
        }

        /// <summary>
        /// Récupérer l'historique des sessions terminées
        /// </summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetSessionHistory()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized();

            var userRole = GetCurrentUserRole();
            List<TelemedicineResponseDto> sessions;

            if (userRole == "admin" || userRole == "Admin")
            {
                // Admin voit toutes les sessions
                sessions = await _telemedicineService.GetAllSessionsAsync();
            }
            else
            {
                // Utilisateurs normaux voient seulement leurs sessions
                sessions = await _telemedicineService.GetSessionsByUserAsync(userId);
            }

            var historySessions = sessions
                .Where(s => s.Etat == "terminé" || s.Etat == "annulé")
                .OrderByDescending(s => s.DateDebut)
                .ToList();

            return Ok(historySessions);
        }
    }

    public class UpdateSessionStateDto
    {
        public string Etat { get; set; } = "";
    }
}
