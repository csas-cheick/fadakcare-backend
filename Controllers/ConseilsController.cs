using backend.Models;
using backend.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

[Route("api/conseils")]
[ApiController]
[Authorize]
public class ConseilsController : ControllerBase
{
    private readonly IConseilService _conseilService;

    public ConseilsController(IConseilService conseilService)
    {
        _conseilService = conseilService;
    }

    [HttpGet("patient/{patientId}")]
    public async Task<IActionResult> GetConseilsPourPatient(int patientId)
    {
        var conseils = await _conseilService.GetConseilsPourPatientAsync(patientId);
        return Ok(conseils);
    }

    [HttpGet("medecin/{medecinId}")]
    public async Task<IActionResult> GetConseilsDuMedecin(int medecinId)
    {
        var conseils = await _conseilService.GetConseilsDuMedecinAsync(medecinId);
        return Ok(conseils);
    }

    [HttpGet("tous")]
    public async Task<IActionResult> GetTous()
    {
        var conseils = await _conseilService.GetTousLesConseilsAsync();
        return Ok(conseils);
    }

    [HttpPost("envoyer")]
    public async Task<IActionResult> Envoyer([FromBody] Conseil conseil)
    {
        var result = await _conseilService.EnvoyerConseilAsync(conseil);
        return result ? Ok(new { success = true }) : BadRequest(new { success = false });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> ModifierConseil(int id, [FromBody] string nouveauMessage)
    {
        var success = await _conseilService.ModifierConseilAsync(id, nouveauMessage);
        return success
            ? Ok(new { success = true, message = "Conseil modifié avec succès." })
            : NotFound(new { success = false, message = "Conseil introuvable." });
    }

}
