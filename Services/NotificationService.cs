using backend.Data;
using backend.Dtos;
using backend.Models;
using backend.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;

        public NotificationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<NotificationDto>> GetByUserAsync(int utilisateurId, int take = 20, int skip = 0)
        {
            var list = await _context.Notifications
                .Where(n => n.UtilisateurId == utilisateurId)
                .OrderByDescending(n => n.DateNotif)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return list.Select(n => new NotificationDto
            {
                Id = n.Id,
                Type = n.Type,
                Message = n.Message,
                Lu = n.Lu,
                UtilisateurId = n.UtilisateurId,
                DateNotif = n.DateNotif,
                TimeAgo = GetTimeAgo(n.DateNotif)
            });
        }

        public async Task<int> CountUnreadAsync(int utilisateurId)
        {
            return await _context.Notifications.CountAsync(n => n.UtilisateurId == utilisateurId && !n.Lu);
        }

        public async Task<Notification> CreateAsync(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task MarquerCommeLuAsync(int notificationId, int utilisateurId)
        {
            var notif = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId && n.UtilisateurId == utilisateurId);
            if (notif != null && !notif.Lu)
            {
                notif.Lu = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarquerToutCommeLuAsync(int utilisateurId)
        {
            var notifs = await _context.Notifications.Where(n => n.UtilisateurId == utilisateurId && !n.Lu).ToListAsync();
            if (notifs.Count > 0)
            {
                foreach (var n in notifs) n.Lu = true;
                await _context.SaveChangesAsync();
            }
        }

        private static string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;
            
            if (timeSpan.TotalMinutes < 1)
                return "À l'instant";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} min";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours}h";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays}j";
            return dateTime.ToString("dd/MM/yyyy");
        }
    }
}
