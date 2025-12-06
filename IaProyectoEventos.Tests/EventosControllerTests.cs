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
    public class EventosControllerTests
    {
        private AppDbContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetEventos_ReturnsAll()
        {
            using var context = CreateInMemoryContext("GetAllEventosDb");
            context.TipoEventos.Add(new TipoEvento { Id = 1, Nombre = "Concierto", Estado = true });

            context.Eventos.Add(new Evento
            {
                Id = 1,
                Nombre = "Evento 1",
                Descripcion = "Desc 1",
                Direccion = "Dir 1",
                Costo = 0m,
                FechaInicio = DateOnly.FromDateTime(DateTime.UtcNow),
                FechaFin = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                HoraInicio = TimeOnly.FromDateTime(DateTime.UtcNow),
                HoraFin = TimeOnly.FromDateTime(DateTime.UtcNow.AddHours(1)),
                TipoEventoId = 1,
                Estado = true
            });

            context.Eventos.Add(new Evento
            {
                Id = 2,
                Nombre = "Evento 2",
                Descripcion = "Desc 2",
                Direccion = "Dir 2",
                Costo = 5m,
                FechaInicio = DateOnly.FromDateTime(DateTime.UtcNow),
                FechaFin = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)),
                HoraInicio = TimeOnly.FromDateTime(DateTime.UtcNow),
                HoraFin = TimeOnly.FromDateTime(DateTime.UtcNow.AddHours(2)),
                TipoEventoId = 1,
                Estado = true
            });

            await context.SaveChangesAsync();

            var controller = new EventosController(context);
            var result = await controller.GetEventos();

            Assert.IsType<ActionResult<IEnumerable<Evento>>>(result);
            var items = Assert.IsAssignableFrom<IEnumerable<Evento>>(result.Value!);
            Assert.Equal(2, items.Count());
        }

        [Fact]
        public async Task GetEvento_ReturnsNotFound_WhenMissing()
        {
            using var context = CreateInMemoryContext("GetMissingEventoDb");
            var controller = new EventosController(context);
            var result = await controller.GetEvento(99);

            Assert.IsType<ActionResult<Evento>>(result);
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PostEvento_CreatesAndReturnsCreated()
        {
            using var context = CreateInMemoryContext("PostEventoDb");
            // Ensure TipoEvento exists because PostEvento validates its existence
            context.TipoEventos.Add(new TipoEvento { Id = 1, Nombre = "Taller", Estado = true });
            await context.SaveChangesAsync();

            var controller = new EventosController(context);

            var evento = new Evento
            {
                Nombre = "Nuevo Evento",
                Descripcion = "Descripcion",
                Direccion = "Lugar",
                Costo = 10m,
                FechaInicio = DateOnly.FromDateTime(DateTime.UtcNow),
                FechaFin = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                HoraInicio = TimeOnly.FromDateTime(DateTime.UtcNow),
                HoraFin = TimeOnly.FromDateTime(DateTime.UtcNow.AddHours(1)),
                TipoEventoId = 1,
                Estado = true
            };

            var result = await controller.PostEvento(evento);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returned = Assert.IsType<Evento>(created.Value);
            Assert.Equal("Nuevo Evento", returned.Nombre);
            Assert.True(returned.Id > 0);
        }

        [Fact]
        public async Task PostEvento_ReturnsBadRequest_WhenTipoMissing()
        {
            using var context = CreateInMemoryContext("PostEventoNoTipoDb");
            // Do NOT add TipoEvento
            var controller = new EventosController(context);

            var evento = new Evento
            {
                Nombre = "Evento Fail",
                TipoEventoId = 999,
                FechaInicio = DateOnly.FromDateTime(DateTime.UtcNow),
                FechaFin = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                HoraInicio = TimeOnly.FromDateTime(DateTime.UtcNow),
                HoraFin = TimeOnly.FromDateTime(DateTime.UtcNow.AddHours(1)),
                Estado = true
            };

            var result = await controller.PostEvento(evento);
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task PostEvento_ReturnsBadRequest_WhenUsuarioMissing()
        {
            using var context = CreateInMemoryContext("PostEventoNoUsuarioDb");
            // Ensure TipoEvento exists
            context.TipoEventos.Add(new TipoEvento { Id = 1, Nombre = "Taller", Estado = true });
            await context.SaveChangesAsync();

            var controller = new EventosController(context);

            var evento = new Evento
            {
                Nombre = "Evento Fail User",
                TipoEventoId = 1,
                UsuarioId = 999, // non-existing user
                FechaInicio = DateOnly.FromDateTime(DateTime.UtcNow),
                FechaFin = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                HoraInicio = TimeOnly.FromDateTime(DateTime.UtcNow),
                HoraFin = TimeOnly.FromDateTime(DateTime.UtcNow.AddHours(1)),
                Estado = true
            };

            var result = await controller.PostEvento(evento);
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }
    }
}
