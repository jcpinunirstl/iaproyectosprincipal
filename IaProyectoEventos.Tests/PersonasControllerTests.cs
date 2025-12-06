using IaProyectoEventos.Controllers;
using IaProyectoEventos.Data;
using IaProyectoEventos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace IaProyectoEventos.Tests
{
    public class PersonasControllerTests
    {
        private AppDbContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetPersonas_ReturnsAll()
        {
            using var context = CreateInMemoryContext("GetAllPersonasDb");
            context.Personas.Add(new Persona { Id = 1, Nombre = "Persona 1", Telefono = "123" });
            context.Personas.Add(new Persona { Id = 2, Nombre = "Persona 2", Telefono = "456" });
            await context.SaveChangesAsync();

            var controller = new PersonasController(context);
            var result = await controller.GetPersonas();

            Assert.IsType<ActionResult<IEnumerable<Persona>>>(result);
            var items = Assert.IsAssignableFrom<IEnumerable<Persona>>(result.Value!);
            Assert.Equal(2, items.Count());
        }

        [Fact]
        public async Task GetPersona_ReturnsNotFound_WhenMissing()
        {
            using var context = CreateInMemoryContext("GetMissingPersonaDb");
            var controller = new PersonasController(context);
            var result = await controller.GetPersona(99);

            Assert.IsType<ActionResult<Persona>>(result);
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PostPersona_CreatesAndReturnsCreated()
        {
            using var context = CreateInMemoryContext("PostPersonaDb");
            var controller = new PersonasController(context);

            var persona = new Persona
            {
                Nombre = "Nueva Persona",
                Telefono = "000",
                Estado = true
            };

            var result = await controller.PostPersona(persona);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returned = Assert.IsType<Persona>(created.Value);
            Assert.Equal("Nueva Persona", returned.Nombre);
            Assert.True(returned.Id > 0);
        }

        [Fact]
        public async Task DeletePersona_ReturnsBadRequest_WhenHasAsistencias()
        {
            using var context = CreateInMemoryContext("DeletePersonaHasAsistenciasDb");
            // create persona
            var persona = new Persona { Id = 1, Nombre = "P1" };
            context.Personas.Add(persona);
            // create tipoEvento and evento required by RegistroAsistencia
            var tipo = new TipoEvento { Id = 1, Nombre = "T", Estado = true };
            context.TipoEventos.Add(tipo);
            var evento = new Evento { Id = 1, Nombre = "E1", TipoEventoId = 1, FechaInicio = System.DateOnly.FromDateTime(System.DateTime.UtcNow), FechaFin = System.DateOnly.FromDateTime(System.DateTime.UtcNow), HoraInicio = System.TimeOnly.FromDateTime(System.DateTime.UtcNow), HoraFin = System.TimeOnly.FromDateTime(System.DateTime.UtcNow), Estado = true };
            context.Eventos.Add(evento);
            // add registro referencing persona
            var registro = new RegistroAsistencia { Id = 1, EventoId = 1, PersonaId = 1, FechaEntrada = System.DateTime.UtcNow };
            context.RegistroAsistencias.Add(registro);
            await context.SaveChangesAsync();

            var controller = new PersonasController(context);
            var result = await controller.DeletePersona(1);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task DeletePersona_ReturnsNotFound_WhenMissing()
        {
            using var context = CreateInMemoryContext("DeletePersonaNotFoundDb");
            var controller = new PersonasController(context);
            var result = await controller.DeletePersona(999);
            Assert.IsType<NotFoundResult>(result);
        }
    }

    public class TipoEventosControllerErrorTests
    {
        private AppDbContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetTipoEvento_ReturnsNotFound_WhenMissing()
        {
            using var context = CreateInMemoryContext("TipoEventoMissingDb");
            var controller = new IaProyectoEventos.Controllers.TipoEventosController(context);
            var result = await controller.GetTipoEvento(1234);
            Assert.IsType<ActionResult<IaProyectoEventos.Models.TipoEvento>>(result);
            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}
