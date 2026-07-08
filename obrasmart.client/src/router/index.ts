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
        // {
        //   // A futuro: /presupuestos
        //   path: 'presupuestos',
        //   name: 'Presupuestos',
        //   component: () => import('../views/Dashboard.vue'), // Placeholder por ahora
        // }
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
