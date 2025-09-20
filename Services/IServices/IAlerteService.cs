using backend.Dtos.Alerte;
using backend.Models;

namespace backend.Services.IServices;

public interface IAlerteService
{
    Task<IEnumerable<AlerteDto>> GetAlertesPourUtilisateur(int userId, string role);
    Task<Alerte> EnvoyerAlerte(Alerte alerte);
    Task<IEnumerable<AlerteDto>> GetToutesLesAlertesAsync();
}