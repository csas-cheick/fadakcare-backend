using backend.Models;
using backend.Services.IServices;
using Microsoft.AspNetCore.Mvc;


namespace backend.Controllers
{
    [ApiController]
    [Route("api/rendezvous")]
    public class RendezVousController : ControllerBase
    {
        private readonly IRendezVousService _service;

        public RendezVousController(IRendezVousService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Creer(RendezVous rdv)
        {
            var result = await _service.CreerRendezVousAsync(rdv);
            return CreatedAtAction(nameof(GetParId), new { id = result.Id }, result);
        }

        [HttpGet("patient/{patientId}")]
        public async Task<IActionResult> GetParPatient(int patientId)
        {
            var list = await _service.GetRendezVousParPatientAsync(patientId);
            return Ok(list);
        }

        [HttpGet("medecin/{medecinId}")]
        public async Task<IActionResult> GetParMedecin(int medecinId)
        {
            var list = await _service.GetRendezVousParMedecinAsync(medecinId);
            return Ok(list);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetAllRendezVousAsync();
            return Ok(list);
        }

        [HttpPut("{id}/etat")]
        public async Task<IActionResult> ModifierEtat(int id, [FromBody] string etat)
        {
            var result = await _service.ModifierEtatAsync(id, etat);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpPut]
        public async Task<IActionResult> Modifier(RendezVous rdv)
        {
            var updated = await _service.ModifierRendezVousAsync(rdv);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Supprimer(int id)
        {
            var result = await _service.SupprimerRendezVousAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetParId(int id)
        {
            var rdv = await _service.GetByIdAsync(id);
            if (rdv == null) return NotFound();
            return Ok(rdv);
        }

        [HttpGet("statistiques/{role}/{userId?}")]
        public async Task<IActionResult> GetStats(string role, int? userId = null)
        {
            var stats = await _service.GetStatistiquesAsync(role, userId);
            return Ok(stats);
        }

        [HttpGet("prochain/{patientId}")]
        public async Task<IActionResult> ProchainRendezVous(int patientId)
        {
            var prochain = await _service.GetProchainRendezVousAsync(patientId);
            if (prochain == null) return NotFound();
            return Ok(prochain);
        }


    }
}