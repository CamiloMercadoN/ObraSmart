using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ObraSmart.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PresupuestoClienteNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Presupuestos_Clientes_ClienteId",
                table: "Presupuestos");

            migrationBuilder.AlterColumn<Guid>(
                name: "ClienteId",
                table: "Presupuestos",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_Presupuestos_Clientes_ClienteId",
                table: "Presupuestos",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Presupuestos_Clientes_ClienteId",
                table: "Presupuestos");

            migrationBuilder.AlterColumn<Guid>(
                name: "ClienteId",
                table: "Presupuestos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Presupuestos_Clientes_ClienteId",
                table: "Presupuestos",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
