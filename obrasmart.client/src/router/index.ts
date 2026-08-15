import { createRouter, createWebHistory } from 'vue-router';
import { useAuthStore } from '../stores/authStore'; // Importar el store

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/registro',
      name: 'Registro',
      component: () => import('../views/auth/Registro.vue')
    },
    {
      path: '/login',
      name: 'Login',
      component: () => import('../views/auth/Login.vue')
    },
    {
      path: '/',
      component: () => import('../layouts/MainLayout.vue'),
      meta: { requiresAuth: true },
      children: [
        {
          path: '',
          name: 'Dashboard',
          component: () => import('../views/dashboard/Dashboard.vue'),
        },
        {
          path: 'clientes',
          name: 'Clientes',
          component: () => import('../views/clientes/Clientes.vue'),
        },
        {
          path: 'insumos',
          name: 'Insumos',
          component: () => import('../views/insumos/Insumos.vue'),
        },
        {
          path: 'insumos/precios',
          name: 'ActualizacionPrecios',
          component: () => import('../views/insumos/ActualizacionPrecios.vue'),
        },
        {
          path: 'apus',
          name: 'Apus',
          component: () => import('../views/apus/Apus.vue'),
        },
        {
          path: 'apus/crear',
          name: 'CrearApu',
          component: () => import('../views/apus/ApuForm.vue'),
        },
        {
          path: 'apus/editar/:id',
          name: 'EditarApu',
          component: () => import('../views/apus/ApuForm.vue'),
        },
        {
          path: 'presupuestos',
          name: 'Presupuestos',
          component: () => import('../views/presupuestos/Presupuestos.vue'),
        },
        {
          path: 'presupuestos/crear',
          name: 'CrearPresupuesto',
          component: () => import('../views/presupuestos/PresupuestoForm.vue'),
        },
        {
          path: 'presupuestos/editar/:id',
          name: 'EditarPresupuesto',
          component: () => import('../views/presupuestos/PresupuestoForm.vue'),
        },
        {
          path: 'cotizaciones',
          name: 'Cotizaciones',
          component: () => import('../views/cotizaciones/Cotizaciones.vue'),
        },
        {
          path: 'configuracion',
          name: 'ConfiguracionComercial',
          component: () => import('../views/configuracion/ConfiguracionComercial.vue'),
        }
      ]
    },
  ]
});

router.beforeEach((to, from) => {
  const authStore = useAuthStore();

  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    return '/login';
  } else if ((to.path === '/login' || to.path === '/registro') && authStore.isAuthenticated) {
    return '/';
  }
});

export default router;
