using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using backend.Services.IServices;
using backend.Dtos.Depistage;
using backend.Dtos.Questionnaire;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/depistage")]
 [Authorize]
public class DepistageController : ControllerBase
{
    private readonly IDepistageService _depistageService;
    private readonly backend.Data.AppDbContext _context;

    public DepistageController(IDepistageService depistageService, backend.Data.AppDbContext context)
    {
        _depistageService = depistageService;
        _context = context;
    }

    [HttpPost("seDepister")]
    public async Task<IActionResult> SoumettreDepistage([FromBody] SoumissionDepistageDto dto)
    {
        try
        {
            await _depistageService.SoumettreDepistageAsync(dto);
            return Ok(new
            {
                success = true,
                message = "Dépistage soumis avec succès."
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = $"Erreur lors de la soumission : {ex.Message}"
            });
        }
    }
    [HttpGet("dernier/{patientId}")]
    public async Task<IActionResult> DernierDepistage(int patientId)
    {
        var depistage = await _depistageService.GetDernierDepistageAsync(patientId);
        if (depistage == null) return NotFound();
        return Ok(depistage);
    }

    [HttpGet("prefill/{patientId}")]
    public async Task<ActionResult<IEnumerable<QuestionnaireWithReponsesDto>>> GetQuestionnairesAvecDernieresReponses(int patientId)
    {
        var data = await _depistageService.GetQuestionnairesAvecDernieresReponsesAsync(patientId);
        return Ok(data);
    }

    [HttpGet("{depistageId}/reponses")]
    public async Task<ActionResult<DepistageReponsesDto>> GetDepistageReponses(int depistageId)
    {
        var dto = await _depistageService.GetDepistageReponsesAsync(depistageId);
        if (dto == null) return NotFound();
        return Ok(dto);
    }

    [HttpGet("resultat/{resultatId}/reponses")]
    public async Task<ActionResult<DepistageReponsesDto>> GetDepistageReponsesParResultat(int resultatId)
    {
        var depistageId = await _context.ResultatIA
            .Where(r => r.Id == resultatId)
            .Select(r => r.DepistageId)
            .FirstOrDefaultAsync();
        if (depistageId == 0) return NotFound();
        var dto = await _depistageService.GetDepistageReponsesAsync(depistageId);
        if (dto == null) return NotFound();
        return Ok(dto);
    }

}
