using Microsoft.AspNetCore.Mvc;
using backend.Services;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers;

[Route("api/patients")]
[ApiController]
[Authorize]
public class PatientsController : ControllerBase
{
    private readonly PatientService _patientService;

    public PatientsController(PatientService patientService)
    {
        _patientService = patientService;
    }

    [HttpGet("details")]
    public async Task<ActionResult<IEnumerable<object>>> GetAllPatientsWithDetails()
    {
        var result = await _patientService.GetAllPatientsWithDetailsAsync();
        return Ok(result);
    }

    [HttpGet("non-affectes")]
    public async Task<ActionResult<IEnumerable<object>>> GetPatientsNonAffectes()
    {
        var result = await _patientService.GetPatientsNonAffectesAsync();
        return Ok(result);
    }

    [HttpPut("{patientId}/affecter/{medecinId}")]
    public async Task<IActionResult> AffecterPatient(int patientId, int medecinId)
    {
        try
        {
            var result = await _patientService.AffecterPatientAsync(patientId, medecinId);
            return result
                ? Ok(new { success = true, message = "Patient affecté avec succès" })
                : BadRequest(new { success = false, message = "Patient ou médecin introuvable" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = $"Erreur lors de l'affectation: {ex.Message}"
            });
        }
    }

    [HttpPut("{patientId}/desaffecter")]
    public async Task<IActionResult> DesaffecterPatient(int patientId)
    {
        try
        {
            var result = await _patientService.DesaffecterPatientAsync(patientId);
            return result
                ? Ok(new { success = true, message = "Patient désaffecté avec succès" })
                : BadRequest(new { success = false, message = "Patient introuvable ou non affecté" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = $"Erreur lors de la désaffectation: {ex.Message}"
            });
        }
    }

    [HttpGet("medecin/{medecinId}")]
    public async Task<ActionResult<IEnumerable<object>>> GetPatientsByMedecin(int medecinId)
    {
        try
        {
            var patients = await _patientService.GetPatientsByMedecinAsync(medecinId);
            return Ok(patients);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = $"Erreur lors de la récupération des patients: {ex.Message}"
            });
        }
    }

    [HttpGet("resultat/{patientId}")]
    public async Task<IActionResult> ResultatParPatient(int patientId)
    {
        try
        {
            var resultats = await _patientService.GetResultatParPatient(patientId);

            if (!resultats.Any())
            {
                return NotFound(new { Message = "Aucun résultat trouvé pour ce patient" });
            }

            return Ok(resultats);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Une erreur interne est survenue", ex });
        }
    }

    [HttpGet("resultat/{patientId}/details")]
    public async Task<IActionResult> ResultatParPatientDetails(int patientId)
    {
        try
        {
            var patientDet = await _patientService.GetResultatParPatientDetails(patientId);

            if (patientDet == null)
            {
                return NotFound(new { Message = "Aucun patient trouvé avec ce id" });
            }

            return Ok(patientDet);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Une erreur interne est survenue", ex });
        }
    }

    // === Endpoints ajoutés pour correspondre aux appels frontend (medecinPatientsService) ===
    // GET /api/patients/{patientId}
    [HttpGet("{patientId}")]
    public async Task<IActionResult> GetPatientById(int patientId)
    {
        try
        {
            var patient = await _patientService.GetPatientByIdAsync(patientId);
            if (patient == null)
                return NotFound(new { message = "Patient introuvable" });

            return Ok(new
            {
                id = patient.Id,
                nom = patient.Nom,
                email = patient.Email,
                adresse = patient.Adresse,
                telephone = patient.Telephone,
                dateNaissance = patient.DateNaissance.ToString("yyyy-MM-dd"),
                profession = patient.Profession,
                photoUrl = patient.PhotoUrl,
                medecinId = patient.MedecinId
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur interne", detail = ex.Message });
        }
    }

    // GET /api/patients/{patientId}/depistages
    [HttpGet("{patientId}/depistages")]
    public async Task<IActionResult> GetDepistagesPatient(int patientId)
    {
        try
        {
            var resultats = await _patientService.GetResultatParPatient(patientId);
            // Retourner une liste (éventuellement vide) pour éviter un 404 côté frontend lors de l'absence de résultats
            return Ok(resultats);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur interne", detail = ex.Message });
        }
    }

    [HttpPost("{patientId}/bloquer")]
    public async Task<IActionResult> BloquerPatient(int patientId)
    {
        try
        {
            var success = await _patientService.BloquerPatientAsync(patientId);
            if (!success)
                return NotFound(new { message = "Patient non trouvé" });

            return Ok(new { message = "Patient bloqué avec succès" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{patientId}/debloquer")]
    public async Task<IActionResult> DebloquerPatient(int patientId)
    {
        try
        {
            var success = await _patientService.DebloquerPatientAsync(patientId);
            if (!success)
                return NotFound(new { message = "Patient non trouvé" });

            return Ok(new { message = "Patient débloqué avec succès" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
