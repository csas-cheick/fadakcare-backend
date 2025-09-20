using backend.Models;

namespace backend.Services.IServices
{
    public interface IRendezVousService
    {
        Task<RendezVous?> CreerRendezVousAsync(RendezVous rdv);
        Task<IEnumerable<RendezVous>> GetRendezVousParPatientAsync(int patientId);
        Task<IEnumerable<RendezVous>> GetRendezVousParMedecinAsync(int medecinId);
        Task<bool> ModifierEtatAsync(int id, string nouvelEtat);
        Task<RendezVous?> ModifierRendezVousAsync(RendezVous rdv);
        Task<bool> SupprimerRendezVousAsync(int id);
        Task<RendezVous?> GetByIdAsync(int id);
        Task<IEnumerable<RendezVous>> GetAllRendezVousAsync();
        Task<object> GetStatistiquesAsync(string role, int? userId = null);
        Task<RendezVous?> GetProchainRendezVousAsync(int patientId);
    }
}