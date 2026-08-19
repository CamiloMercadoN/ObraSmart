import { apiClient, manejarErrorHttp } from '../utils/apiClient';

const BASE_URL = '/Dashboard';

export const dashboardService = {
  async obtenerResumen(): Promise<any> {
    const response = await apiClient(BASE_URL);
    if (!response.ok) await manejarErrorHttp(response);

    const result = await response.json();
    return result.data;
  }
};
