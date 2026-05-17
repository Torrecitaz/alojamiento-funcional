<template>
  <div class="login-page container">
    <div class="login-container card">
      <div class="login-header">
        <h2>Bienvenido a <span class="highlight">AlojamientoMR</span></h2>
        <p>Ingresa tus credenciales para continuar</p>
      </div>

      <form @submit.prevent="handleLogin" class="login-form">
        <div class="form-group">
          <label for="email">Correo Electrónico</label>
          <input 
            type="email" 
            id="email" 
            v-model="email" 
            required 
            placeholder="ejemplo@correo.com"
          />
        </div>

        <div class="form-group">
          <label for="password">Contraseña</label>
          <input 
            type="password" 
            id="password" 
            v-model="password" 
            required 
            placeholder="********"
          />
        </div>

        <button type="submit" class="btn-primary login-btn" :disabled="loading">
          {{ loading ? 'Iniciando sesión...' : 'Ingresar' }}
        </button>

        <div v-if="error" class="error-message">
          {{ error }}
        </div>
      </form>

      <div class="login-footer">
        <p>¿No tienes una cuenta? <router-link to="/register">Regístrate aquí</router-link></p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import { useAuthStore } from '@/store/auth';
import { useRouter } from 'vue-router';

const email = ref('');
const password = ref('');
const loading = ref(false);
const error = ref(null);

const authStore = useAuthStore();
const router = useRouter();

const handleLogin = async () => {
  loading.value = true;
  error.value = null;
  
  try {
    await authStore.login({ email: email.value, password: password.value });
    router.push(authStore.isAdmin ? '/admin' : '/');
  } catch (err) {
    error.value = 'Credenciales inválidas. Por favor, intenta nuevamente.';
    console.error('Error logging in:', err);
  } finally {
    loading.value = false;
  }
};
</script>

<style scoped>
.login-page {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: calc(100vh - 200px);
}

.login-container {
  width: 100%;
  max-width: 450px;
  padding: 3rem 2rem;
}

.login-header {
  text-align: center;
  margin-bottom: 2rem;
}

.login-header h2 {
  font-size: 1.75rem;
}

.login-header p {
  color: var(--color-text-secondary);
  margin-top: 0.5rem;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
  color: var(--color-text-primary);
}

.login-btn {
  width: 100%;
  margin-top: 1rem;
}

.error-message {
  margin-top: 1rem;
  color: #e53935;
  background-color: #ffebee;
  padding: 0.75rem;
  border-radius: var(--radius-sm);
  text-align: center;
  font-size: 0.875rem;
}

.login-footer {
  margin-top: 2rem;
  text-align: center;
  font-size: 0.9rem;
}
</style>
