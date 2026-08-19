using ObraSmart.Application.Common;
using ObraSmart.Application.DTOs.Dashboard;
using ObraSmart.Application.Interfaces.Services;
using ObraSmart.Domain.Entities;
using ObraSmart.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace ObraSmart.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IPresupuestoRepository _presupuestoRepository;
        private readonly IEstructuraAPURepository _apuRepository;

        public DashboardService(
            IPresupuestoRepository presupuestoRepository,
            IEstructuraAPURepository apuRepository)
        {
            _presupuestoRepository = presupuestoRepository;
            _apuRepository = apuRepository;
        }

        public async Task<Result<DashboardDto>> ObtenerResumenAsync(Guid usuarioId)
        {
            try
            {
                var presupuestos = await _presupuestoRepository.GetAllWithDependenciesAsync();
                var apus = await _apuRepository.GetAllAsync();

                // Calculamos las métricas
                var presupuestosActivos = presupuestos.Count(p => (p.Estado == "Borrador" || p.Estado == "Emitido") && !p.EsPlantilla);

                // Extraemos todas las cotizaciones a partir de los presupuestos del usuario
                var cotizaciones = presupuestos.SelectMany(p => p.Cotizaciones ?? new List<Cotizacion>()).ToList();

                var cotizacionesEnviadas = cotizaciones.Count(c => c.Estado == "Emitida");
                var pendientes = cotizaciones.Count(c => c.Estado == "Borrador");
                var apusCreadas = apus.Count(a=> !a.EsPlantilla);

                var proyectosRecientes = presupuestos
                    .Where(p => !p.EsPlantilla)
                    .OrderByDescending(p => p.FechaCreacion) // O FechaEmision, según tu modelo
                    .Take(5)
                    .Select(p => new ProyectoRecienteDto
                    {
                        Id = p.Id,
                        Titulo = p.NombreProyecto,
                        Cliente = p.Cliente?.Nombre ?? "Sin Cliente",
                        Fecha = p.FechaCreacion,
                        Estado = p.Estado
                    }).ToList();

                var dashboardData = new DashboardDto
                {
                    PresupuestosActivos = presupuestosActivos,
                    CotizacionesEnviadas = cotizacionesEnviadas,
                    ApusCreadas = apusCreadas,
                    Pendientes = pendientes,
                    ProyectosRecientes = proyectosRecientes
                };

                return Result<DashboardDto>.Success(dashboardData);
            }
            catch (Exception ex)
            {
                return Result<DashboardDto>.Failure("Error al cargar el dashboard: " + ex.Message, "ERROR_DASHBOARD");
            }
        }
    }
}
