using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
   {
       [HttpGet]
       [AllowAnonymous] // pas besoin d'auth
       public IActionResult Ping()
       {
            return Ok("Hello from backend!");
       }
    }  
}