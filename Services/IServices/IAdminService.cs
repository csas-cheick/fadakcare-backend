using backend.Models;

namespace backend.Services.IServices;

public interface IAdminService
{
    Task<Admin?> GetAdminByIdAsync(int id);
    Task UpdateAdminAsync(Admin admin);
}