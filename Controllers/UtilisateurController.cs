using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
namespace backend.Controllers;

[ApiController]
[Route("api/utilisateurs")]
[Authorize]
public class UtilisateurController : ControllerBase
{
    private readonly AppDbContext _context;

    public UtilisateurController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchUsers([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Ok(new List<Utilisateur>());

        var users = await _context.Utilisateur
            .Where(u => u.Nom != null && u.Nom.Contains(query))
            .Select(u => new
            {
                id = u.Id,
                utilisateurId = u.Id,
                nom = u.Nom,
                role = u.Role
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("search-by-session")]
    public async Task<IActionResult> SearchUsersBySession(
    [FromQuery] string query,
    [FromQuery] string type,
    [FromQuery] int medecinId)
    {
        if (string.IsNullOrWhiteSpace(query)) return Ok(new List<object>());

        IQueryable<Utilisateur> users = _context.Utilisateur;

        if (type == "patient-medecin" || type == "patients-medecin")
        {
            IQueryable<Patient> patient = _context.Patients;
            users = patient.Where(u => u.Role == "patient" && u.MedecinId == medecinId);
        }
        else if (type == "medecin-medecins")
        {
            // 🔎 Tous les médecins sauf soi-même
            users = users.Where(u => u.Role == "doctor" && u.Id != medecinId);
        }
        else
        {
            return BadRequest("Type de session inconnu");
        }

        var result = await users
            .Where(u => u.Nom != null && u.Nom.Contains(query))
            .Select(u => new
            {
                id = u.Id,
                utilisateurId = u.Id,
                nom = u.Nom,
                role = u.Role
            })
            .ToListAsync();

        return Ok(result);
    }

}
