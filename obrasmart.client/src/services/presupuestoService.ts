import { apiClient, manejarErrorHttp } from '../utils/apiClient';
import type { IPresupuesto } from '../interfaces/IPresupuesto';

const BASE_URL = '/Presupuestos';

export const presupuestoService = {
  async obtenerTodos(): Promise<IPresupuesto[]> {
    const response = await apiClient(BASE_URL);
    if (!response.ok) await manejarErrorHttp(response);
    return response.json();
  },

  async obtenerPorId(id: string): Promise<IPresupuesto> {
    const response = await apiClient(`${BASE_URL}/${id}`);
    if (!response.ok) await manejarErrorHttp(response);
    return response.json();
  },

  async crear(presupuesto: IPresupuesto): Promise<string> {
    const response = await apiClient(BASE_URL, {
      method: 'POST',
      body: JSON.stringify(presupuesto),
    });
    if (!response.ok) await manejarErrorHttp(response);
    return response.json(); // Retorna el Guid del presupuesto creado
  },

  async actualizar(id: string, presupuesto: IPresupuesto): Promise<void> {
    const response = await apiClient(`${BASE_URL}/${id}`, {
      method: 'PUT',
      body: JSON.stringify(presupuesto),
    });
    if (!response.ok) await manejarErrorHttp(response);
  },

  async eliminar(id: string): Promise<void> {
    const response = await apiClient(`${BASE_URL}/${id}`, {
      method: 'DELETE',
    });
    if (!response.ok) await manejarErrorHttp(response);
  }
};
