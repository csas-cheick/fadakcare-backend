using backend.Dtos.Patient;
using backend.Models;

namespace backend.Services.IServices;

public interface IPatientService
{
    Task<IEnumerable<object>> GetAllPatientsWithDetailsAsync();
    Task<IEnumerable<object>> GetPatientsNonAffectesAsync();
    Task<bool> AffecterPatientAsync(int patientId, int medecinId);
    Task<bool> DesaffecterPatientAsync(int patientId);
    Task<IEnumerable<object>> GetPatientsByMedecinAsync(int medecinId);
    Task<Patient?> GetPatientByIdAsync(int id);
    Task UpdatePatientAsync(Patient patient);
    Task<IEnumerable<ResultatPatient>> GetResultatParPatient(int patientId);
    Task<PatientDet?> GetResultatParPatientDetails(int id);
    Task<bool> BloquerPatientAsync(int patientId);
    Task<bool> DebloquerPatientAsync(int patientId);
}