using System;

namespace backend.Dtos.compte
{
    public class ResetPassword
    {
        public string? Email { get; set; }
        public string? Code { get; set; }
        public string? newPassword { get; set; }
    }
}