namespace backend.Services.IServices;
 public interface IDashboardService
    {
        Task<object> GetDashboardAdminAsync();
        Task<object> GetDashboardMedecinAsync(int medecinId);
        Task<object> GetDashboardPatientAsync(int patientId);
    }