using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;

namespace backend.Services
{
    public interface ICloudinaryService
    {
        Task<string> UploadProfilePhotoAsync(IFormFile file, string userId, string userType);
        Task<bool> DeletePhotoAsync(string publicId);
    }

    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryService> _logger;

        public CloudinaryService(IConfiguration configuration, ILogger<CloudinaryService> logger)
        {
            var cloudinaryConfig = configuration.GetSection("Cloudinary");
            var account = new Account(
                cloudinaryConfig["CloudName"],
                cloudinaryConfig["ApiKey"],
                cloudinaryConfig["ApiSecret"]
            );
            _cloudinary = new Cloudinary(account);
            _logger = logger;
        }

        public async Task<string> UploadProfilePhotoAsync(IFormFile file, string userId, string userType)
        {
            try
            {
                if (file == null || file.Length == 0)
                    throw new ArgumentException("Fichier non valide");

                // Validation du type de fichier
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/jpg", "image/webp" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                    throw new ArgumentException("Type de fichier non supporté. Utilisez JPG, PNG ou WebP");

                // Validation de la taille (max 5MB)
                if (file.Length > 5 * 1024 * 1024)
                    throw new ArgumentException("Le fichier est trop volumineux. Taille maximum : 5MB");

                using var stream = file.OpenReadStream();
                
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(file.FileName, stream),
                    PublicId = $"fadakcare/{userType.ToLower()}s/{userId}/profile",
                    Folder = $"fadakcare/{userType.ToLower()}s/{userId}",
                    Transformation = new Transformation()
                        .Width(400)
                        .Height(400)
                        .Crop("fill")
                        .Quality("auto")
                        .FetchFormat("auto"),
                    Overwrite = true,
                    UniqueFilename = false,
                    UseFilename = false
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                
                if (uploadResult.Error != null)
                {
                    _logger.LogError("Erreur upload Cloudinary: {Error}", uploadResult.Error.Message);
                    throw new Exception($"Erreur lors de l'upload: {uploadResult.Error.Message}");
                }

                _logger.LogInformation("Photo de profil uploadée avec succès pour {UserType} ID {UserId}", userType, userId);
                return uploadResult.SecureUrl.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'upload de la photo de profil pour {UserType} ID {UserId}", userType, userId);
                throw;
            }
        }

        public async Task<bool> DeletePhotoAsync(string publicId)
        {
            try
            {
                var deleteParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deleteParams);
                
                return result.Result == "ok";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de la photo avec PublicId: {PublicId}", publicId);
                return false;
            }
        }
    }
}