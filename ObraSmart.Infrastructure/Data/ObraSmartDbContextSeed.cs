using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ObraSmart.Domain.Entities;

namespace ObraSmart.Infrastructure.Data
{
    public static class ObraSmartDbContextSeed
    {
        private class InsumoSeedDto : Insumo { public List<Guid> EtiquetasIds { get; set; } = new(); }
        private class ApuSeedDto : EstructuraAPU { public List<Guid> EtiquetasIds { get; set; } = new(); }

        public static async Task SeedAsync(ObraSmartDbContext context)
        {
            var basePath = AppContext.BaseDirectory;
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Territorios
            if (!context.Paises.Any())
            {
                var filePathTerritorios = Path.Combine(basePath, "Data", "SeedData", "territorios.json");
                if (File.Exists(filePathTerritorios))
                {
                    var jsonTerritoriosData = await File.ReadAllTextAsync(filePathTerritorios);
                    var paises = JsonSerializer.Deserialize<List<Pais>>(jsonTerritoriosData, options);

                    if (paises != null && paises.Any())
                    {
                        await context.Paises.AddRangeAsync(paises);
                        await context.SaveChangesAsync();
                    }
                }
            }

            // Unidades de Medida
            if (!context.UnidadesMedida.Any())
            {
                var filePathUnidades = Path.Combine(basePath, "Data", "SeedData", "unidades_medida.json");
                if (File.Exists(filePathUnidades))
                {
                    var jsonUnidadesData = await File.ReadAllTextAsync(filePathUnidades);
                    var unidades = JsonSerializer.Deserialize<List<UnidadMedida>>(jsonUnidadesData, options);

                    if (unidades != null && unidades.Any())
                    {
                        using var transaction = await context.Database.BeginTransactionAsync();
                        await context.UnidadesMedida.AddRangeAsync(unidades);
                        await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [UnidadesMedida] ON");
                        await context.SaveChangesAsync();
                        await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [UnidadesMedida] OFF");
                        await transaction.CommitAsync();
                    }
                }
            }

            // Usuario del Sistema
            var usuarioSistemaId = Guid.Parse("10000000-0000-0000-0000-000000000000");

            if (!context.Usuarios.Any(u => u.Id == usuarioSistemaId))
            {
                var usuarioSistema = new Usuario
                {
                    Id = usuarioSistemaId,
                    Rut = "0-0",
                    RazonSocial = "ObraSmart Base Data",
                    Correo = "sistema@obrasmart.cl",
                    Telefono = "N/A",
                    Direccion = "N/A",
                    CiudadId = null,
                    PasswordHash = "N/A",
                    PorcentajeIva = 19.00m,
                    FormaPagoPredeterminada = "N/A",
                    ValidezCotizacionDias = 0
                };

                await context.Usuarios.AddAsync(usuarioSistema);
                await context.SaveChangesAsync();
            }

            // Etiquetas
            if (!context.Etiquetas.Any(e => e.EsPlantilla))
            {
                var filePath = Path.Combine(basePath, "Data", "SeedData", "etiquetas_plantilla.json");
                if (File.Exists(filePath))
                {
                    var data = await File.ReadAllTextAsync(filePath);
                    var etiquetas = JsonSerializer.Deserialize<List<Etiqueta>>(data, options);
                    if (etiquetas != null)
                    {
                        etiquetas.ForEach(e => e.UsuarioId = usuarioSistemaId);
                        await context.Etiquetas.AddRangeAsync(etiquetas);
                        await context.SaveChangesAsync();
                    }
                }
            }

            var etiquetasDb = context.Etiquetas.ToList();

            // Insumos
            if (!context.Insumos.Any(i => i.EsPlantilla))
            {
                var filePath = Path.Combine(basePath, "Data", "SeedData", "insumos_plantilla.json");
                if (File.Exists(filePath))
                {
                    var data = await File.ReadAllTextAsync(filePath);
                    var insumosDto = JsonSerializer.Deserialize<List<InsumoSeedDto>>(data, options);

                    if (insumosDto != null)
                    {
                        var nuevosInsumos = new List<Insumo>();
                        foreach (var dto in insumosDto)
                        {
                            var nuevoInsumo = new Insumo
                            {
                                Id = dto.Id,
                                UsuarioId = usuarioSistemaId,
                                EsPlantilla = dto.EsPlantilla,
                                Descripcion = dto.Descripcion,
                                PrecioReferencia = dto.PrecioReferencia,
                                TipoInsumo = dto.TipoInsumo,
                                UnidadMedidaId = dto.UnidadMedidaId
                            };
                            nuevosInsumos.Add(nuevoInsumo);
                        }

                        await context.Insumos.AddRangeAsync(nuevosInsumos);
                        await context.SaveChangesAsync();

                        foreach (var dto in insumosDto)
                        {
                            var insumoDb = await context.Insumos.FindAsync(dto.Id);
                            if (insumoDb != null)
                            {
                                foreach (var id in dto.EtiquetasIds)
                                {
                                    var tag = etiquetasDb.FirstOrDefault(e => e.Id == id);
                                    if (tag != null) insumoDb.Etiquetas.Add(tag);
                                }
                            }
                        }
                        await context.SaveChangesAsync();
                    }
                }
            }

            // APUs
            if (!context.EstructurasAPU.Any(a => a.EsPlantilla))
            {
                var filePath = Path.Combine(basePath, "Data", "SeedData", "apu_plantilla.json");
                if (File.Exists(filePath))
                {
                    var data = await File.ReadAllTextAsync(filePath);
                    var apusDto = JsonSerializer.Deserialize<List<ApuSeedDto>>(data, options);

                    if (apusDto != null)
                    {
                        var nuevosApus = new List<EstructuraAPU>();
                        foreach (var dto in apusDto)
                        {
                            var nuevoApu = new EstructuraAPU
                            {
                                Id = dto.Id,
                                UsuarioId = usuarioSistemaId,
                                EsPlantilla = dto.EsPlantilla,
                                Nombre = dto.Nombre,
                                UnidadMedidaId = dto.UnidadMedidaId,
                                CostoTotalCalculado = dto.CostoTotalCalculado,
                                Componentes = dto.Componentes
                            };
                            nuevosApus.Add(nuevoApu);
                        }

                        await context.EstructurasAPU.AddRangeAsync(nuevosApus);
                        await context.SaveChangesAsync();

                        foreach (var dto in apusDto)
                        {
                            var apuDb = await context.EstructurasAPU.FindAsync(dto.Id);
                            if (apuDb != null)
                            {
                                foreach (var id in dto.EtiquetasIds)
                                {
                                    var tag = etiquetasDb.FirstOrDefault(e => e.Id == id);
                                    if (tag != null) apuDb.Etiquetas.Add(tag);
                                }
                            }
                        }
                        await context.SaveChangesAsync();
                    }
                }
            }

            // Presupuestos
            if (!context.Presupuestos.Any(p => p.EsPlantilla))
            {
                var filePathPresupuestos = Path.Combine(basePath, "Data", "SeedData", "presupuesto_plantilla.json");
                if (File.Exists(filePathPresupuestos))
                {
                    var jsonPresupuestosData = await File.ReadAllTextAsync(filePathPresupuestos);
                    var presupuestos = JsonSerializer.Deserialize<List<Presupuesto>>(jsonPresupuestosData, options);

                    if (presupuestos != null && presupuestos.Any())
                    {
                        var apusDb = context.EstructurasAPU.ToList();

                        foreach (var p in presupuestos)
                        {
                            p.UsuarioId = usuarioSistemaId;
                            p.ClienteId = null;

                            foreach (var item in p.Items)
                            {
                                var apuOrigen = apusDb.FirstOrDefault(a => a.Id == item.EstructuraAPUOrigenId);
                                if (apuOrigen != null)
                                {
                                    item.UnidadMedidaId = apuOrigen.UnidadMedidaId;
                                }
                                else
                                {
                                    item.UnidadMedidaId = 1;
                                }
                            }
                        }

                        await context.Presupuestos.AddRangeAsync(presupuestos);
                        await context.SaveChangesAsync();
                    }
                }
            }
        }
    }
}