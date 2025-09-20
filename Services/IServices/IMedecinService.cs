using backend.Models;
using backend.Dtos.Medecin;

namespace backend.Services.IServices;
public interface IMedecinService
{
    Task<object> CreateMedecinAsync(Medecin medecin);
    Task<IEnumerable<object>> GetAllMedecinsAsync();
    Task<object?> GetMedecinByIdAsync(int id);
    Task<Medecin?> GetMedecinByIdAsync2(int id);
    Task UpdateMedecinAsync(Medecin medecin);
    Task<MedecinDto?> GetMedecinDuPatient(int patientId);
    Task<List<PatientResultatDto>> GetPatientsWithDepistageCountAsync(int medecinId);
    Task<bool> BloquerMedecinAsync(int medecinId);
    Task<bool> DebloquerMedecinAsync(int medecinId);
}