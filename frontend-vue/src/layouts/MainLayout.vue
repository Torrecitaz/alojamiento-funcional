<template>
  <div class="main-layout">
    <header class="navbar glass">
      <div class="container navbar-container">
        <router-link to="/" class="brand">
          <span class="brand-text">AlojamientoMR</span>
        </router-link>
        
        <nav class="nav-links">
          <router-link to="/">Inicio</router-link>
          <router-link to="/propiedades">Propiedades</router-link>
          
          <template v-if="authStore.isAuthenticated">
            <router-link to="/mis-reservas">Mis Reservas</router-link>
            <router-link v-if="authStore.isAdmin" to="/admin">Admin Dashboard</router-link>
            
            <div class="user-menu">
              <span class="user-greeting">Hola, {{ authStore.user?.nombre }}</span>
              <button @click="logout" class="btn-secondary btn-sm">Salir</button>
            </div>
          </template>
          <template v-else>
            <router-link to="/login" class="btn-secondary">Ingresar</router-link>
            <router-link to="/register" class="btn-primary">Registrarse</router-link>
          </template>
        </nav>
      </div>
    </header>

    <main class="main-content">
      <router-view v-slot="{ Component }">
        <transition name="fade" mode="out-in">
          <component :is="Component" />
        </transition>
      </router-view>
    </main>

    <footer class="footer">
      <div class="container footer-content">
        <div class="footer-brand">
          <h3>AlojamientoMR</h3>
          <p>Encuentra tu lugar ideal con diseño moderno y minimalista.</p>
        </div>
        <div class="footer-links">
          <h4>Enlaces Rápidos</h4>
          <router-link to="/">Inicio</router-link>
          <router-link to="/propiedades">Propiedades</router-link>
        </div>
        <div class="footer-credits">
          <p>Desarrollado por Mateo Torres &copy; 2026</p>
        </div>
      </div>
    </footer>
  </div>
</template>

<script setup>
import { useAuthStore } from '@/store/auth';
import { useRouter } from 'vue-router';

const authStore = useAuthStore();
const router = useRouter();

const logout = () => {
  authStore.logout();
  router.push('/login');
};
</script>

<style scoped>
.main-layout {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
}

.navbar {
  position: sticky;
  top: 0;
  z-index: 100;
  padding: 1rem 0;
  border-bottom: 1px solid var(--color-border);
}

.navbar-container {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.brand {
  font-size: 1.5rem;
  font-weight: 700;
  color: var(--color-primary-dark);
}

.nav-links {
  display: flex;
  align-items: center;
  gap: 1.5rem;
}

.nav-links a {
  font-weight: 500;
}

.nav-links a:not(.btn-primary):not(.btn-secondary) {
  color: var(--color-text-primary);
}

.nav-links a:hover:not(.btn-primary):not(.btn-secondary) {
  color: var(--color-primary-dark);
}

.user-menu {
  display: flex;
  align-items: center;
  gap: 1rem;
  border-left: 1px solid var(--color-border);
  padding-left: 1.5rem;
}

.user-greeting {
  font-weight: 600;
  color: var(--color-primary-dark);
}

.btn-sm {
  padding: 0.5rem 1rem;
  font-size: 0.875rem;
}

.main-content {
  flex: 1;
  padding-top: 2rem;
  padding-bottom: 4rem;
}

.footer {
  background-color: var(--color-secondary);
  padding: 3rem 0;
  margin-top: auto;
}

.footer-content {
  display: grid;
  grid-template-columns: 1fr 1fr 1fr;
  gap: 2rem;
}

.footer h3, .footer h4 {
  color: var(--color-primary-dark);
  margin-bottom: 1rem;
}

.footer p {
  color: var(--color-text-secondary);
}

.footer-links {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.footer-links a {
  color: var(--color-text-secondary);
}

.footer-links a:hover {
  color: var(--color-primary-dark);
}

@media (max-width: 768px) {
  .nav-links {
    display: none; /* simple responsive for now */
  }
  .footer-content {
    grid-template-columns: 1fr;
  }
}
</style>
