export interface ICotizacion {
  id: string;
  presupuestoId: string;
  numeroCotizacion: string;
  fechaEmision: string;
  fechaVencimiento: string;
  estado: string;
  archivoPdfUrl: string;
}

export interface ICrearCotizacionRequest {
  presupuestoId: string;
  fechaVencimiento: string;
  numeroCotizacionPersonalizado?: number | null;
}

export interface IActualizarEstadoCotizacionRequest {
  nuevoEstado: string;
}

export interface IRenovarVigenciaCotizacionRequest {
  nuevaFechaVencimiento: string;
}
