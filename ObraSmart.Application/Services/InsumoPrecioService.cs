using ObraSmart.Application.Common;
using ObraSmart.Application.DTOs.Insumos;
using ObraSmart.Application.Interfaces.Services;
using ObraSmart.Domain.Interfaces.Repositories;
using System.Globalization;
using System.Text;

namespace ObraSmart.Application.Services
{
    public class InsumoPrecioService : IInsumoPrecioService
    {
        private readonly IInsumoRepository _insumoRepository;

        public InsumoPrecioService(IInsumoRepository insumoRepository)
        {
            _insumoRepository = insumoRepository;
        }

        public async Task<Result> ActualizarPrecioIndividualAsync(Guid id, ActualizarPrecioDto dto)
        {
            var insumo = await _insumoRepository.GetByIdAsync(id);
            if (insumo == null)
                return Result.Failure("Insumo no encontrado.");

            if (insumo.EsPlantilla)
                return Result.Failure("No se pueden modificar los precios de las plantillas globales del sistema.");

            insumo.PrecioReferencia = dto.NuevoPrecio;
            await _insumoRepository.UpdateAsync(insumo);

            return Result.Success();
        }

        public async Task<Result<ResumenProcesamientoDto>> ReajustarPreciosLoteAsync(ReajusteLoteDto dto)
        {
            var todos = await _insumoRepository.GetAllWithDependenciesAsync();

            var query = todos.Where(i => !i.EsPlantilla);

            if (!string.IsNullOrEmpty(dto.TipoInsumo))
            {
                query = query.Where(i => i.TipoInsumo.Equals(dto.TipoInsumo, StringComparison.OrdinalIgnoreCase));
            }

            if (dto.EtiquetaId.HasValue)
            {
                query = query.Where(i => i.Etiquetas.Any(e => e.Id == dto.EtiquetaId.Value));
            }

            var insumosAEditar = query.ToList();
            var resumen = new ResumenProcesamientoDto
            {
                Procesados = insumosAEditar.Count
            };

            foreach (var insumo in insumosAEditar)
            {
                decimal nuevoPrecio = insumo.PrecioReferencia;

                if (dto.EsPorcentaje)
                {
                    decimal factor = 1 + (dto.Valor / 100);
                    nuevoPrecio *= factor;
                }
                else
                {
                    nuevoPrecio += dto.Valor;
                }

                // Asegurar que el precio no caiga por debajo de cero tras el reajuste
                insumo.PrecioReferencia = Math.Max(0, Math.Round(nuevoPrecio, 2));
                insumo.Etiquetas = null!;
                await _insumoRepository.UpdateAsync(insumo);
                resumen.Actualizados++;
            }

            return Result<ResumenProcesamientoDto>.Success(resumen);
        }

        // Importación Masiva mediante CSV
        public async Task<Result<byte[]>> ExportarPlantillaCsvAsync()
        {
            var insumos = await _insumoRepository.GetAllAsync();
            var editables = insumos.Where(i => !i.EsPlantilla).OrderBy(i => i.Descripcion);

            var sb = new StringBuilder();
            sb.AppendLine("Id;Descripcion;PrecioActual;NuevoPrecio");

            foreach (var insumo in editables)
            {
                // Limpiamos los punto y coma de la descripción para no romper el formato CSV
                var descLimpia = insumo.Descripcion.Replace(";", ",").Replace("\r", "").Replace("\n", "");
                var precioActualStr = insumo.PrecioReferencia.ToString("F2", CultureInfo.InvariantCulture);

                // Dejamos la cuarta columna vacía para que el usuario la llene
                sb.AppendLine($"{insumo.Id};{descLimpia};{precioActualStr};");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var bom = Encoding.UTF8.GetPreamble(); // Agrega el BOM para que Excel detecte UTF-8 y no rompa los acentos

            return Result<byte[]>.Success(bom.Concat(bytes).ToArray());
        }
        public async Task<Result<ResumenProcesamientoDto>> ImportarPreciosCsvAsync(Stream fileStream)
        {
            var resumen = new ResumenProcesamientoDto();
            using var reader = new StreamReader(fileStream);

            // Saltarse la cabecera
            string? header = await reader.ReadLineAsync();
            int numeroLinea = 1;

            while (!reader.EndOfStream)
            {
                numeroLinea++;
                string? linea = await reader.ReadLineAsync();

                if (string.IsNullOrWhiteSpace(linea)) continue;

                var columnas = linea.Split(';');
                if (columnas.Length < 4)
                {
                    resumen.DetalleErrores.Add($"Línea {numeroLinea}: Estructura inválida. Faltan columnas.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(columnas[3])) continue;

                resumen.Procesados++;

                // Validar formato de Guid
                if (!Guid.TryParse(columnas[0].Trim(), out Guid insumoId))
                {
                    resumen.DetalleErrores.Add($"Línea {numeroLinea}: El ID '{columnas[0]}' no tiene un formato UUID/Guid válido.");
                    continue;
                }

                //Validar formato numérico de precio
                if (!decimal.TryParse(columnas[3].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal nuevoPrecio))
                {
                    // Reintento con la cultura actual por si usan comas decimales regionales (es-CL)
                    if (!decimal.TryParse(columnas[3].Trim(), NumberStyles.Any, CultureInfo.CurrentCulture, out nuevoPrecio))
                    {
                        resumen.DetalleErrores.Add($"Línea {numeroLinea}: El precio '{columnas[3]}' no es un número decimal válido.");
                        continue;
                    }
                }

                if (nuevoPrecio < 0)
                {
                    resumen.DetalleErrores.Add($"Línea {numeroLinea}: El precio no puede ser un valor negativo ({nuevoPrecio}).");
                    continue;
                }

                // Obtener el insumo del repositorio y comprobar seguridad
                var insumo = await _insumoRepository.GetByIdAsync(insumoId);
                if (insumo == null)
                {
                    resumen.DetalleErrores.Add($"Línea {numeroLinea}: El insumo con ID '{insumoId}' no existe en el catálogo.");
                    continue;
                }

                if (insumo.EsPlantilla)
                {
                    resumen.DetalleErrores.Add($"Línea {numeroLinea}: El insumo '{insumo.Descripcion}' es una plantilla global y no puede modificarse.");
                    continue;
                }

                // Modificar y guardar
                insumo.PrecioReferencia = Math.Round(nuevoPrecio, 2);
                await _insumoRepository.UpdateAsync(insumo);
                resumen.Actualizados++;
            }

            return Result<ResumenProcesamientoDto>.Success(resumen);
        }
    }
}
