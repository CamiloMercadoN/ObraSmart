using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ObraSmart.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DisableGlobalCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clientes_Usuarios_UsuarioId",
                table: "Clientes");

            migrationBuilder.DropForeignKey(
                name: "FK_Cotizaciones_Presupuestos_PresupuestoId",
                table: "Cotizaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_EstructurasAPU_Usuarios_UsuarioId",
                table: "EstructurasAPU");

            migrationBuilder.DropForeignKey(
                name: "FK_Insumos_Usuarios_UsuarioId",
                table: "Insumos");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemsPresupuesto_EstructurasAPU_EstructuraAPUOrigenId",
                table: "ItemsPresupuesto");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemsPresupuesto_Presupuestos_PresupuestoId",
                table: "ItemsPresupuesto");

            migrationBuilder.DropForeignKey(
                name: "FK_RecursosItemPresupuesto_ItemsPresupuesto_ItemPresupuestoId",
                table: "RecursosItemPresupuesto");

            migrationBuilder.AddForeignKey(
                name: "FK_Clientes_Usuarios_UsuarioId",
                table: "Clientes",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cotizaciones_Presupuestos_PresupuestoId",
                table: "Cotizaciones",
                column: "PresupuestoId",
                principalTable: "Presupuestos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EstructurasAPU_Usuarios_UsuarioId",
                table: "EstructurasAPU",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Insumos_Usuarios_UsuarioId",
                table: "Insumos",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemsPresupuesto_EstructurasAPU_EstructuraAPUOrigenId",
                table: "ItemsPresupuesto",
                column: "EstructuraAPUOrigenId",
                principalTable: "EstructurasAPU",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemsPresupuesto_Presupuestos_PresupuestoId",
                table: "ItemsPresupuesto",
                column: "PresupuestoId",
                principalTable: "Presupuestos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RecursosItemPresupuesto_ItemsPresupuesto_ItemPresupuestoId",
                table: "RecursosItemPresupuesto",
                column: "ItemPresupuestoId",
                principalTable: "ItemsPresupuesto",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clientes_Usuarios_UsuarioId",
                table: "Clientes");

            migrationBuilder.DropForeignKey(
                name: "FK_Cotizaciones_Presupuestos_PresupuestoId",
                table: "Cotizaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_EstructurasAPU_Usuarios_UsuarioId",
                table: "EstructurasAPU");

            migrationBuilder.DropForeignKey(
                name: "FK_Insumos_Usuarios_UsuarioId",
                table: "Insumos");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemsPresupuesto_EstructurasAPU_EstructuraAPUOrigenId",
                table: "ItemsPresupuesto");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemsPresupuesto_Presupuestos_PresupuestoId",
                table: "ItemsPresupuesto");

            migrationBuilder.DropForeignKey(
                name: "FK_RecursosItemPresupuesto_ItemsPresupuesto_ItemPresupuestoId",
                table: "RecursosItemPresupuesto");

            migrationBuilder.AddForeignKey(
                name: "FK_Clientes_Usuarios_UsuarioId",
                table: "Clientes",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cotizaciones_Presupuestos_PresupuestoId",
                table: "Cotizaciones",
                column: "PresupuestoId",
                principalTable: "Presupuestos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EstructurasAPU_Usuarios_UsuarioId",
                table: "EstructurasAPU",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Insumos_Usuarios_UsuarioId",
                table: "Insumos",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemsPresupuesto_EstructurasAPU_EstructuraAPUOrigenId",
                table: "ItemsPresupuesto",
                column: "EstructuraAPUOrigenId",
                principalTable: "EstructurasAPU",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemsPresupuesto_Presupuestos_PresupuestoId",
                table: "ItemsPresupuesto",
                column: "PresupuestoId",
                principalTable: "Presupuestos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RecursosItemPresupuesto_ItemsPresupuesto_ItemPresupuestoId",
                table: "RecursosItemPresupuesto",
                column: "ItemPresupuestoId",
                principalTable: "ItemsPresupuesto",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
