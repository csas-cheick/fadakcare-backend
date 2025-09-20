using System;

namespace backend.Dtos
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool Lu { get; set; }
        public int UtilisateurId { get; set; }
        public DateTime DateNotif { get; set; }
        public string TimeAgo { get; set; } = string.Empty;
    }
}
