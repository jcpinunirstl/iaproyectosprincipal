using System.ComponentModel.DataAnnotations;

namespace IaProyectoEventos.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        // Datos personales
        [MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;
        [MaxLength(30)]
        public string Telefono { get; set; } = string.Empty;
        public DateOnly? FechaNacimiento { get; set; }
        public Genero? Genero { get; set; }

        public string PasswordHash { get; set; } = string.Empty;

        // Sal aleatoria usada para derivar el hash
        public string? Salt { get; set; }

        [EmailAddress]
        [MaxLength(200)]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string Rol { get; set; } = "usuario"; // e.g., admin, usuario

        public bool Estado { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? UltimoIngresoUtc { get; set; }
    }
}
