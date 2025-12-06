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
    public class TipoEventosControllerTests
    {
        private AppDbContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetTipoEventos_ReturnsAll()
        {
            using var context = CreateInMemoryContext("GetAllDb");
            context.TipoEventos.Add(new TipoEvento { Id = 1, Nombre = "Concierto", Estado = true });
            context.TipoEventos.Add(new TipoEvento { Id = 2, Nombre = "Conferencia", Estado = true });
            await context.SaveChangesAsync();

            var controller = new TipoEventosController(context);
            var result = await controller.GetTipoEventos();

            Assert.IsType<ActionResult<IEnumerable<TipoEvento>>>(result);
            var items = Assert.IsAssignableFrom<IEnumerable<TipoEvento>>(result.Value!);
            Assert.Equal(2, items.Count());
        }

        [Fact]
        public async Task GetTipoEvento_ReturnsNotFound_WhenMissing()
        {
            using var context = CreateInMemoryContext("GetMissingDb");
            var controller = new TipoEventosController(context);
            var result = await controller.GetTipoEvento(99);

            Assert.IsType<ActionResult<TipoEvento>>(result);
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PostTipoEvento_CreatesAndReturnsCreated()
        {
            using var context = CreateInMemoryContext("PostDb");
            var controller = new TipoEventosController(context);

            var tipo = new TipoEvento { Nombre = "Taller", Estado = true };
            var result = await controller.PostTipoEvento(tipo);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returned = Assert.IsType<TipoEvento>(created.Value);
            Assert.Equal("Taller", returned.Nombre);
            Assert.True(returned.Id > 0);
        }
    }
}
