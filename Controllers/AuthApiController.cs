using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Gimnasio.Models.ViewModels;
using Gimnasio.Services;

namespace Gimnasio.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthApiController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IConfiguration _configuration;

        public AuthApiController(IUsuarioService usuarioService, IConfiguration configuration)
        {
            _usuarioService = usuarioService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginView model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var usuario = await _usuarioService.ObtenerPorDni(model.Dni);
            if (usuario == null || !_usuarioService.VerifPassword(model.Password, usuario.password))
                return Unauthorized("DNI o contraseña incorrectos.");

            if (!usuario.estado)
                return Unauthorized("Su cuenta está desactivada.");

            var token = GenerarToken(usuario);
            return Ok(new { token });
        }

        private string GenerarToken(Gimnasio.Models.Usuario usuario)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? ""));

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.id.ToString()),
                new Claim(ClaimTypes.Name, usuario.nombre),
                new Claim(ClaimTypes.Role, usuario.RolNombre)
            };

            var token = new JwtSecurityToken(
                issuer:   _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims:   claims,
                expires:  DateTime.Now.AddHours(8),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}