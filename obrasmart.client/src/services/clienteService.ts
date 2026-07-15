import { apiClient, manejarErrorHttp } from '../utils/apiClient';
import type { ICliente } from '../interfaces/ICliente';

export const clienteService = {
  async obtenerTodos(): Promise<ICliente[]> {
    const response = await apiClient('/Clientes');
    if (!response.ok) throw new Error('Error al cargar la lista de clientes.');
    return await response.json();
  },

  async obtenerPorId(id: string): Promise<ICliente> {
    const response = await apiClient(`/Clientes/${id}`);
    if (!response.ok) throw new Error('Error al obtener los detalles del cliente.');
    return await response.json();
  },

  async crear(cliente: ICliente): Promise<ICliente> {
    const response = await apiClient('/Clientes', {
      method: 'POST',
      body: JSON.stringify(cliente)
    });

    if (!response.ok) await manejarErrorHttp(response);
    return await response.json();
  },

  async actualizar(id: string, cliente: ICliente): Promise<void> {
    const response = await apiClient(`/Clientes/${id}`, {
      method: 'PUT',
      body: JSON.stringify(cliente)
    });

    if (!response.ok) await manejarErrorHttp(response);
  },

  async eliminar(id: string): Promise<void> {
    const response = await apiClient(`/Clientes/${id}`, {
      method: 'DELETE'
    });

    if (!response.ok) await manejarErrorHttp(response);
  },
};
