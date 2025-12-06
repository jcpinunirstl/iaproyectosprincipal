using IaProyectoEventos.Controllers;
using IaProyectoEventos.Data;
using IaProyectoEventos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace IaProyectoEventos.Tests
{
    public class RegistroAsistenciasControllerTests
    {
        private AppDbContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetRegistroAsistencias_ReturnsAll()
        {
            using var context = CreateInMemoryContext("GetAllRegistrosDb");
            var tipo = new TipoEvento { Id = 1, Nombre = "T", Estado = true };
            context.TipoEventos.Add(tipo);
            var evento = new Evento { Id = 1, Nombre = "E1", TipoEventoId = 1, FechaInicio = DateOnly.FromDateTime(DateTime.UtcNow), FechaFin = DateOnly.FromDateTime(DateTime.UtcNow), HoraInicio = TimeOnly.FromDateTime(DateTime.UtcNow), HoraFin = TimeOnly.FromDateTime(DateTime.UtcNow), Estado = true };
            context.Eventos.Add(evento);
            var persona = new Persona { Id = 1, Nombre = "P1" };
            context.Personas.Add(persona);
            context.RegistroAsistencias.Add(new RegistroAsistencia { Id = 1, EventoId = 1, PersonaId = 1, FechaEntrada = DateTime.UtcNow });
            context.RegistroAsistencias.Add(new RegistroAsistencia { Id = 2, EventoId = 1, PersonaId = 1, FechaEntrada = DateTime.UtcNow });
            await context.SaveChangesAsync();

            var controller = new RegistroAsistenciasController(context);
            var result = await controller.GetRegistroAsistencias();

            Assert.IsType<ActionResult<IEnumerable<RegistroAsistencia>>>(result);
            var items = Assert.IsAssignableFrom<IEnumerable<RegistroAsistencia>>(result.Value!);
            Assert.Equal(2, items.Count());
        }

        [Fact]
        public async Task PostRegistroAsistencia_CreatesAndReturnsCreated()
        {
            using var context = CreateInMemoryContext("PostRegistroDb");
            var tipo = new TipoEvento { Id = 1, Nombre = "T", Estado = true };
            context.TipoEventos.Add(tipo);
            var evento = new Evento { Id = 1, Nombre = "E1", TipoEventoId = 1, FechaInicio = DateOnly.FromDateTime(DateTime.UtcNow), FechaFin = DateOnly.FromDateTime(DateTime.UtcNow), HoraInicio = TimeOnly.FromDateTime(DateTime.UtcNow), HoraFin = TimeOnly.FromDateTime(DateTime.UtcNow), Estado = true };
            context.Eventos.Add(evento);
            var persona = new Persona { Id = 1, Nombre = "P1" };
            context.Personas.Add(persona);
            await context.SaveChangesAsync();

            var controller = new RegistroAsistenciasController(context);
            var registro = new RegistroAsistencia { EventoId = 1, PersonaId = 1, FechaEntrada = DateTime.UtcNow, Observacion = "Ok" };
            var result = await controller.PostRegistroAsistencia(registro);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returned = Assert.IsType<RegistroAsistencia>(created.Value);
            Assert.Equal(1, returned.EventoId);
            Assert.Equal(1, returned.PersonaId);
            Assert.True(returned.Id > 0);
        }

        [Fact]
        public async Task PostRegistroAsistencia_ReturnsBadRequest_WhenMissingReferences()
        {
            using var context = CreateInMemoryContext("PostRegistroBadDb");
            var controller = new RegistroAsistenciasController(context);
            var registro = new RegistroAsistencia { EventoId = 99, PersonaId = 99, FechaEntrada = DateTime.UtcNow };
            var result = await controller.PostRegistroAsistencia(registro);
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task PutRegistroAsistencia_ReturnsBadRequest_WhenEventoOrPersonaMissingOrIdMismatch()
        {
            using var context = CreateInMemoryContext("PutRegistroBadDb");
            var tipo = new TipoEvento { Id = 1, Nombre = "T", Estado = true };
            context.TipoEventos.Add(tipo);
            var evento = new Evento { Id = 1, Nombre = "E1", TipoEventoId = 1, FechaInicio = DateOnly.FromDateTime(DateTime.UtcNow), FechaFin = DateOnly.FromDateTime(DateTime.UtcNow), HoraInicio = TimeOnly.FromDateTime(DateTime.UtcNow), HoraFin = TimeOnly.FromDateTime(DateTime.UtcNow), Estado = true };
            context.Eventos.Add(evento);
            var persona = new Persona { Id = 1, Nombre = "P1" };
            context.Personas.Add(persona);
            await context.SaveChangesAsync();

            var controller = new RegistroAsistenciasController(context);

            // ID mismatch
            var registroMismatch = new RegistroAsistencia { Id = 2, EventoId = 1, PersonaId = 1, FechaEntrada = DateTime.UtcNow };
            var resMismatch = await controller.PutRegistroAsistencia(1, registroMismatch);
            Assert.IsType<BadRequestResult>(resMismatch);

            // Missing Evento
            var registroMissingEvento = new RegistroAsistencia { Id = 1, EventoId = 99, PersonaId = 1, FechaEntrada = DateTime.UtcNow };
            var resMissingEvento = await controller.PutRegistroAsistencia(1, registroMissingEvento);
            Assert.IsType<BadRequestObjectResult>(resMissingEvento);

            // Missing Persona
            var registroMissingPersona = new RegistroAsistencia { Id = 1, EventoId = 1, PersonaId = 99, FechaEntrada = DateTime.UtcNow };
            var resMissingPersona = await controller.PutRegistroAsistencia(1, registroMissingPersona);
            Assert.IsType<BadRequestObjectResult>(resMissingPersona);
        }
    }
}
