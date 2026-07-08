<template>
  <div class="min-h-screen flex flex-column surface-100">

    <div class="flex justify-content-between align-items-center px-4 py-3 surface-0 shadow-1">
      <div class="flex align-items-center gap-3">
        <Button icon="pi pi-bars" text rounded aria-label="Menú" @click="menuVisible = true" />
        <span class="text-xl font-bold text-primary">ObraSmart</span>
      </div>

      <div class="flex align-items-center gap-2">
        <Button :icon="isDark ? 'pi pi-moon' : 'pi pi-sun'" text rounded aria-label="Cambiar Tema" @click="toggleTheme" />

        <Button icon="pi pi-sign-out" text rounded severity="danger" aria-label="Cerrar Sesión" @click="cerrarSesion" />
      </div>
    </div>

    <Sidebar v-model:visible="menuVisible" header="Menú Principal" class="w-20rem">
      <Menu :model="menuItems" class="w-full border-none" />
    </Sidebar>

    <div class="flex-grow-1 p-3 md:p-4">
      <router-view />
    </div>

  </div>
</template>

<script setup lang="ts">import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '../stores/authStore';

import Sidebar from 'primevue/sidebar';
import Menu from 'primevue/menu';
import Button from 'primevue/button';

const router = useRouter();
const authStore = useAuthStore();
const menuVisible = ref(false);

  const isDark = ref(window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches);

  // Si el sistema está en oscuro, inyectamos la clase inmediatamente
  if (isDark.value) {
    document.documentElement.classList.add('app-dark');
  }

  // Función para alternar manualmente
  const toggleTheme = () => {
    isDark.value = !isDark.value;
    document.documentElement.classList.toggle('app-dark');
  };

// Definición de las opciones del menú
const menuItems = ref([
    {
        label: 'Inicio',
        icon: 'pi pi-home',
        command: () => { router.push('/'); menuVisible.value = false; }
    },
    {
        label: 'Presupuestos',
        icon: 'pi pi-file-edit',
        command: () => { router.push('/presupuestos'); menuVisible.value = false; }
    },
    {
        label: 'Clientes',
        icon: 'pi pi-users',
        command: () => { router.push('/clientes'); menuVisible.value = false; }
    },
    { separator: true }, // Línea divisoria visual
    {
        label: 'Configuración',
        icon: 'pi pi-cog',
        command: () => { router.push('/configuracion'); menuVisible.value = false; }
    }
]);

const cerrarSesion = () => {
    authStore.logout();
};</script>

<style scoped>
  /* Removemos bordes extraños que el Menu de PrimeVue trae por defecto */
  :deep(.p-menu) {
    background: transparent;
  }

  :deep(.p-menuitem-link) {
    padding: 1rem;
    border-radius: var(--border-radius);
  }
</style>
