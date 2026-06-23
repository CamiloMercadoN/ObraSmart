using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ObraSmart.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RazonSocial = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rut = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PorcentajeIva = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    ValidezCotizacionDias = table.Column<int>(type: "int", nullable: false),
                    FormaPagoPredeterminada = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rut = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clientes_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EstructurasAPU",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UnidadMedida = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CostoTotalCalculado = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstructurasAPU", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EstructurasAPU_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Insumos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipoInsumo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrecioReferencia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnidadMedida = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Insumos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Insumos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Presupuestos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NombreProyecto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MontoIva = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Presupuestos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Presupuestos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Presupuestos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComponentesAPU",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EstructuraAPUId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InsumoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComponentesAPU", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComponentesAPU_EstructurasAPU_EstructuraAPUId",
                        column: x => x.EstructuraAPUId,
                        principalTable: "EstructurasAPU",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComponentesAPU_Insumos_InsumoId",
                        column: x => x.InsumoId,
                        principalTable: "Insumos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Cotizaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PresupuestoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroCotizacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ArchivoPdfUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cotizaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cotizaciones_Presupuestos_PresupuestoId",
                        column: x => x.PresupuestoId,
                        principalTable: "Presupuestos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemsPresupuesto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PresupuestoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EstructuraAPUOrigenId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CantidadItem = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PrecioUnitarioCalculado = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemsPresupuesto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemsPresupuesto_EstructurasAPU_EstructuraAPUOrigenId",
                        column: x => x.EstructuraAPUOrigenId,
                        principalTable: "EstructurasAPU",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItemsPresupuesto_Presupuestos_PresupuestoId",
                        column: x => x.PresupuestoId,
                        principalTable: "Presupuestos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecursosItemPresupuesto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemPresupuestoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoInsumo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescripcionCongelada = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PrecioUnitarioCongelado = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CostoTotalRecurso = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecursosItemPresupuesto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecursosItemPresupuesto_ItemsPresupuesto_ItemPresupuestoId",
                        column: x => x.ItemPresupuestoId,
                        principalTable: "ItemsPresupuesto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_UsuarioId",
                table: "Clientes",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ComponentesAPU_EstructuraAPUId",
                table: "ComponentesAPU",
                column: "EstructuraAPUId");

            migrationBuilder.CreateIndex(
                name: "IX_ComponentesAPU_InsumoId",
                table: "ComponentesAPU",
                column: "InsumoId");

            migrationBuilder.CreateIndex(
                name: "IX_Cotizaciones_PresupuestoId",
                table: "Cotizaciones",
                column: "PresupuestoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EstructurasAPU_UsuarioId",
                table: "EstructurasAPU",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Insumos_UsuarioId",
                table: "Insumos",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemsPresupuesto_EstructuraAPUOrigenId",
                table: "ItemsPresupuesto",
                column: "EstructuraAPUOrigenId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemsPresupuesto_PresupuestoId",
                table: "ItemsPresupuesto",
                column: "PresupuestoId");

            migrationBuilder.CreateIndex(
                name: "IX_Presupuestos_ClienteId",
                table: "Presupuestos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Presupuestos_UsuarioId",
                table: "Presupuestos",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_RecursosItemPresupuesto_ItemPresupuestoId",
                table: "RecursosItemPresupuesto",
                column: "ItemPresupuestoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComponentesAPU");

            migrationBuilder.DropTable(
                name: "Cotizaciones");

            migrationBuilder.DropTable(
                name: "RecursosItemPresupuesto");

            migrationBuilder.DropTable(
                name: "Insumos");

            migrationBuilder.DropTable(
                name: "ItemsPresupuesto");

            migrationBuilder.DropTable(
                name: "EstructurasAPU");

            migrationBuilder.DropTable(
                name: "Presupuestos");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
