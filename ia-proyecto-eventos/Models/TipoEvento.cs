using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace IaProyectoEventos.Models
{
    public class TipoEvento
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool Estado { get; set; }

        [JsonIgnore]
        public ICollection<Evento> Eventos { get; set; } = new List<Evento>();
    }
}
