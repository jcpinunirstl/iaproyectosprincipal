using Microsoft.AspNetCore.Mvc;
using IaProyectoEventos.Data;
using IaProyectoEventos.Models;
using Microsoft.EntityFrameworkCore;

namespace IaProyectoEventos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PersonasController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Persona>>> GetPersonas()
        {
            return await _context.Personas.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Persona>> GetPersona(int id)
        {
            var persona = await _context.Personas.FindAsync(id);
            if (persona == null) return NotFound();
            return persona;
        }

        [HttpPost]
        public async Task<ActionResult<Persona>> PostPersona(Persona persona)
        {
            _context.Personas.Add(persona);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPersona), new { id = persona.Id }, persona);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutPersona(int id, Persona persona)
        {
            if (id != persona.Id) return BadRequest();
            _context.Entry(persona).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePersona(int id)
        {
            var persona = await _context.Personas.FindAsync(id);
            if (persona == null) return NotFound();

            var hasAsistencias = await _context.RegistroAsistencias.AnyAsync(ra => ra.PersonaId == id);
            if (hasAsistencias)
            {
                return BadRequest("No se puede eliminar la persona porque tiene registros de asistencia asociados.");
            }

            _context.Personas.Remove(persona);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}