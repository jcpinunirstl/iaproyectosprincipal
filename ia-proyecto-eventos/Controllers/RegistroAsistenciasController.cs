using Microsoft.AspNetCore.Mvc;
using IaProyectoEventos.Data;
using IaProyectoEventos.Models;
using Microsoft.EntityFrameworkCore;

namespace IaProyectoEventos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistroAsistenciasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RegistroAsistenciasController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RegistroAsistencia>>> GetRegistroAsistencias()
        {
            return await _context.RegistroAsistencias
                .Include(ra => ra.Evento)
                .Include(ra => ra.Persona)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RegistroAsistencia>> GetRegistroAsistencia(int id)
        {
            var registro = await _context.RegistroAsistencias
                .Include(ra => ra.Evento)
                .Include(ra => ra.Persona)
                .FirstOrDefaultAsync(ra => ra.Id == id);
            if (registro == null) return NotFound();
            return registro;
        }

        [HttpPost]
        public async Task<ActionResult<RegistroAsistencia>> PostRegistroAsistencia(RegistroAsistencia registro)
        {
            var existsEvento = await _context.Eventos.AnyAsync(e => e.Id == registro.EventoId);
            if (!existsEvento) return BadRequest($"EventoId {registro.EventoId} no existe");

            var existsPersona = await _context.Personas.AnyAsync(p => p.Id == registro.PersonaId);
            if (!existsPersona) return BadRequest($"PersonaId {registro.PersonaId} no existe");

            _context.RegistroAsistencias.Add(registro);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetRegistroAsistencia), new { id = registro.Id }, registro);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutRegistroAsistencia(int id, RegistroAsistencia registro)
        {
            if (id != registro.Id) return BadRequest();

            var existsEvento = await _context.Eventos.AnyAsync(e => e.Id == registro.EventoId);
            if (!existsEvento) return BadRequest($"EventoId {registro.EventoId} no existe");

            var existsPersona = await _context.Personas.AnyAsync(p => p.Id == registro.PersonaId);
            if (!existsPersona) return BadRequest($"PersonaId {registro.PersonaId} no existe");

            _context.Entry(registro).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRegistroAsistencia(int id)
        {
            var registro = await _context.RegistroAsistencias.FindAsync(id);
            if (registro == null) return NotFound();
            _context.RegistroAsistencias.Remove(registro);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}