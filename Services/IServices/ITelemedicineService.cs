using backend.Dtos.Telemedicine;

namespace backend.Services.IServices
{
     public interface ITelemedicineService
    {
        Task<TelemedicineResponseDto?> CreateSessionAsync(CreateTelemedicineDto dto, int createurId);
        Task<TelemedicineResponseDto?> UpdateSessionAsync(int id, UpdateTelemedicineDto dto, int userId);
        Task<bool> DeleteSessionAsync(int id, int userId);
        Task<bool> DeleteSessionAsAdminAsync(int id);
        Task<List<TelemedicineResponseDto>> GetSessionsByUserAsync(int userId, string? etat = null);
        Task<List<TelemedicineResponseDto>> GetAllSessionsAsync(string? etat = null);
        Task<TelemedicineResponseDto?> GetSessionByIdAsync(int id);
        Task<bool> JoinSessionAsync(int sessionId, int userId);
        Task<bool> LeaveSessionAsync(int sessionId, int userId);
        Task<bool> AddParticipantAsync(int sessionId, int participantId, int createurId);
        Task<bool> RemoveParticipantAsync(int sessionId, int participantId, int createurId);
        Task<bool> UpdateSessionStateAsync(int sessionId, string newState);
        Task<List<ParticipantResponseDto>> GetSessionParticipantsAsync(int sessionId);
        Task<List<object>> GetAvailableParticipantsAsync(string sessionType, int medecinId);
    }
}