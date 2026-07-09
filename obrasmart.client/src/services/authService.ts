import { useAuthStore } from '../stores/authStore';

const API_URL = '/api/Auth';

export interface LoginResponse {
  token: string;
}

export interface RegistroPayload {
  correo: string;
  password: string;
  razonSocial: string;
  rut: string;
}

export const authService = {
  async registro(payload: RegistroPayload): Promise<void> {
    const response = await fetch(`${API_URL}/registro`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(payload)
    });

    if (!response.ok) {
      let errorMessage = 'Error al registrar el usuario';
      try {
        const errorData = await response.json();
        if (errorData && errorData.error) {
          errorMessage = errorData.error;
        }
      } catch {
        // Failsafe por si el servidor devuelve un error no JSON (ej. 500 IIS Error)
      }
      throw new Error(errorMessage);
    }
  },

  async login(correo: string, password: string): Promise<LoginResponse> {
    const response = await fetch(`${API_URL}/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ correo, password })
    });

    if (!response.ok) {
      throw new Error('Credenciales inválidas');
    }

    const data: LoginResponse = await response.json();

    // Delegamos el almacenamiento y estado a Pinia
    const authStore = useAuthStore();
    authStore.setToken(data.token);

    return data;
  }
};
