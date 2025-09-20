using backend.Data;
using backend.Models;
using backend.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _context;

        public AdminService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Admin?> GetAdminByIdAsync(int id)
        {
            return await _context.Admins
               .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task UpdateAdminAsync(Admin admin)
        {
            _context.Admins.Update(admin);
            await _context.SaveChangesAsync();
        }

    }
}
