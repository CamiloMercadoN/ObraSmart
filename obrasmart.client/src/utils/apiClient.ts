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

export const manejarErrorHttp = async (response: Response) => {
  if (response.status === 400) {
    try {
      const data = await response.json();
      // Buscamos 'Error' o 'error' según cómo lo serialice el backend
      const mensaje = data.Error || data.error || data.errorMessage || "Error de validación en la solicitud.";
      throw new Error(mensaje);
    } catch (e) {
      // Si falla al parsear el JSON, lanzamos un error por defecto
      throw new Error("Error en la solicitud. Verifica los datos enviados.");
    }
  }

  if (response.status === 401) throw new Error("No autorizado. Inicia sesión nuevamente.");
  if (response.status === 403) throw new Error("No tienes permisos para esta acción.");
  if (response.status === 404) throw new Error("Recurso no encontrado.");

  throw new Error("Ocurrió un error inesperado en el servidor.");
};
