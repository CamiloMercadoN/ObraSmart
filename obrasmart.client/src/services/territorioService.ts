import { apiClient } from '../utils/apiClient';

export interface ITerritorio {
  id: number;
  nombre: string;
}

export const territorioService = {
  async getRegiones(): Promise<ITerritorio[]> {
    const response = await apiClient('/territorios/regiones');

    if (!response.ok) {
      throw new Error('Error al obtener las regiones');
    }

    return await response.json();
  },

  async getCiudadesPorRegion(regionId: number): Promise<ITerritorio[]> {
    const response = await apiClient(`/territorios/regiones/${regionId}/ciudades`);

    if (!response.ok) {
      throw new Error('Error al obtener las comunas');
    }

    return await response.json();
  }
};
