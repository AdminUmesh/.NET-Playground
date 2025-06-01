using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using DotNet_Playground.Models;

namespace DotNet_Playground.Controllers
{
    public class JWTViewController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

    }

    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            if (model.Username == "a" && model.Password == "a")
            {
                var claims = new[]
                {
                   new Claim(ClaimTypes.Name, model.Username),
                   new Claim(ClaimTypes.Role, model.Role)  // 👈 Add role from model
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MySuperSecretKey12345MySuperSecretKey12345MySuperSecretKey12345"));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken("myapp", "myapp", claims,
                    expires: DateTime.Now.AddMinutes(30), signingCredentials: creds);

                return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
            }

            return Unauthorized();
        }
    }


    [Authorize]
    [ApiController]
    [Route("api/check")]
    public class JWTController : ControllerBase
    {
        private readonly AppDbContext _context;
        public JWTController(AppDbContext context) => _context = context;


        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public IActionResult GetAdminData()
        {
            return Ok("Only Admins can see this.");
        }


        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("adminManager")]
        public IActionResult GetAdminMannagerData()
        {
            return Ok("Only Admins and Manager can see this.");
        }


        [Authorize(Roles = "Manager")]
        [HttpGet("manager")]
        public IActionResult GetMannagerData()
        {
            return Ok("Only Manager can see this.");
        }

    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } 
    }

}
