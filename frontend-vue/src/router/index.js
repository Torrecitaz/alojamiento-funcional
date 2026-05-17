import { createRouter, createWebHistory } from 'vue-router';
import { useAuthStore } from '@/store/auth';

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      component: () => import('@/layouts/MainLayout.vue'),
      children: [
        { path: '', name: 'home', component: () => import('@/views/HomePage.vue') },
        { path: 'login', name: 'login', component: () => import('@/views/LoginPage.vue') },
        { path: 'register', name: 'register', component: () => import('@/views/RegisterPage.vue') },
        { path: 'propiedades', name: 'propiedades', component: () => import('@/views/PropiedadesPage.vue') },
        { path: 'propiedades/:id', name: 'propiedadDetalle', component: () => import('@/views/PropiedadDetallePage.vue') },
        { path: 'mis-reservas', name: 'misReservas', component: () => import('@/views/MisReservasPage.vue'), meta: { requiresAuth: true } },
        { path: 'checkout/:codigo', name: 'checkout', component: () => import('@/views/CheckoutPage.vue'), meta: { requiresAuth: true } },
        { path: 'factura/:codigo', name: 'factura', component: () => import('@/views/FacturaPage.vue'), meta: { requiresAuth: true } },
      ]
    },
    {
      path: '/admin',
      component: () => import('@/views/admin/AdminLayout.vue'),
      meta: { requiresAdmin: true },
      children: [
        { path: '', name: 'adminDashboard', component: () => import('@/views/admin/AdminDashboard.vue') },
        { path: 'propiedades', name: 'adminPropiedades', component: () => import('@/views/admin/AdminPropiedades.vue') },
        { path: 'habitaciones', name: 'adminHabitaciones', component: () => import('@/views/admin/AdminHabitaciones.vue') },
        { path: 'usuarios', name: 'adminUsuarios', component: () => import('@/views/admin/AdminUsuarios.vue') },
        { path: 'colaboradores', name: 'adminColaboradores', component: () => import('@/views/admin/AdminColaboradores.vue') },
        { path: 'reservas', name: 'adminReservas', component: () => import('@/views/admin/AdminReservas.vue') },
      ]
    }
  ]
});

router.beforeEach((to, from, next) => {
  const authStore = useAuthStore();
  
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    next({ name: 'login' });
  } else if (to.meta.requiresAdmin && !authStore.isAdmin) {
    next({ name: 'home' });
  } else {
    next();
  }
});

export default router;
