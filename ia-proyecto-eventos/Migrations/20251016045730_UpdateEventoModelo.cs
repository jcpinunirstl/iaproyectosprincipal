using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IaProyectoEventos.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEventoModelo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Fecha",
                table: "Eventos");

            migrationBuilder.RenameColumn(
                name: "Titulo",
                table: "Eventos",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "Lugar",
                table: "Eventos",
                newName: "Direccion");

            migrationBuilder.AddColumn<decimal>(
                name: "Costo",
                table: "Eventos",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "Estado",
                table: "Eventos",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "FechaFin",
                table: "Eventos",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateOnly>(
                name: "FechaInicio",
                table: "Eventos",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "HoraFin",
                table: "Eventos",
                type: "time(6)",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "HoraInicio",
                table: "Eventos",
                type: "time(6)",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Costo",
                table: "Eventos");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Eventos");

            migrationBuilder.DropColumn(
                name: "FechaFin",
                table: "Eventos");

            migrationBuilder.DropColumn(
                name: "FechaInicio",
                table: "Eventos");

            migrationBuilder.DropColumn(
                name: "HoraFin",
                table: "Eventos");

            migrationBuilder.DropColumn(
                name: "HoraInicio",
                table: "Eventos");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "Eventos",
                newName: "Titulo");

            migrationBuilder.RenameColumn(
                name: "Direccion",
                table: "Eventos",
                newName: "Lugar");

            migrationBuilder.AddColumn<DateTime>(
                name: "Fecha",
                table: "Eventos",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
