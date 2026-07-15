import { useAuthStore } from '../stores/authStore';

export const apiClient = async (endpoint: string, options: RequestInit = {}): Promise<Response> => {
  const headers = new Headers(options.headers || {});

  // Obtenemos la instancia de Pinia en tiempo de ejecución
  const authStore = useAuthStore();

  if (authStore.token) {
    headers.set('Authorization', `Bearer ${authStore.token}`);
  }

  if (!headers.has('Content-Type') && !(options.body instanceof FormData)) {
    headers.set('Content-Type', 'application/json');
  }

  const config: RequestInit = {
    ...options,
    headers
  };

  const response = await fetch(`/api${endpoint}`, config);

  // Interceptar tokens expirados
  if (response.status === 401) {
    authStore.logout();
    throw new Error('Sesión expirada. Por favor, inicia sesión nuevamente.');
  }

  return response;
};

export const manejarErrorHttp = async (response: Response): Promise<never> => {
  let errorMsg = 'Ocurrió un error inesperado en el servidor.';
  try {
    const data = await response.json();
    if (data && data.error) errorMsg = data.error;
    else if (data && data.Error) errorMsg = data.Error;
  } catch { }
  throw new Error(errorMsg);
};
