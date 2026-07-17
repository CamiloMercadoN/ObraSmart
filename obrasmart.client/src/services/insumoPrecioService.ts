import { apiClient, manejarErrorHttp } from '../utils/apiClient';

export interface IReajusteLote {
  tipoInsumo?: string | null;
  etiquetaId?: string | null;
  esPorcentaje: boolean;
  valor: number;
}

export interface IResumenProcesamiento {
  procesados: number;
  actualizados: number;
  detalleErrores: string[];
}

export const insumoPrecioService = {
  async actualizarIndividual(id: string, nuevoPrecio: number): Promise<void> {
    const response = await apiClient(`/insumos/precios/${id}`, {
      method: 'PATCH',
      body: JSON.stringify({ nuevoPrecio })
    });

    if (!response.ok) await manejarErrorHttp(response);
  },

  async reajustarLote(payload: IReajusteLote): Promise<IResumenProcesamiento> {
    const response = await apiClient('/insumos/precios/reajuste-lote', {
      method: 'POST',
      body: JSON.stringify(payload)
    });

    if (!response.ok) await manejarErrorHttp(response);
    return await response.json();
  },

  async importarCsv(archivo: File): Promise<IResumenProcesamiento> {
    const formData = new FormData();
    formData.append('archivo', archivo);

    const response = await apiClient('/insumos/precios/importar-csv', {
      method: 'POST',
      body: formData
    });

    if (!response.ok) await manejarErrorHttp(response);
    return await response.json();
  },
  async descargarPlantilla(): Promise<void> {
    const response = await apiClient('/insumos/precios/exportar-plantilla');

    if (!response.ok) await manejarErrorHttp(response);

    // Convertir la respuesta a un Blob (archivo binario)
    const blob = await response.blob();
    const url = window.URL.createObjectURL(blob);

    // Crear un enlace temporal para forzar la descarga
    const a = document.createElement('a');
    a.href = url;
    a.download = 'Plantilla_Actualizacion_Precios.csv';
    document.body.appendChild(a);
    a.click();

    // Limpieza
    a.remove();
    window.URL.revokeObjectURL(url);
  }
};
