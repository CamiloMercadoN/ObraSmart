using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ObraSmart.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PrecargaInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "Insumos");

            migrationBuilder.DropColumn(
                name: "UnidadMedida",
                table: "Insumos");

            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "EstructurasAPU");

            migrationBuilder.DropColumn(
                name: "UnidadMedida",
                table: "EstructurasAPU");

            migrationBuilder.AddColumn<int>(
                name: "UnidadMedidaId",
                table: "RecursosItemPresupuesto",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "EsPlantilla",
                table: "Presupuestos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "UnidadMedidaId",
                table: "ItemsPresupuesto",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "EsPlantilla",
                table: "Insumos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "UnidadMedidaId",
                table: "Insumos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "EsPlantilla",
                table: "EstructurasAPU",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "UnidadMedidaId",
                table: "EstructurasAPU",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "EsPlantilla",
                table: "Clientes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Etiquetas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EsPlantilla = table.Column<bool>(type: "bit", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ColorHex = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Etiquetas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UnidadesMedida",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Abreviacion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnidadesMedida", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EstructuraAPUEtiqueta",
                columns: table => new
                {
                    EstructurasAPUId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EtiquetasId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstructuraAPUEtiqueta", x => new { x.EstructurasAPUId, x.EtiquetasId });
                    table.ForeignKey(
                        name: "FK_EstructuraAPUEtiqueta_EstructurasAPU_EstructurasAPUId",
                        column: x => x.EstructurasAPUId,
                        principalTable: "EstructurasAPU",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EstructuraAPUEtiqueta_Etiquetas_EtiquetasId",
                        column: x => x.EtiquetasId,
                        principalTable: "Etiquetas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EtiquetaInsumo",
                columns: table => new
                {
                    EtiquetasId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InsumosId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EtiquetaInsumo", x => new { x.EtiquetasId, x.InsumosId });
                    table.ForeignKey(
                        name: "FK_EtiquetaInsumo_Etiquetas_EtiquetasId",
                        column: x => x.EtiquetasId,
                        principalTable: "Etiquetas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EtiquetaInsumo_Insumos_InsumosId",
                        column: x => x.InsumosId,
                        principalTable: "Insumos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecursosItemPresupuesto_UnidadMedidaId",
                table: "RecursosItemPresupuesto",
                column: "UnidadMedidaId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemsPresupuesto_UnidadMedidaId",
                table: "ItemsPresupuesto",
                column: "UnidadMedidaId");

            migrationBuilder.CreateIndex(
                name: "IX_Insumos_UnidadMedidaId",
                table: "Insumos",
                column: "UnidadMedidaId");

            migrationBuilder.CreateIndex(
                name: "IX_EstructurasAPU_UnidadMedidaId",
                table: "EstructurasAPU",
                column: "UnidadMedidaId");

            migrationBuilder.CreateIndex(
                name: "IX_EstructuraAPUEtiqueta_EtiquetasId",
                table: "EstructuraAPUEtiqueta",
                column: "EtiquetasId");

            migrationBuilder.CreateIndex(
                name: "IX_EtiquetaInsumo_InsumosId",
                table: "EtiquetaInsumo",
                column: "InsumosId");

            migrationBuilder.AddForeignKey(
                name: "FK_EstructurasAPU_UnidadesMedida_UnidadMedidaId",
                table: "EstructurasAPU",
                column: "UnidadMedidaId",
                principalTable: "UnidadesMedida",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Insumos_UnidadesMedida_UnidadMedidaId",
                table: "Insumos",
                column: "UnidadMedidaId",
                principalTable: "UnidadesMedida",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemsPresupuesto_UnidadesMedida_UnidadMedidaId",
                table: "ItemsPresupuesto",
                column: "UnidadMedidaId",
                principalTable: "UnidadesMedida",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RecursosItemPresupuesto_UnidadesMedida_UnidadMedidaId",
                table: "RecursosItemPresupuesto",
                column: "UnidadMedidaId",
                principalTable: "UnidadesMedida",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EstructurasAPU_UnidadesMedida_UnidadMedidaId",
                table: "EstructurasAPU");

            migrationBuilder.DropForeignKey(
                name: "FK_Insumos_UnidadesMedida_UnidadMedidaId",
                table: "Insumos");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemsPresupuesto_UnidadesMedida_UnidadMedidaId",
                table: "ItemsPresupuesto");

            migrationBuilder.DropForeignKey(
                name: "FK_RecursosItemPresupuesto_UnidadesMedida_UnidadMedidaId",
                table: "RecursosItemPresupuesto");

            migrationBuilder.DropTable(
                name: "EstructuraAPUEtiqueta");

            migrationBuilder.DropTable(
                name: "EtiquetaInsumo");

            migrationBuilder.DropTable(
                name: "UnidadesMedida");

            migrationBuilder.DropTable(
                name: "Etiquetas");

            migrationBuilder.DropIndex(
                name: "IX_RecursosItemPresupuesto_UnidadMedidaId",
                table: "RecursosItemPresupuesto");

            migrationBuilder.DropIndex(
                name: "IX_ItemsPresupuesto_UnidadMedidaId",
                table: "ItemsPresupuesto");

            migrationBuilder.DropIndex(
                name: "IX_Insumos_UnidadMedidaId",
                table: "Insumos");

            migrationBuilder.DropIndex(
                name: "IX_EstructurasAPU_UnidadMedidaId",
                table: "EstructurasAPU");

            migrationBuilder.DropColumn(
                name: "UnidadMedidaId",
                table: "RecursosItemPresupuesto");

            migrationBuilder.DropColumn(
                name: "EsPlantilla",
                table: "Presupuestos");

            migrationBuilder.DropColumn(
                name: "UnidadMedidaId",
                table: "ItemsPresupuesto");

            migrationBuilder.DropColumn(
                name: "EsPlantilla",
                table: "Insumos");

            migrationBuilder.DropColumn(
                name: "UnidadMedidaId",
                table: "Insumos");

            migrationBuilder.DropColumn(
                name: "EsPlantilla",
                table: "EstructurasAPU");

            migrationBuilder.DropColumn(
                name: "UnidadMedidaId",
                table: "EstructurasAPU");

            migrationBuilder.DropColumn(
                name: "EsPlantilla",
                table: "Clientes");

            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "Insumos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UnidadMedida",
                table: "Insumos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "EstructurasAPU",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UnidadMedida",
                table: "EstructurasAPU",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
