import { apiClient, manejarErrorHttp } from '../utils/apiClient';

export interface IConfiguracionComercial {
  razonSocial: string;
  porcentajeIva: number;
  diasValidez: number;
  logoBase64: string | null;
}

export const configuracionService = {
  obtener: async (): Promise<IConfiguracionComercial> => {
    const response = await apiClient('/configuracion-comercial');

    if (!response.ok) {
      await manejarErrorHttp(response);
    }

    return await response.json();
  },

  guardar: async (data: IConfiguracionComercial): Promise<void> => {
    const response = await apiClient('/configuracion-comercial', {
      method: 'PUT',
      body: JSON.stringify(data)
    });

    if (!response.ok) {
      try {
        const errorData = await response.clone().json();

        if (errorData.errors && errorData.errors.length > 0) {
          throw new Error(errorData.errors.join(' '));
        }

        if (errorData.errorMessage) {
          throw new Error(errorData.errorMessage);
        }
      } catch (e: any) {
        if (e.message && e.message !== 'Failed to parse URL from /api/configuracion-comercial') {
          throw e;
        }
      }

      await manejarErrorHttp(response);
    }
  }
};
