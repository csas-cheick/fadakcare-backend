using Microsoft.AspNetCore.Mvc;
using backend.Services.IServices;
using Microsoft.AspNetCore.Authorization;

[Route("api/dashboard")]
[ApiController]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("admin")]
    [Authorize(Roles="admin")]
    public async Task<IActionResult> GetAdminDashboard()
    {
        var result = await _dashboardService.GetDashboardAdminAsync();
        return Ok(result);
    }

    [HttpGet("medecin/{id}")]
    [Authorize(Roles="doctor")]
    public async Task<IActionResult> GetMedecinDashboard(int id)
    {
        var result = await _dashboardService.GetDashboardMedecinAsync(id);
        return Ok(result);
    }

    [HttpGet("patient/{id}")]
    [Authorize(Roles="patient")]
    public async Task<IActionResult> GetPatientDashboard(int id)
    {

        var result = await _dashboardService.GetDashboardPatientAsync(id);
        return Ok(result);
    }
}
