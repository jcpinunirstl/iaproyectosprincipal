using IaProyectoEventos.Controllers;
using IaProyectoEventos.Data;
using IaProyectoEventos.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace IaProyectoEventos.Tests
{
    public class SecurityTests
    {
        private AppDbContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new AppDbContext(options);
        }

        private IConfiguration CreateTestConfiguration()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                { "Jwt:Key", "super_secret_test_key_which_is_long_enough_1234567890" },
                { "Jwt:Issuer", "test" },
                { "Jwt:Audience", "test" }
            };
            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        [Fact]
        public async Task Jwt_HeaderAndPayloadContainExpectedValues()
        {
            using var context = CreateInMemoryContext("JwtHeaderPayloadDb");
            var config = CreateTestConfiguration();
            var controller = new UsuariosController(context, config);

            var reg = new UsuariosController.UsuarioRegisterRequest
            {
                Username = "jwtcheck",
                Password = "JwtP@ss123",
            };

            var regRes = await controller.Register(reg);
            Assert.NotNull(regRes.Value);
            var token = regRes.Value!.Token;
            Assert.False(string.IsNullOrEmpty(token));

            var header = DecodeJwtHeader(token);
            Assert.True(header.RootElement.TryGetProperty("alg", out var alg));
            Assert.Equal("HS256", alg.GetString());
            Assert.True(header.RootElement.TryGetProperty("typ", out var typ));
            Assert.Equal("JWT", typ.GetString());

            var payload = DecodeJwtPayload(token);
            Assert.True(payload.RootElement.TryGetProperty("sub", out var sub));
            Assert.Equal(regRes.Value!.UsuarioId.ToString(), sub.GetString());
            Assert.True(payload.RootElement.TryGetProperty("unique_name", out var un));
            Assert.Equal(regRes.Value!.Username, un.GetString());
        }

        [Fact]
        public async Task Authorization_GetEventosByUsuario_RequiresUserClaim()
        {
            using var context = CreateInMemoryContext("AuthRequireDb");
            var tipo = new TipoEvento { Id = 1, Nombre = "T", Estado = true };
            context.TipoEventos.Add(tipo);
            context.Eventos.Add(new Evento { Id = 1, Nombre = "E1", TipoEventoId = 1, UsuarioId = 42, FechaInicio = DateOnly.FromDateTime(DateTime.UtcNow), FechaFin = DateOnly.FromDateTime(DateTime.UtcNow), HoraInicio = TimeOnly.FromDateTime(DateTime.UtcNow), HoraFin = TimeOnly.FromDateTime(DateTime.UtcNow), Estado = true });
            await context.SaveChangesAsync();

            var eventosController = new EventosController(context);

            // No user -> should return Unauthorized
            eventosController.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() } };
            var res = await eventosController.GetEventosByUsuario();
            Assert.IsType<ActionResult<IEnumerable<Evento>>>(res);
            Assert.IsType<UnauthorizedResult>(res.Result);

            // With user claim and matching id -> returns events
            var claims = new[] { new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, "42") };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
            eventosController.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = principal } };

            var res2 = await eventosController.GetEventosByUsuario();
            Assert.IsType<ActionResult<IEnumerable<Evento>>>(res2);
            var items = Assert.IsAssignableFrom<IEnumerable<Evento>>(res2.Value!);
            Assert.Single(items);
        }

        [Fact]
        public async Task Injection_RegisterUsernameWithSqlLikeContent_DoesNotBreak()
        {
            using var context = CreateInMemoryContext("InjectionDb");
            var config = CreateTestConfiguration();
            var controller = new UsuariosController(context, config);

            var dangerous = "evil'; DROP TABLE Usuarios; --";
            var req = new UsuariosController.UsuarioRegisterRequest
            {
                Username = dangerous,
                Password = "SafePass123"
            };

            var res = await controller.Register(req);
            Assert.NotNull(res.Value);

            // Ensure user is stored with exact username and table still accessible
            var saved = await context.Usuarios.FirstOrDefaultAsync(u => u.Username == dangerous);
            Assert.NotNull(saved);
            var count = await context.Usuarios.CountAsync();
            Assert.True(count >= 1);
        }

        [Fact]
        public async Task SqlInjection_LoginBypassAttempt_ReturnsUnauthorized()
        {
            using var context = CreateInMemoryContext("SqlInjectionLoginDb");
            var config = CreateTestConfiguration();
            var controller = new UsuariosController(context, config);

            // register a normal user
            var reg = new UsuariosController.UsuarioRegisterRequest
            {
                Username = "normaluser",
                Password = "NormalP@ss1"
            };
            var regRes = await controller.Register(reg);
            Assert.NotNull(regRes.Value);

            // attempt SQL injection in username field commonly used to bypass authentication
            var loginReq = new UsuariosController.LoginRequest
            {
                Username = "' OR '1'='1",
                Password = "doesntmatter"
            };

            var loginRes = await controller.Login(loginReq);
            Assert.IsType<UnauthorizedObjectResult>(loginRes.Result);
        }

        [Fact]
        public async Task Validation_PostUsuarioMissingPasswordHash_ReturnsBadRequest()
        {
            using var context = CreateInMemoryContext("ValidationDb");
            var config = CreateTestConfiguration();
            var controller = new UsuariosController(context, config);

            var usuario = new Usuario
            {
                Username = "vuser",
                Nombre = "V User",
                PasswordHash = "",
                Email = "v@example.com"
            };

            var res = await controller.PostUsuario(usuario);
            Assert.IsType<BadRequestObjectResult>(res.Result);
        }

        [Fact]
        public async Task Headers_JwtHeaderContainsAlgAndTyp()
        {
            using var context = CreateInMemoryContext("HeadersDb");
            var config = CreateTestConfiguration();
            var controller = new UsuariosController(context, config);

            var reg = new UsuariosController.UsuarioRegisterRequest
            {
                Username = "hdruser",
                Password = "HdrP@ss!"
            };

            var regRes = await controller.Register(reg);
            Assert.NotNull(regRes.Value);
            var token = regRes.Value!.Token;

            var header = DecodeJwtHeader(token);
            Assert.True(header.RootElement.TryGetProperty("alg", out var alg));
            Assert.Equal("HS256", alg.GetString());
            Assert.True(header.RootElement.TryGetProperty("typ", out var typ));
            Assert.Equal("JWT", typ.GetString());
        }

        // helpers
        private static JsonDocument DecodeJwtPayload(string token)
        {
            var parts = token.Split('.');
            if (parts.Length < 2) throw new ArgumentException("Invalid token");
            return DecodeBase64Json(parts[1]);
        }

        private static JsonDocument DecodeJwtHeader(string token)
        {
            var parts = token.Split('.');
            if (parts.Length < 2) throw new ArgumentException("Invalid token");
            return DecodeBase64Json(parts[0]);
        }

        private static JsonDocument DecodeBase64Json(string input)
        {
            var payload = input.Replace('-', '+').Replace('_', '/');
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
                case 0: break;
                default: payload += "="; break;
            }
            var bytes = Convert.FromBase64String(payload);
            var json = Encoding.UTF8.GetString(bytes);
            return JsonDocument.Parse(json);
        }
    }
}
