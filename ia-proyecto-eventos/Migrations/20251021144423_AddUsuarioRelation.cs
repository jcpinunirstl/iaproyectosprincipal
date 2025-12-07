using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IaProyectoEventos.Migrations
{
    /// <inheritdoc />
    public partial class AddUsuarioRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Columna e índice ya existen - solo se sincroniza el modelo
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No se elimina la columna en rollback
        }
    }
}
