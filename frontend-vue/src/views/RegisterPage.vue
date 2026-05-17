<template>
  <div class="register-page container">
    <div class="register-container card">
      <div class="register-header">
        <h2>Crear Cuenta en <span class="highlight">AlojamientoMR</span></h2>
        <p>Regístrate para empezar a reservar</p>
      </div>

      <div v-if="success" class="success-message">
        <p>✅ ¡Registro exitoso! Ahora puedes iniciar sesión.</p>
        <router-link to="/login" class="btn-primary" style="display:inline-block;margin-top:1rem;">Ir a Login</router-link>
      </div>

      <form v-else @submit.prevent="handleRegister" class="register-form">
        <div class="form-group">
          <label for="nombreCompleto">Nombre Completo</label>
          <input type="text" id="nombreCompleto" v-model="form.nombreCompleto" required placeholder="Juan Pérez" />
        </div>

        <div class="form-group">
          <label for="email">Correo Electrónico</label>
          <input type="email" id="email" v-model="form.email" required placeholder="ejemplo@correo.com" />
        </div>

        <div class="form-group">
          <label for="password">Contraseña</label>
          <input type="password" id="password" v-model="form.password" required placeholder="Mínimo 8 caracteres" minlength="8" />
        </div>

        <div class="form-row">
          <div class="form-group">
            <label for="cedula">Cédula</label>
            <input type="text" id="cedula" v-model="form.cedula" required placeholder="1712345678" pattern="\d{10}" title="10 dígitos numéricos" />
          </div>
          <div class="form-group">
            <label for="telefono">Teléfono</label>
            <input type="text" id="telefono" v-model="form.telefono" required placeholder="0991234567" pattern="\d+" />
          </div>
        </div>

        <div class="form-group">
          <label for="domicilio">Domicilio</label>
          <input type="text" id="domicilio" v-model="form.domicilio" required placeholder="Av. Principal 123, Quito" />
        </div>

        <button type="submit" class="btn-primary register-btn" :disabled="loading">
          {{ loading ? 'Registrando...' : 'Crear Cuenta' }}
        </button>

        <div v-if="error" class="error-message">
          {{ error }}
        </div>
      </form>

      <div v-if="!success" class="register-footer">
        <p>¿Ya tienes una cuenta? <router-link to="/login">Inicia sesión</router-link></p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue';
import api from '@/services/api';

const form = reactive({
  nombreCompleto: '',
  email: '',
  password: '',
  cedula: '',
  telefono: '',
  domicilio: ''
});

const loading = ref(false);
const error = ref(null);
const success = ref(false);

const handleRegister = async () => {
  loading.value = true;
  error.value = null;

  try {
    await api.post('/usuarios/clientes/registrar', form);
    success.value = true;
  } catch (err) {
    const msg = err.response?.data?.message || err.response?.data?.errors?.[0] || 'Error al registrar. Verifica los datos e intenta nuevamente.';
    error.value = msg;
    console.error('Error registering:', err);
  } finally {
    loading.value = false;
  }
};
</script>

<style scoped>
.register-page {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: calc(100vh - 200px);
  padding: 2rem 1rem;
}

.register-container {
  width: 100%;
  max-width: 520px;
  padding: 3rem 2rem;
}

.register-header {
  text-align: center;
  margin-bottom: 2rem;
}

.register-header h2 {
  font-size: 1.75rem;
}

.register-header p {
  color: var(--color-text-secondary);
  margin-top: 0.5rem;
}

.form-group {
  margin-bottom: 1.25rem;
  flex: 1;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
  color: var(--color-text-primary);
}

.form-row {
  display: flex;
  gap: 1rem;
}

.register-btn {
  width: 100%;
  margin-top: 0.5rem;
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

.success-message {
  text-align: center;
  padding: 2rem;
  color: #2e7d32;
  background: #e8f5e9;
  border-radius: var(--radius-md);
}

.register-footer {
  margin-top: 2rem;
  text-align: center;
  font-size: 0.9rem;
}
</style>
