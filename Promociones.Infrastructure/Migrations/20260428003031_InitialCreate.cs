using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Promociones.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PRO_Descuento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NombreCampana = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PorcentajeDescuento = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    MontoMinimoCompra = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    FechaInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EsIndefinido = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PRO_Descuento", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PRO_PromocionMSI",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdPromocionDescuento = table.Column<Guid>(type: "uuid", nullable: false),
                    BancosParticipantes = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Meses = table.Column<int>(type: "integer", nullable: false),
                    MontoMinimoCompra = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PRO_PromocionMSI", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PRO_PromocionMSI_PRO_Descuento_IdPromocionDescuento",
                        column: x => x.IdPromocionDescuento,
                        principalTable: "PRO_Descuento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PRO_ReglaCategoria",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdPromocionDescuento = table.Column<Guid>(type: "uuid", nullable: false),
                    IdCategoriaCatalogo = table.Column<Guid>(type: "uuid", nullable: false),
                    PorcentajeAplicable = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PRO_ReglaCategoria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PRO_ReglaCategoria_PRO_Descuento_IdPromocionDescuento",
                        column: x => x.IdPromocionDescuento,
                        principalTable: "PRO_Descuento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PRO_PromocionMSI_IdPromocionDescuento",
                table: "PRO_PromocionMSI",
                column: "IdPromocionDescuento");

            migrationBuilder.CreateIndex(
                name: "IX_PRO_ReglaCategoria_IdPromocionDescuento_IdCategoriaCatalogo",
                table: "PRO_ReglaCategoria",
                columns: new[] { "IdPromocionDescuento", "IdCategoriaCatalogo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PRO_PromocionMSI");

            migrationBuilder.DropTable(
                name: "PRO_ReglaCategoria");

            migrationBuilder.DropTable(
                name: "PRO_Descuento");
        }
    }
}
