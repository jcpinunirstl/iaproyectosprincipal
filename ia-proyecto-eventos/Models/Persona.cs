namespace IaProyectoEventos.Models
{
    public class Persona
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public DateOnly FechaNacimiento { get; set; }
        public Genero Genero { get; set; }
        public bool Estado { get; set; }
    }
}
