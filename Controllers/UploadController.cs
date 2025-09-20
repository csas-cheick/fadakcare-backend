using backend.Services;
using backend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [Route("api/upload")]
    [ApiController]
    [Authorize]
    public class UploadController : ControllerBase
    {
        private readonly ICloudinaryService _cloudinaryService;
        private readonly AppDbContext _context;
        private readonly ILogger<UploadController> _logger;

        public UploadController(ICloudinaryService cloudinaryService, AppDbContext context, ILogger<UploadController> logger)
        {
            _cloudinaryService = cloudinaryService;
            _context = context;
            _logger = logger;
        }

        [HttpPost("profile-photo")]
        public async Task<IActionResult> UploadProfilePhoto([FromForm] IFormFile file, [FromForm] string userId, [FromForm] string userType)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "Aucun fichier sélectionné" });

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userType))
                    return BadRequest(new { message = "UserId et UserType sont requis" });

                // Validation du userType
                var allowedUserTypes = new[] { "patient", "medecin", "admin" };
                if (!allowedUserTypes.Contains(userType.ToLower()))
                    return BadRequest(new { message = "Type d'utilisateur non valide" });

                var photoUrl = await _cloudinaryService.UploadProfilePhotoAsync(file, userId, userType);

                // Mettre à jour l'URL de la photo dans la base de données
                var userIdInt = int.Parse(userId);
                var utilisateur = await _context.Utilisateur.FirstOrDefaultAsync(u => u.Id == userIdInt);
                
                if (utilisateur != null)
                {
                    utilisateur.PhotoUrl = photoUrl;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("PhotoUrl mise à jour dans la base de données pour l'utilisateur ID {UserId}: {PhotoUrl}", userId, photoUrl);
                }
                else
                {
                    _logger.LogWarning("Utilisateur avec ID {UserId} non trouvé lors de la mise à jour de PhotoUrl", userId);
                }

                return Ok(new 
                { 
                    message = "Photo de profil uploadée avec succès",
                    photoUrl = photoUrl,
                    success = true
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Erreur de validation lors de l'upload: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message, success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'upload de la photo de profil");
                return StatusCode(500, new { message = "Erreur interne du serveur", success = false });
            }
        }

        [HttpDelete("profile-photo/{publicId}")]
        public async Task<IActionResult> DeleteProfilePhoto(string publicId)
        {
            try
            {
                var success = await _cloudinaryService.DeletePhotoAsync(publicId);
                
                if (success)
                    return Ok(new { message = "Photo supprimée avec succès", success = true });
                else
                    return BadRequest(new { message = "Erreur lors de la suppression", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de la photo");
                return StatusCode(500, new { message = "Erreur interne du serveur", success = false });
            }
        }
    }
}