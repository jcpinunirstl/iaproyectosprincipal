using Microsoft.EntityFrameworkCore;

namespace IaProyectoEventos.Models
{
    public class Evento
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        [Precision(18, 2)]
        public decimal Costo { get; set; }
        public DateOnly FechaInicio { get; set; }
        public DateOnly FechaFin { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }
        public int TipoEventoId { get; set; }
        public TipoEvento? TipoEvento { get; set; }
        public int? UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }
        public bool Estado { get; set; }
    }
}
