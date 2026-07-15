import { apiClient, manejarErrorHttp } from '../utils/apiClient';
import type { IInsumo, IUnidadMedida, IEtiqueta } from '../interfaces/IInsumo';

export const insumoService = {
  async obtenerTodos(): Promise<IInsumo[]> {
    const response = await apiClient('/Insumos');
    if (!response.ok) throw new Error('Error al cargar la lista de insumos.');
    return await response.json();
  },

  async obtenerPorId(id: string): Promise<IInsumo> {
    const response = await apiClient(`/Insumos/${id}`);
    if (!response.ok) throw new Error('Error al obtener los detalles del insumo.');
    return await response.json();
  },

  async crear(insumo: IInsumo): Promise<string> {
    const response = await apiClient('/Insumos', {
      method: 'POST',
      body: JSON.stringify(insumo)
    });

    if (!response.ok) await manejarErrorHttp(response);
    return await response.json();
  },

  async actualizar(id: string, insumo: IInsumo): Promise<void> {
    const response = await apiClient(`/Insumos/${id}`, {
      method: 'PUT',
      body: JSON.stringify(insumo)
    });

    if (!response.ok) await manejarErrorHttp(response);
  },

  async eliminar(id: string): Promise<void> {
    const response = await apiClient(`/Insumos/${id}`, {
      method: 'DELETE'
    });

    if (!response.ok) await manejarErrorHttp(response);
  },

  // Endpoints Auxiliares para cargar selectores en el Formulario
  async obtenerUnidadesMedida(): Promise<IUnidadMedida[]> {
    const response = await apiClient('/UnidadesMedida');
    if (!response.ok) throw new Error('Error al cargar las unidades de medida.');
    return await response.json();
  },

  async obtenerEtiquetas(): Promise<IEtiqueta[]> {
    const response = await apiClient('/Etiquetas');
    if (!response.ok) throw new Error('Error al cargar las etiquetas.');
    return await response.json();
  },
};
