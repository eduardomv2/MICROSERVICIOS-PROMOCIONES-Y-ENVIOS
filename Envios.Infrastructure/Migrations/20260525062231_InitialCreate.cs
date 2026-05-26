using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Envios.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ENV_Envio",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdOrden = table.Column<int>(type: "integer", nullable: false),
                    DireccionSnapshot = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    GuiaPaqueteria = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EstadoActual = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    NombreRepartidor = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    FechaEstimada = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaEntregado = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IdRepartidor = table.Column<int>(type: "integer", nullable: true),
                    TelefonoRepartidor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ENV_Envio", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ENV_Repartidor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EstaDisponible = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ENV_Repartidor", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ENV_HistorialRastreo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdEnvio = table.Column<Guid>(type: "uuid", nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Nota = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FechaEvento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NuevaFechaProgramada = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ENV_HistorialRastreo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ENV_HistorialRastreo_ENV_Envio_IdEnvio",
                        column: x => x.IdEnvio,
                        principalTable: "ENV_Envio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ENV_HistorialRastreo_IdEnvio",
                table: "ENV_HistorialRastreo",
                column: "IdEnvio");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ENV_HistorialRastreo");

            migrationBuilder.DropTable(
                name: "ENV_Repartidor");

            migrationBuilder.DropTable(
                name: "ENV_Envio");
        }
    }
}
