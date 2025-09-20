using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Dtos;
using backend.Models;

namespace backend.Services.IServices
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationDto>> GetByUserAsync(int utilisateurId, int take = 20, int skip = 0);
        Task<int> CountUnreadAsync(int utilisateurId);
        Task<Notification> CreateAsync(Notification notification);
        Task MarquerCommeLuAsync(int notificationId, int utilisateurId);
        Task MarquerToutCommeLuAsync(int utilisateurId);
    }
}
