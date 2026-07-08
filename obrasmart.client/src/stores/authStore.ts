import { defineStore } from 'pinia';
import { ref, computed } from 'vue';

function decodificarJwt(token: string) {
  try {
    const base64Url = token.split('.')[1] ?? '';
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function (c) {
      return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(''));
    return JSON.parse(jsonPayload);
  } catch (e) {
    return null;
  }
}

export const useAuthStore = defineStore('auth', () => {
  // Estado
  const token = ref<string | null>(localStorage.getItem('token'));

  // Getters
  const isAuthenticated = computed(() => !!token.value);

  // Extrae la RazonSocial del token
  const usuarioNombre = computed(() => {
    if (!token.value) return 'Usuario';
    const payload = decodificarJwt(token.value);
    return payload?.RazonSocial || payload?.unique_name || payload?.name || 'Usuario';
  });

  // Acciones
  const setToken = (newToken: string) => {
    token.value = newToken;
    localStorage.setItem('token', newToken);
  };

  const logout = () => {
    token.value = null;
    localStorage.removeItem('token');
    window.location.href = '/login'; // Limpiamos la ruta y recargamos
  };

  return {
    token,
    isAuthenticated,
    usuarioNombre,
    setToken,
    logout
  };
});
