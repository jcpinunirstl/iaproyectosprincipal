using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IaProyectoEventos.Data;
using IaProyectoEventos.Models;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace IaProyectoEventos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EventosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Evento>>> GetEventos()
        {
            return await _context.Eventos
                .Include(e => e.TipoEvento)
                .Include(e => e.Usuario)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Evento>> GetEvento(int id)
        {
            var evento = await _context.Eventos
                .Include(e => e.TipoEvento)
                .Include(e => e.Usuario)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (evento == null) return NotFound();
            return evento;
        }

        [Authorize]
        [HttpGet("usuario")]
        public async Task<ActionResult<IEnumerable<Evento>>> GetEventosByUsuario()
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub) ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            if (!int.TryParse(userIdClaim.Value, out var usuarioId)) return Unauthorized();

            return await _context.Eventos
                .Include(e => e.TipoEvento)
                .Include(e => e.Usuario)
                .Where(e => e.UsuarioId == usuarioId)
                .OrderByDescending(e => e.FechaInicio)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Evento>> PostEvento(Evento evento)
        {
            var existsTipo = await _context.TipoEventos.AnyAsync(te => te.Id == evento.TipoEventoId);
            if (!existsTipo) return BadRequest($"TipoEventoId {evento.TipoEventoId} no existe");

            if (evento.UsuarioId.HasValue)
            {
                var existsUsuario = await _context.Usuarios.AnyAsync(u => u.Id == evento.UsuarioId);
                if (!existsUsuario) return BadRequest($"UsuarioId {evento.UsuarioId} no existe");
            }

            _context.Eventos.Add(evento);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetEvento), new { id = evento.Id }, evento);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutEvento(int id, Evento evento)
        {
            if (id != evento.Id) return BadRequest();

            var existsTipo = await _context.TipoEventos.AnyAsync(te => te.Id == evento.TipoEventoId);
            if (!existsTipo) return BadRequest($"TipoEventoId {evento.TipoEventoId} no existe");

            if (evento.UsuarioId.HasValue)
            {
                var existsUsuario = await _context.Usuarios.AnyAsync(u => u.Id == evento.UsuarioId);
                if (!existsUsuario) return BadRequest($"UsuarioId {evento.UsuarioId} no existe");
            }

            _context.Entry(evento).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvento(int id)
        {
            var evento = await _context.Eventos.FindAsync(id);
            if (evento == null) return NotFound();
            _context.Eventos.Remove(evento);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
