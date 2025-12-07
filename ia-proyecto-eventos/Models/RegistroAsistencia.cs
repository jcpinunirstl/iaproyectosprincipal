using System;

namespace IaProyectoEventos.Models
{
    public class RegistroAsistencia
    {
        public int Id { get; set; }

        // Campos solicitados
        public DateTime FechaEntrada { get; set; }
        public string Observacion { get; set; } = string.Empty;

        // Relaciones
        public int EventoId { get; set; }
        public Evento? Evento { get; set; }

        public int PersonaId { get; set; }
        public Persona? Persona { get; set; }
    }
}