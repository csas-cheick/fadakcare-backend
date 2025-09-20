using backend.Dtos.compte;
using backend.Models;
using backend.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public Task<IActionResult> Login([FromBody] LoginRequest loginRequest) =>
            _authService.Login(loginRequest);

        [HttpPost("forgotPassword")]
        [AllowAnonymous]
        public Task<IActionResult> ForgotPassword([FromBody] ForgotPassword Fpassword) =>
            _authService.ForgotPassword(Fpassword);

        [HttpPost("verifyCode")]
        [AllowAnonymous]
        public Task<IActionResult> VerifyCode([FromBody] VerifyCode request) =>
            _authService.VerifyCode(request);

        [HttpPost("resetPassword")]
        [AllowAnonymous]
        public Task<IActionResult> ResetPassword([FromBody] ResetPassword request) =>
            _authService.ResetPassword(request);

        [HttpPost("register")]
        [AllowAnonymous]
        public Task<IActionResult> Register([FromBody] Utilisateur utilisateur) =>
            _authService.Register(utilisateur);

        [HttpPost("refresh")]
        [AllowAnonymous]
        public Task<IActionResult> Refresh([FromBody] RefreshRequest request) =>
            _authService.RefreshToken(request);

        [HttpPost("google-login")]
        [AllowAnonymous]
        public Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request) =>
            _authService.GoogleLogin(request);

        [HttpPost("logout")]
        [AllowAnonymous]
        public Task<IActionResult> Logout([FromBody] RefreshRequest request) =>
            _authService.Logout(request);
    }
}
