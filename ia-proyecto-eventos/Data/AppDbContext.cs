using Microsoft.EntityFrameworkCore;
using IaProyectoEventos.Models;

namespace IaProyectoEventos.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Evento> Eventos { get; set; }
        public DbSet<TipoEvento> TipoEventos { get; set; }
        public DbSet<Persona> Personas { get; set; }
        public DbSet<RegistroAsistencia> RegistroAsistencias { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar relación Evento -> Usuario
            modelBuilder.Entity<Evento>()
                .HasOne(e => e.Usuario)
                .WithMany()
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
