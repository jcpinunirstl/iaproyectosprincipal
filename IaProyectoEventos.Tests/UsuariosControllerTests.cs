using IaProyectoEventos.Controllers;
using IaProyectoEventos.Data;
using IaProyectoEventos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace IaProyectoEventos.Tests
{
    public class UsuariosControllerTests
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
        public async Task GetUsuarios_ReturnsAll()
        {
            using var context = CreateInMemoryContext("GetAllUsuariosDb");
            context.Usuarios.Add(new Usuario { Id = 1, Username = "user1", Nombre = "User 1" });
            context.Usuarios.Add(new Usuario { Id = 2, Username = "user2", Nombre = "User 2" });
            await context.SaveChangesAsync();

            var config = CreateTestConfiguration();
            var controller = new UsuariosController(context, config);
            var result = await controller.GetUsuarios();

            Assert.IsType<ActionResult<IEnumerable<Usuario>>>(result);
            var items = Assert.IsAssignableFrom<IEnumerable<Usuario>>(result.Value!);
            Assert.Equal(2, items.Count());
        }

        [Fact]
        public async Task GetUsuario_ReturnsNotFound_WhenMissing()
        {
            using var context = CreateInMemoryContext("GetMissingUsuarioDb");
            var config = CreateTestConfiguration();
            var controller = new UsuariosController(context, config);
            var result = await controller.GetUsuario(99);

            Assert.IsType<ActionResult<Usuario>>(result);
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PostUsuario_CreatesAndReturnsCreated()
        {
            using var context = CreateInMemoryContext("PostUsuarioDb");
            var config = CreateTestConfiguration();
            var controller = new UsuariosController(context, config);

            var usuario = new Usuario
            {
                Username = "nuevo",
                Nombre = "Nuevo Nombre",
                PasswordHash = Convert.ToBase64String(Encoding.UTF8.GetBytes("dummyhash")),
                Email = "test@example.com",
                Rol = "usuario",
                Estado = true
            };

            var result = await controller.PostUsuario(usuario);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returned = Assert.IsType<Usuario>(created.Value);
            Assert.Equal("nuevo", returned.Username);
            Assert.True(returned.Id > 0);
        }

        [Fact]
        public async Task RegisterAndLogin_Workflow()
        {
            using var context = CreateInMemoryContext("RegisterLoginDb");
            var config = CreateTestConfiguration();
            var controller = new UsuariosController(context, config);

            var register = new UsuariosController.UsuarioRegisterRequest
            {
                Username = "testuser",
                Password = "MyP@ssw0rd!",
                Nombre = "Test User",
                Email = "testuser@example.com"
            };

            var registerResult = await controller.Register(register);
            Assert.NotNull(registerResult.Value);
            var loginResp = registerResult.Value!;
            Assert.Equal(register.Username.Trim().ToLower(), loginResp.Username);
            Assert.True(loginResp.UsuarioId > 0);
            Assert.False(string.IsNullOrEmpty(loginResp.Token));

            // Now attempt login
            var loginRequest = new UsuariosController.LoginRequest
            {
                Username = "testuser",
                Password = "MyP@ssw0rd!"
            };

            var loginResult = await controller.Login(loginRequest);
            Assert.NotNull(loginResult.Value);
            var loginResp2 = loginResult.Value!;
            Assert.Equal(loginResp.UsuarioId, loginResp2.UsuarioId);
            Assert.False(string.IsNullOrEmpty(loginResp2.Token));
        }

        [Fact]
        public async Task Register_Duplicate_ReturnsConflict()
        {
            using var context = CreateInMemoryContext("RegisterDuplicateDb");
            var config = CreateTestConfiguration();
            var controller = new UsuariosController(context, config);

            var req = new UsuariosController.UsuarioRegisterRequest
            {
                Username = "duplicate",
                Password = "pass"
            };

            var first = await controller.Register(req);
            Assert.NotNull(first.Value);

            var second = await controller.Register(req);
            Assert.IsType<ConflictObjectResult>(second.Result);
        }
    }
}
