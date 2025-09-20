using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using backend.Services.IServices;
using Microsoft.EntityFrameworkCore;
using backend.Dtos.compte;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;

namespace backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;
        private readonly ILogger<AuthService> _logger;
        private readonly IConfiguration _config;

        public AuthService(AppDbContext context, EmailService emailService, ILogger<AuthService> logger, IConfiguration config)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
            _config = config;
        }

        private string GenerateJwtToken(Utilisateur user, DateTime expiresAt)
        {
            var jwtSection = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Normaliser le rôle pour correspondre aux attributs [Authorize(Roles="...")]
            var normalizedRole = NormalizeRole(user.Role);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, normalizedRole),
                new Claim("name", user.Nom ?? string.Empty),
                new Claim("role_original", user.Role ?? string.Empty)
            };
            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: expiresAt,
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string NormalizeRole(string? role)
        {
            if (string.IsNullOrWhiteSpace(role)) return string.Empty;
            var r = role.Trim().ToLowerInvariant();

            // Gestion des variantes (accents etc.)
            if (r.StartsWith("médec") || r.StartsWith("medec")) return "doctor"; // médecin / medecin
            return r switch
            {
                "medecin" => "doctor",
                "doctor" => "doctor",
                "patient" => "patient",
                "admin" => "admin",
                _ => r // laisser tel quel si non mappé
            };
        }

        private string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        }

        private async Task<RefreshToken> CreateRefreshTokenAsync(int userId)
        {
            var days = _config.GetValue<int>("Jwt:RefreshTokenDays", 7);
            var refresh = new RefreshToken
            {
                UserId = userId,
                Token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"),
                ExpiresAt = DateTime.UtcNow.AddDays(days),
                Revoked = false
            };
            _context.RefreshTokens.Add(refresh);
            await _context.SaveChangesAsync();
            return refresh;
        }

        private object BuildAuthResponse(Utilisateur user, string accessToken, DateTime expires, RefreshToken refresh)
        {
            return new AuthResponse
            {
                UserId = user.Id,
                Email = user.Email,
                Role = user.Role,
                AccessToken = accessToken,
                ExpiresAt = expires,
                RefreshToken = refresh.Token
            };
        }

        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {
            if (string.IsNullOrWhiteSpace(loginRequest.Email) || string.IsNullOrWhiteSpace(loginRequest.MotDePasse))
                return new BadRequestObjectResult(new { message = "Veuillez remplir tous les champs." });

            var user = await _context.Utilisateur.FirstOrDefaultAsync(u => u.Email == loginRequest.Email);
            if (user == null || string.IsNullOrEmpty(user.MotDePasse))
                return new UnauthorizedObjectResult(new { message = "Email ou mot de passe incorrect." });

            // Vérifier si l'utilisateur est bloqué
            if (user.EstBloque)
                return new UnauthorizedObjectResult(new { 
                    message = "Votre compte a été bloqué. Veuillez contacter l'administrateur à l'adresse : fadakcare@gmail.com",
                    isBlocked = true 
                });

            // Upgrade path: if stored password is not a BCrypt hash (no $2 prefix), hash it now after verifying plaintext match
            var isBcrypt = user.MotDePasse.StartsWith("$2");
            if (!isBcrypt)
            {
                if (user.MotDePasse != loginRequest.MotDePasse)
                    return new UnauthorizedObjectResult(new { message = "Email ou mot de passe incorrect." });
                user.MotDePasse = BCrypt.Net.BCrypt.HashPassword(user.MotDePasse);
                _context.Utilisateur.Update(user);
                await _context.SaveChangesAsync();
            }
            else if (!BCrypt.Net.BCrypt.Verify(loginRequest.MotDePasse, user.MotDePasse))
            {
                return new UnauthorizedObjectResult(new { message = "Email ou mot de passe incorrect." });
            }
            var minutes = _config.GetValue<int>("Jwt:AccessTokenMinutes", 30);
            var expires = DateTime.UtcNow.AddMinutes(minutes);
            var accessToken = GenerateJwtToken(user, expires);
            var refresh = await CreateRefreshTokenAsync(user.Id);
            var auth = BuildAuthResponse(user, accessToken, expires, refresh);
            user.isOnline = true;
            _context.Utilisateur.Update(user);
            await _context.SaveChangesAsync();
            Console.WriteLine(user.isOnline);
            return new OkObjectResult(auth);
        }

        public async Task<IActionResult> ForgotPassword(ForgotPassword Fpassword)
        {
            var utilisateur = await _context.Utilisateur.FirstOrDefaultAsync(u => u.Email == Fpassword.Email);
            if (utilisateur == null)
                return new BadRequestObjectResult(new { message = "Cet email n'est pas encore enregistré" });

            string codeVerification = new Random().Next(100000, 999999).ToString();
            var passwordReset = new PasswordReset
            {
                Email = Fpassword.Email,
                Code = codeVerification,
                Expiration = DateTime.UtcNow.AddMinutes(10)
            };

            _context.PasswordResets.Add(passwordReset);
            await _context.SaveChangesAsync();
            await _emailService.SendResetPasswordEmail(Fpassword.Email!, codeVerification);

            return new OkObjectResult(new { message = "Un code de vérification a été envoyé par email." });
        }

        public async Task<IActionResult> VerifyCode(VerifyCode request)
        {
            var passwordReset = await _context.PasswordResets
                .FirstOrDefaultAsync(pr => pr.Email == request.Email && pr.Code == request.Code);

            if (passwordReset == null || passwordReset.Expiration < DateTime.UtcNow)
                return new BadRequestObjectResult(new { message = "Code invalide ou expiré." });

            return new OkObjectResult(new { message = "Code vérifié avec succès." });
        }

        public async Task<IActionResult> ResetPassword(ResetPassword request)
        {
            var passwordReset = await _context.PasswordResets
                .FirstOrDefaultAsync(pr => pr.Email == request.Email && pr.Code == request.Code);

            if (passwordReset == null || passwordReset.Expiration < DateTime.UtcNow)
                return new BadRequestObjectResult(new { message = "Code invalide ou expiré." });

            var utilisateur = await _context.Utilisateur.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (utilisateur == null)
                return new BadRequestObjectResult(new { message = "Utilisateur introuvable." });

            // Hash the new password (supports upgrade path if old stored as plaintext)
            utilisateur.MotDePasse = BCrypt.Net.BCrypt.HashPassword(request.newPassword);
            _context.Utilisateur.Update(utilisateur);
            _context.PasswordResets.Remove(passwordReset);
            await _context.SaveChangesAsync();

            return new OkObjectResult(new { message = "Mot de passe réinitialisé avec succès." });
        }

        public async Task<IActionResult> Register(Utilisateur utilisateur)
        {
            if (string.IsNullOrEmpty(utilisateur.Email) || string.IsNullOrEmpty(utilisateur.MotDePasse))
                return new BadRequestObjectResult(new { message = "Tous les champs sont requis." });

            var verifierExist = await _context.Utilisateur
                .FirstOrDefaultAsync(u => u.Email == utilisateur.Email);

            if (verifierExist != null)
                return new BadRequestObjectResult(new { message = "Cet email est déjà utilisé" });

            // Hash password
            var hashed = BCrypt.Net.BCrypt.HashPassword(utilisateur.MotDePasse);

            var patient = new Patient
            {
                Nom = utilisateur.Nom,
                Email = utilisateur.Email,
                MotDePasse = hashed,
                DateNaissance = utilisateur.DateNaissance,
                Telephone = utilisateur.Telephone,
                Adresse = utilisateur.Adresse,
                Role = utilisateur.Role,
                Profession = "Non renseigné",
                MedecinId = null,
                EstBloque = true,
                isOnline = false
            };
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.Utilisateur.Add(patient);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendAccountPendingValidationEmail(patient.Email, patient.Nom ?? "Utilisateur", "patient");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erreur lors de l'envoi de l'email de validation en attente");
                    }
                });

                var minutes = _config.GetValue<int>("Jwt:AccessTokenMinutes", 30);
                var expires = DateTime.UtcNow.AddMinutes(minutes);
                var accessToken = GenerateJwtToken(patient, expires);
                var refresh = await CreateRefreshTokenAsync(patient.Id);
                var authResp = BuildAuthResponse(patient, accessToken, expires, refresh);
                return new OkObjectResult(authResp);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Erreur lors de l'inscription");
                return new BadRequestObjectResult(new { message = "Une erreur est survenue lors de l'inscription" });
            }
        }

        public async Task<IActionResult> RefreshToken(RefreshRequest request)
        {
            if (request.UserId <= 0 || string.IsNullOrWhiteSpace(request.RefreshToken))
                return new BadRequestObjectResult(new { message = "Requête invalide" });

            var stored = await _context.RefreshTokens.FirstOrDefaultAsync(r => r.UserId == request.UserId && r.Token == request.RefreshToken);
            if (stored == null || stored.Revoked || stored.ExpiresAt < DateTime.UtcNow)
                return new UnauthorizedObjectResult(new { message = "Refresh token invalide" });

            var user = await _context.Utilisateur.FirstOrDefaultAsync(u => u.Id == request.UserId);
            if (user == null)
                return new UnauthorizedObjectResult(new { message = "Utilisateur introuvable" });

            var minutes = _config.GetValue<int>("Jwt:AccessTokenMinutes", 30);
            var expires = DateTime.UtcNow.AddMinutes(minutes);
            var accessToken = GenerateJwtToken(user, expires);
            return new OkObjectResult(new AuthResponse
            {
                UserId = user.Id,
                Email = user.Email,
                Role = user.Role,
                AccessToken = accessToken,
                ExpiresAt = expires,
                RefreshToken = stored.Token
            });
        }

        public async Task<IActionResult> GoogleLogin(GoogleLoginRequest request)
        {
            // Vérifier si l'utilisateur existe déjà par email
            var existingUser = await _context.Patients.FirstOrDefaultAsync(p => p.Email == request.Email) as Utilisateur
                            ?? await _context.Medecins.FirstOrDefaultAsync(m => m.Email == request.Email) as Utilisateur
                            ?? await _context.Admins.FirstOrDefaultAsync(a => a.Email == request.Email) as Utilisateur;

            if (existingUser != null)
            {
                // Vérifier si l'utilisateur est bloqué
                if (existingUser.EstBloque)
                    return new UnauthorizedObjectResult(new { 
                        message = "Votre compte a été bloqué. Veuillez contacter l'administrateur à l'adresse : fadakcare@gmail.com",
                        isBlocked = true 
                    });

                // Utilisateur existant - générer token et retourner
                var existingMinutes = _config.GetValue<int>("Jwt:AccessTokenMinutes", 30);
                var existingExpires = DateTime.UtcNow.AddMinutes(existingMinutes);
                var existingAccessToken = GenerateJwtToken(existingUser, existingExpires);
                
                var existingRefreshToken = GenerateRefreshToken();
                var existingRefreshEntity = new RefreshToken
                {
                    Token = existingRefreshToken,
                    UserId = existingUser.Id,
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow
                };
                _context.RefreshTokens.Add(existingRefreshEntity);
                await _context.SaveChangesAsync();

                return new OkObjectResult(new AuthResponse
                {
                    UserId = existingUser.Id,
                    Email = existingUser.Email,
                    Role = existingUser.Role,
                    AccessToken = existingAccessToken,
                    ExpiresAt = existingExpires,
                    RefreshToken = existingRefreshToken
                });
            }

            // Nouvel utilisateur - créer un compte patient
            var newPatient = new Patient
            {
                Nom = request.Nom,
                Email = request.Email,
                MotDePasse = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()), // Mot de passe aléatoire
                Role = "patient",
                DateNaissance = DateTime.Now.AddYears(-25), // Âge par défaut
                Telephone = "", // Vide pour l'instant
                Adresse = "", // Vide pour l'instant
                Profession = "", // Vide pour l'instant
                MedecinId = null, // Aucun médecin assigné pour l'instant
                GoogleId = request.GoogleId,
                EstBloque = true,
                isOnline = false
            };

            _context.Patients.Add(newPatient);
            await _context.SaveChangesAsync();

            // Envoyer email de validation en attente
            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendAccountPendingValidationEmail(newPatient.Email, newPatient.Nom ?? "Utilisateur", "patient");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de l'envoi de l'email de validation en attente");
                }
            });

            // Générer tokens pour le nouveau patient
            var minutes = _config.GetValue<int>("Jwt:AccessTokenMinutes", 30);
            var expires = DateTime.UtcNow.AddMinutes(minutes);
            var accessToken = GenerateJwtToken(newPatient, expires);
            
            var refreshToken = GenerateRefreshToken();
            var refreshEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = newPatient.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };
            _context.RefreshTokens.Add(refreshEntity);
            await _context.SaveChangesAsync();

            return new OkObjectResult(new AuthResponse
            {
                UserId = newPatient.Id,
                Email = newPatient.Email,
                Role = newPatient.Role,
                AccessToken = accessToken,
                ExpiresAt = expires,
                RefreshToken = refreshToken
            });
        }

        public async Task<IActionResult> Logout(RefreshRequest request)
        {
            var stored = await _context.RefreshTokens.FirstOrDefaultAsync(r => r.UserId == request.UserId && r.Token == request.RefreshToken);
            var user = await _context.Utilisateur.FirstOrDefaultAsync(u => u.Id == request.UserId);
            if (stored != null)
            {
                stored.Revoked = true;
                if (user != null)
                {
                    user.isOnline = false;
                    _context.Utilisateur.Update(user);
                }
                await _context.SaveChangesAsync();
            }
            return new OkObjectResult(new { message = "Déconnecté" });
        }
    }
}
