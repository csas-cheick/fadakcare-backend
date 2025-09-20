using Microsoft.AspNetCore.Mvc;
using backend.Services.IServices;
using backend.Dtos.compte;
using Microsoft.AspNetCore.Authorization;
using BCrypt.Net;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/compte")]
    [Authorize]
    public class ProfilController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly IMedecinService _medecinService;
        private readonly IPatientService _patientService;

        public ProfilController(IAdminService adminService, IMedecinService medecinService, IPatientService patientService)
        {
            _adminService = adminService;
            _medecinService = medecinService;
            _patientService = patientService;
        }

        [HttpGet("profil/{role}/{id}")]
        public async Task<IActionResult> GetProfil(string role, int id)
        {
            switch (role.ToLower())
            {
                case "admin":
                    var admin = await _adminService.GetAdminByIdAsync(id);
                    if (admin == null) return NotFound();
                    return Ok(new
                    {
                        nom = admin.Nom,
                        email = admin.Email,
                        adresse = admin.Adresse,
                        dateNaissance = admin.DateNaissance.ToString("yyyy-MM-dd"),
                        telephone = admin.Telephone,
                        grade = admin.Grade,
                        role = "admin",
                        photoUrl = admin.PhotoUrl
                    });

                case "doctor":
                    var medecin = await _medecinService.GetMedecinByIdAsync2(id);
                    if (medecin == null) return NotFound();
                    return Ok(new
                    {
                        nom = medecin.Nom,
                        email = medecin.Email,
                        adresse = medecin.Adresse,
                        dateNaissance = medecin.DateNaissance.ToString("yyyy-MM-dd"),
                        telephone = medecin.Telephone,
                        service = medecin.Service,
                        specialite = medecin.Specialite,
                        numeroOrdre = medecin.NumeroOrdre,
                        role = "doctor",
                        photoUrl = medecin.PhotoUrl
                    });

                case "patient":
                    var patient = await _patientService.GetPatientByIdAsync(id);
                    if (patient == null) return NotFound();
                    return Ok(new
                    {
                        nom = patient.Nom,
                        email = patient.Email,
                        adresse = patient.Adresse,
                        dateNaissance = patient.DateNaissance.ToString("yyyy-MM-dd"),
                        telephone = patient.Telephone,
                        profession = patient.Profession,
                        medecinId = patient.MedecinId,
                        role = "patient",
                        photoUrl = patient.PhotoUrl
                    });

                default:
                    return BadRequest("Rôle inconnu.");
            }
        }

        [HttpPut("change-password/{role}/{id}")]
        public async Task<IActionResult> ChangePassword(string role, int id, [FromBody] ChangePasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.OldPassword) || string.IsNullOrWhiteSpace(dto.NewPassword))
                return BadRequest(new { message = "Champs requis manquants." });

            switch (role.ToLower())
            {
                case "admin":
                    var admin = await _adminService.GetAdminByIdAsync(id);
                    if (admin == null) return NotFound(new { message = "Admin non trouvé." });
                    if (!admin.MotDePasse!.StartsWith("$2") ? admin.MotDePasse != dto.OldPassword : !BCrypt.Net.BCrypt.Verify(dto.OldPassword, admin.MotDePasse))
                        return BadRequest(new { message = "Ancien mot de passe incorrect." });
                    admin.MotDePasse = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                    await _adminService.UpdateAdminAsync(admin);
                    return Ok(new { message = "Mot de passe modifié avec succès." });

                case "doctor":
                    var medecin = await _medecinService.GetMedecinByIdAsync2(id);
                    if (medecin == null) return NotFound(new { message = "Médecin non trouvé." });
                    if (!medecin.MotDePasse!.StartsWith("$2") ? medecin.MotDePasse != dto.OldPassword : !BCrypt.Net.BCrypt.Verify(dto.OldPassword, medecin.MotDePasse))
                        return BadRequest(new { message = "Ancien mot de passe incorrect." });
                    medecin.MotDePasse = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                    await _medecinService.UpdateMedecinAsync(medecin);
                    return Ok(new { message = "Mot de passe modifié avec succès." });

                case "patient":
                    var patient = await _patientService.GetPatientByIdAsync(id);
                    if (patient == null) return NotFound(new { message = "Patient non trouvé." });
                    if (!patient.MotDePasse!.StartsWith("$2") ? patient.MotDePasse != dto.OldPassword : !BCrypt.Net.BCrypt.Verify(dto.OldPassword, patient.MotDePasse))
                        return BadRequest(new { message = "Ancien mot de passe incorrect." });
                    patient.MotDePasse = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                    await _patientService.UpdatePatientAsync(patient);
                    return Ok(new { message = "Mot de passe modifié avec succès." });

                default:
                    return BadRequest(new { message = "Rôle non reconnu." });
            }
        }

        [HttpPut("update-profile/{role}/{id}")]
        public async Task<IActionResult> UpdateProfile(string role, int id, [FromBody] UpdateProfileDto dto)
        {
            switch (role.ToLower())
            {
                case "admin":
                    var admin = await _adminService.GetAdminByIdAsync(id);
                    if (admin == null) return NotFound();

                    admin.Nom = dto.Nom;
                    admin.Adresse = dto.Adresse;
                    admin.Telephone = dto.Telephone;
                    admin.DateNaissance = dto.DateNaissance;
                    admin.Email = dto.Email;
                    admin.Grade = dto.Grade;

                    await _adminService.UpdateAdminAsync(admin);
                    return Ok(new { message = "Profil admin mis à jour." });

                case "doctor":
                    var medecin = await _medecinService.GetMedecinByIdAsync2(id);
                    if (medecin == null) return NotFound();

                    medecin.Nom = dto.Nom;
                    medecin.Adresse = dto.Adresse;
                    medecin.Telephone = dto.Telephone;
                    medecin.DateNaissance = dto.DateNaissance;
                    medecin.Email = dto.Email;
                    medecin.NumeroOrdre = dto.NumeroOrdre;
                    medecin.Service = dto.Service;
                    medecin.Specialite = dto.Specialite;

                    await _medecinService.UpdateMedecinAsync(medecin);
                    return Ok(new { message = "Profil médecin mis à jour." });

                case "patient":
                    var patient = await _patientService.GetPatientByIdAsync(id);
                    if (patient == null) return NotFound();

                    patient.Nom = dto.Nom;
                    patient.Adresse = dto.Adresse;
                    patient.Telephone = dto.Telephone;
                    patient.DateNaissance = dto.DateNaissance;
                    patient.Email = dto.Email;
                    patient.Profession = dto.Profession;

                    await _patientService.UpdatePatientAsync(patient);
                    return Ok(new { message = "Profil patient mis à jour." });

                default:
                    return BadRequest("Rôle inconnu.");
            }
        }
    }
}

