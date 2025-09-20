using backend.Models;
using backend.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using backend.Dtos.Medecin;

namespace backend.Controllers;

[ApiController]
[Route("api/medecins")]
 [Authorize]
public class MedecinsController : ControllerBase
{
    private readonly IMedecinService _medecinService;

    public MedecinsController(IMedecinService medecinService)
    {
        _medecinService = medecinService;
    }

    [HttpPost("ajouterMedecin")]
    public async Task<IActionResult> CreateMedecin([FromBody] Medecin medecin)
    {
        try
        {
            var result = await _medecinService.CreateMedecinAsync(medecin);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch
        {
            return StatusCode(500, new { message = "Une erreur est survenue" });
        }
    }

    [HttpGet("listeMedecins")]
    public async Task<IActionResult> GetMedecins()
    {
        var result = await _medecinService.GetAllMedecinsAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMedecin(int id)
    {
        var result = await _medecinService.GetMedecinByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("MonMedecin/{patientId}")]
    public async Task<ActionResult<MedecinDto>> GetMedecinDuPatient(int patientId)
    {
        var medecin = await _medecinService.GetMedecinDuPatient(patientId);

        if (medecin == null)
            return NotFound(new { message = "Médecin non trouvé pour ce patient." });

        return Ok(medecin);
    }

    [HttpGet("{medecinId}/Mespatients")]
    public async Task<IActionResult> GetPatients(int medecinId)
    {
        var patients = await _medecinService.GetPatientsWithDepistageCountAsync(medecinId);
        return Ok(patients);
    }

    [HttpPost("{medecinId}/bloquer")]
    public async Task<IActionResult> BloquerMedecin(int medecinId)
    {
        try
        {
            var success = await _medecinService.BloquerMedecinAsync(medecinId);
            if (!success)
                return NotFound(new { message = "Médecin non trouvé" });

            return Ok(new { message = "Médecin bloqué avec succès" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{medecinId}/debloquer")]
    public async Task<IActionResult> DebloquerMedecin(int medecinId)
    {
        try
        {
            var success = await _medecinService.DebloquerMedecinAsync(medecinId);
            if (!success)
                return NotFound(new { message = "Médecin non trouvé" });

            return Ok(new { message = "Médecin débloqué avec succès" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

}
