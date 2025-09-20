using backend.Data;
using backend.Models;
using backend.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class ConseilService : IConseilService
{
    private readonly AppDbContext _context;

    public ConseilService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Conseil>> GetConseilsPourPatientAsync(int patientId)
    {
        return await _context.Conseils
            .Where(c => c.PatientId == patientId)
            .OrderByDescending(c => c.DateEnvoi)
            .ToListAsync();
    }

    public async Task<IEnumerable<Conseil>> GetConseilsDuMedecinAsync(int medecinId)
    {
        return await _context.Conseils
            .Where(c => c.MedecinId == medecinId)
            .OrderByDescending(c => c.DateEnvoi)
            .ToListAsync();
    }

    public async Task<IEnumerable<Conseil>> GetTousLesConseilsAsync()
    {
        return await _context.Conseils
            .OrderByDescending(c => c.DateEnvoi)
            .ToListAsync();
    }

    public async Task<bool> EnvoyerConseilAsync(Conseil conseil)
    {
        _context.Conseils.Add(conseil);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> ModifierConseilAsync(int id, string nouveauMessage)
    {
        var conseil = await _context.Conseils.FindAsync(id);
        if (conseil == null)
            return false;

        conseil.Message = nouveauMessage;
        await _context.SaveChangesAsync();
        return true;
    }

}
