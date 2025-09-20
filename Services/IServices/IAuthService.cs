using backend.Dtos.compte;
using backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace backend.Services.IServices;

public interface IAuthService
{
    Task<IActionResult> Login(LoginRequest loginRequest);
    Task<IActionResult> ForgotPassword(ForgotPassword Fpassword);
    Task<IActionResult> VerifyCode(VerifyCode request);
    Task<IActionResult> ResetPassword(ResetPassword request);
    Task<IActionResult> Register(Utilisateur utilisateur);
    Task<IActionResult> RefreshToken(RefreshRequest request);
    Task<IActionResult> GoogleLogin(GoogleLoginRequest request);
    Task<IActionResult> Logout(RefreshRequest request);
}