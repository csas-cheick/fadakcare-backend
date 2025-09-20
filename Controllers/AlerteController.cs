using backend.Dtos.Alerte;
using backend.Models;
using backend.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/alerte")]
[Authorize]
public class AlerteController : ControllerBase
{
    private readonly IAlerteService _alerteService;

    public AlerteController(IAlerteService alerteService)
    {
        _alerteService = alerteService;
    }

    [HttpGet("{role}/{id}")]
    public async Task<ActionResult<IEnumerable<AlerteDto>>> GetAlertesPourUtilisateur(string role, int id)
    {
        Console.WriteLine($"GetAlertesPourUtilisateur called with role: {role}, userId: {id}");

        try
        {
            var alertes = await _alerteService.GetAlertesPourUtilisateur(id, role);
            return Ok(alertes);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = $"Erreur lors de la récupération des alertes : {ex.Message}"
            });
        }
    }


    [HttpPost("envoyer")]
    public async Task<IActionResult> EnvoyerAlerte([FromBody] Alerte alerte)
    {
        var nouvelleAlerte = await _alerteService.EnvoyerAlerte(alerte);
        return Ok(nouvelleAlerte);
    }

    [HttpGet("toutes")]
    public async Task<ActionResult<IEnumerable<AlerteDto>>> GetToutesLesAlertes()
    {
        try
        {
            var alertes = await _alerteService.GetToutesLesAlertesAsync();
            return Ok(alertes);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = $"Erreur lors de la récupération des alertes : {ex.Message}"
            });
        }
    }

}
