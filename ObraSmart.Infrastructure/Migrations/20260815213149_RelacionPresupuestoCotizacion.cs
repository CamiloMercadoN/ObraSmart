using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ObraSmart.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RelacionPresupuestoCotizacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Cotizaciones_PresupuestoId",
                table: "Cotizaciones");

            migrationBuilder.AddColumn<int>(
                name: "UltimoNumeroCotizacion",
                table: "Usuarios",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Cotizaciones_PresupuestoId",
                table: "Cotizaciones",
                column: "PresupuestoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Cotizaciones_PresupuestoId",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "UltimoNumeroCotizacion",
                table: "Usuarios");

            migrationBuilder.CreateIndex(
                name: "IX_Cotizaciones_PresupuestoId",
                table: "Cotizaciones",
                column: "PresupuestoId",
                unique: true);
        }
    }
}
