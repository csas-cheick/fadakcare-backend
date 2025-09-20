using backend.Models;

namespace backend.Services.IServices;

public interface IConseilService
{
    Task<IEnumerable<Conseil>> GetConseilsPourPatientAsync(int patientId);
    Task<IEnumerable<Conseil>> GetConseilsDuMedecinAsync(int medecinId);
    Task<IEnumerable<Conseil>> GetTousLesConseilsAsync();
    Task<bool> EnvoyerConseilAsync(Conseil conseil);
    Task<bool> ModifierConseilAsync(int id, string nouveauMessage);
}