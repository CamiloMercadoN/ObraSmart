import { apiClient, manejarErrorHttp } from '../utils/apiClient';
import type {
  ICotizacion,
  ICrearCotizacionRequest,
  IActualizarEstadoCotizacionRequest,
  IRenovarVigenciaCotizacionRequest
} from '../interfaces/ICotizacion';

const BASE_URL = '/Cotizaciones';

export const cotizacionService = {

  // Endpoint pendiente de agregar en el backend para listar la grilla
  async obtenerTodas(): Promise<ICotizacion[]> {
    const response = await apiClient(BASE_URL);
    if (!response.ok) await manejarErrorHttp(response);
    return response.json();
  },

  async obtenerPorId(id: string): Promise<ICotizacion> {
    const response = await apiClient(`${BASE_URL}/${id}`);
    if (!response.ok) await manejarErrorHttp(response);
    return response.json();
  },

  async crear(request: ICrearCotizacionRequest): Promise<ICotizacion> {
    const response = await apiClient(BASE_URL, {
      method: 'POST',
      body: JSON.stringify(request),
    });
    if (!response.ok) await manejarErrorHttp(response);
    return response.json();
  },

  async actualizarEstado(id: string, request: IActualizarEstadoCotizacionRequest): Promise<ICotizacion> {
    const response = await apiClient(`${BASE_URL}/${id}/estado`, {
      method: 'PATCH',
      body: JSON.stringify(request),
    });
    if (!response.ok) await manejarErrorHttp(response);
    return response.json();
  },

  async eliminar(id: string): Promise<void> {
    const response = await apiClient(`${BASE_URL}/${id}`, {
      method: 'DELETE',
    });
    if (!response.ok) await manejarErrorHttp(response);
  },

  async renovarVigencia(id: string, request: IRenovarVigenciaCotizacionRequest): Promise<ICotizacion> {
    const response = await apiClient(`${BASE_URL}/${id}/vigencia`, {
      method: 'PATCH',
      body: JSON.stringify(request),
    });
    if (!response.ok) await manejarErrorHttp(response);
    return response.json();
  },

  // Descarga tradicional del PDF
  async descargarPdf(id: string, numeroCotizacion: string, incluirRecursos: boolean = false): Promise<void> {
    const response = await apiClient(`${BASE_URL}/${id}/pdf?incluirRecursos=${incluirRecursos}`);
    if (!response.ok) await manejarErrorHttp(response);

    const blob = await response.blob();
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `Cotizacion-${numeroCotizacion}.pdf`;
    document.body.appendChild(a);
    a.click();
    a.remove();
    window.URL.revokeObjectURL(url);
  },

  async obtenerPdfBlob(id: string, incluirRecursos: boolean = false): Promise<Blob> {
    const response = await apiClient(`${BASE_URL}/${id}/pdf?incluirRecursos=${incluirRecursos}`);
    return await response.blob();
  },

  // Integración PWA - Compartir PDF nativo (WhatsApp, Correo, etc.)
  async compartirPdf(id: string, numeroCotizacion: string, incluirRecursos: boolean = false): Promise<void> {
    const response = await apiClient(`${BASE_URL}/${id}/pdf?incluirRecursos=${incluirRecursos}`);
    if (!response.ok) await manejarErrorHttp(response);

    const blob = await response.blob();
    const file = new File([blob], `Cotizacion-${numeroCotizacion}.pdf`, { type: 'application/pdf' });

    // Verifica si el dispositivo soporta Web Share API con archivos (soportado en móviles modernos)
    if (navigator.canShare && navigator.canShare({ files: [file] })) {
      await navigator.share({
        title: `Cotización ${numeroCotizacion}`,
        text: 'Adjunto la cotización solicitada.',
        files: [file]
      });
    } else {
      // Fallback: Si se ejecuta en un PC sin soporte de share, fuerza la descarga
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = file.name;
      document.body.appendChild(a);
      a.click();
      a.remove();
      window.URL.revokeObjectURL(url);
    }
  }
};
