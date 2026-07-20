import { apiClient, manejarErrorHttp } from '../utils/apiClient';
import type { IEstructuraAPU, IEstructuraAPUUpsert } from '../interfaces/IApu';

export const apuService = {
  async obtenerTodos(): Promise<IEstructuraAPU[]> {
    const response = await apiClient('/apus');
    if (!response.ok) throw new Error('Error al cargar la lista de APUs.');
    return await response.json();
  },

  async obtenerPorId(id: string): Promise<IEstructuraAPU> {
    const response = await apiClient(`/apus/${id}`);
    if (!response.ok) throw new Error('Error al obtener los detalles del APU.');
    return await response.json();
  },

  async crear(apu: IEstructuraAPUUpsert): Promise<string> {
    const response = await apiClient('/apus', {
      method: 'POST',
      body: JSON.stringify(apu)
    });

    if (!response.ok) await manejarErrorHttp(response);
    return await response.json();
  },

  async actualizar(id: string, apu: IEstructuraAPUUpsert): Promise<void> {
    const response = await apiClient(`/apus/${id}`, {
      method: 'PUT',
      body: JSON.stringify(apu)
    });

    if (!response.ok) await manejarErrorHttp(response);
  },

  async eliminar(id: string): Promise<void> {
    const response = await apiClient(`/apus/${id}`, {
      method: 'DELETE'
    });

    if (!response.ok) await manejarErrorHttp(response);
  },

  async recalcularCosto(id: string): Promise<void> {
    const response = await apiClient(`/apus/${id}/recalcular`, {
      method: 'PATCH'
    });

    if (!response.ok) await manejarErrorHttp(response);
  }
};
