using Microsoft.AspNetCore.Mvc;
using IaProyectoEventos.Data;
using IaProyectoEventos.Models;
using Microsoft.EntityFrameworkCore;

namespace IaProyectoEventos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TipoEventosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TipoEventosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TipoEvento>>> GetTipoEventos()
        {
            return await _context.TipoEventos.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TipoEvento>> GetTipoEvento(int id)
        {
            var tipoEvento = await _context.TipoEventos.FindAsync(id);
            if (tipoEvento == null) return NotFound();
            return tipoEvento;
        }

        [HttpPost]
        public async Task<ActionResult<TipoEvento>> PostTipoEvento(TipoEvento tipoEvento)
        {
            _context.TipoEventos.Add(tipoEvento);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTipoEvento), new { id = tipoEvento.Id }, tipoEvento);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTipoEvento(int id, TipoEvento tipoEvento)
        {
            if (id != tipoEvento.Id) return BadRequest();
            _context.Entry(tipoEvento).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTipoEvento(int id)
        {
            var tipoEvento = await _context.TipoEventos
                .Include(te => te.Eventos)
                .FirstOrDefaultAsync(te => te.Id == id);
            if (tipoEvento == null) return NotFound();

            if (tipoEvento.Eventos.Any())
            {
                return BadRequest("No se puede eliminar el TipoEvento porque tiene eventos asociados.");
            }

            _context.TipoEventos.Remove(tipoEvento);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
