using Microsoft.AspNetCore.Mvc;
using IaProyectoEventos.Data;
using IaProyectoEventos.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace IaProyectoEventos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public UsuariosController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // ======================================
        // 🔐 Hash seguro con PBKDF2
        // ======================================
        private static (string hash, string salt) HashPassword(string password)
        {
            var saltBytes = RandomNumberGenerator.GetBytes(16);
            var salt = Convert.ToBase64String(saltBytes);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 100_000, HashAlgorithmName.SHA256);
            var hash = Convert.ToBase64String(pbkdf2.GetBytes(32));
            return (hash, salt);
        }

        private static string HashWithSalt(string password, string salt)
        {
            var saltBytes = Convert.FromBase64String(salt);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 100_000, HashAlgorithmName.SHA256);
            return Convert.ToBase64String(pbkdf2.GetBytes(32));
        }

        // ======================================
        // 🔑 Generar JWT
        // ======================================
        private string GenerateJwt(Usuario user)
        {
            var jwtSection = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(ClaimTypes.Role, user.Rol)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(4),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ======================================
        // 📋 GET: api/Usuarios
        // ======================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            return await _context.Usuarios.AsNoTracking().ToListAsync();
        }

        // ======================================
        // 📋 GET: api/Usuarios/{id}
        // ======================================
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();
            return usuario;
        }

        // ======================================
        // ➕ POST: api/Usuarios
        // ======================================
        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario.PasswordHash))
            {
                return BadRequest("PasswordHash requerido. Use el endpoint /register para registrar con contraseña.");
            }

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id }, usuario);
        }

        // ======================================
        // 🧾 Registro (usa modelo Usuario + Password)
        // ======================================
        [HttpPost("register")]
        public async Task<ActionResult<LoginResponse>> Register([FromBody] UsuarioRegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Username y Password son requeridos");

            var normalizedUsername = request.Username.Trim().ToLower();
            var exists = await _context.Usuarios.AnyAsync(u => u.Username.ToLower() == normalizedUsername);
            if (exists) return Conflict("El nombre de usuario ya existe");

            var (hash, salt) = HashPassword(request.Password);

            var user = new Usuario
            {
                Username = normalizedUsername,
                Nombre = request.Nombre,
                Telefono = request.Telefono,
                FechaNacimiento = request.FechaNacimiento,
                Genero = request.Genero,
                Email = request.Email,
                PasswordHash = hash,
                Salt = salt,
                Rol = string.IsNullOrWhiteSpace(request.Rol) ? "usuario" : request.Rol,
                Estado = true,
                FechaCreacion = DateTime.UtcNow
            };

            _context.Usuarios.Add(user);
            await _context.SaveChangesAsync();

            var token = GenerateJwt(user);

            return new LoginResponse
            {
                UsuarioId = user.Id,
                Username = user.Username,
                Rol = user.Rol,
                Token = token
            };
        }

        // ======================================
        // 🔓 Login con emisión de JWT
        // ======================================
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            var normalizedUsername = request.Username.Trim().ToLower();
            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Username.ToLower() == normalizedUsername && u.Estado);
            if (user == null) return Unauthorized("Credenciales inválidas");

            if (string.IsNullOrEmpty(user.Salt)) return Unauthorized("Usuario no tiene salt configurado");

            var hash = HashWithSalt(request.Password, user.Salt);
            try
            {
                if (!CryptographicOperations.FixedTimeEquals(
                    Convert.FromBase64String(user.PasswordHash),
                    Convert.FromBase64String(hash)))
                {
                    return Unauthorized("Credenciales inválidas");
                }
            }
            catch
            {
                return Unauthorized("Error al validar credenciales");
            }

            user.UltimoIngresoUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var token = GenerateJwt(user);
            return new LoginResponse
            {
                UsuarioId = user.Id,
                Username = user.Username,
                Rol = user.Rol,
                Token = token
            };
        }

        // ======================================
        // ✏️ PUT: api/Usuarios/{id}
        // ======================================
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, Usuario usuario)
        {
            if (id != usuario.Id) return BadRequest("ID no coincide");

            _context.Entry(usuario).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ======================================
        // ❌ DELETE: api/Usuarios/{id}
        // ======================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ======================================
        // 📦 Clases auxiliares
        // ======================================
        public class UsuarioRegisterRequest : Usuario
        {
            public string Password { get; set; } = string.Empty;
        }

        public class LoginRequest
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class LoginResponse
        {
            public int UsuarioId { get; set; }
            public string Username { get; set; } = string.Empty;
            public string Rol { get; set; } = string.Empty;
            public string Token { get; set; } = string.Empty;
        }
    }
}
