<template>
  <div class="checkout-page container">
    <div class="page-header">
      <h2>Pago de Reserva</h2>
      <p>Selecciona tu método de pago y confirma</p>
    </div>

    <div v-if="loading" class="loading-state">
      <div class="spinner"></div>
      <p>Cargando métodos de pago...</p>
    </div>

    <div v-else class="checkout-content">
      <div class="checkout-grid">
        <!-- Métodos de pago -->
        <div class="metodos-section card">
          <h3>Método de Pago</h3>
          <div class="metodos-list">
            <div
              v-for="m in metodosPago"
              :key="m.metodoPagoId"
              class="metodo-card"
              :class="{ selected: metodoSeleccionado === m.metodoPagoId }"
              @click="metodoSeleccionado = m.metodoPagoId"
            >
              <div class="metodo-icon">
                {{ m.tipo === 'CREDITO' ? '💳' : m.tipo === 'DEBITO' ? '🏦' : '🏨' }}
              </div>
              <div class="metodo-label">
                {{ m.tipo === 'CREDITO' ? 'Tarjeta de Crédito' : m.tipo === 'DEBITO' ? 'Tarjeta de Débito' : 'Pago en Sitio' }}
              </div>
            </div>
          </div>

          <div v-if="metodoSeleccionado && metodoSeleccionado !== 3" class="card-form">
            <div class="form-group">
              <label>Número de Tarjeta</label>
              <input type="text" placeholder="1234 5678 9012 3456" maxlength="19" />
            </div>
            <div class="form-row">
              <div class="form-group">
                <label>Vencimiento</label>
                <input type="text" placeholder="MM/AA" maxlength="5" />
              </div>
              <div class="form-group">
                <label>CVV</label>
                <input type="text" placeholder="123" maxlength="4" />
              </div>
            </div>
          </div>
        </div>

        <!-- Resumen -->
        <div class="resumen-section card">
          <h3>Resumen</h3>
          <div class="resumen-info">
            <p>Código de reserva: <strong>{{ $route.params.codigo }}</strong></p>
            <p class="metodo-selected" v-if="metodoSeleccionado">
              Método: <strong>{{ metodosPago.find(m => m.metodoPagoId === metodoSeleccionado)?.tipo }}</strong>
            </p>
          </div>
          <button
            class="btn-primary confirmar-btn"
            :disabled="!metodoSeleccionado || procesando"
            @click="confirmarPago"
          >
            {{ procesando ? 'Procesando...' : 'Confirmar Pago' }}
          </button>
          <div v-if="pagoError" class="error-message">{{ pagoError }}</div>
          <div v-if="pagoExito" class="success-message">
            <p>✅ ¡Pago procesado exitosamente!</p>
            <router-link to="/mis-reservas" class="btn-secondary" style="margin-top:1rem;display:inline-block;">Ver Mis Reservas</router-link>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import api from '@/services/api';

const metodosPago = ref([]);
const metodoSeleccionado = ref(null);
const loading = ref(true);
const procesando = ref(false);
const pagoError = ref(null);
const pagoExito = ref(false);

const fetchMetodos = async () => {
  try {
    const res = await api.get('/facturas/metodos-pago');
    metodosPago.value = res.data.data || res.data || [];
  } catch (err) {
    console.error('Error fetching metodos:', err);
    metodosPago.value = [
      { metodoPagoId: 1, tipo: 'CREDITO' },
      { metodoPagoId: 2, tipo: 'DEBITO' },
      { metodoPagoId: 3, tipo: 'EnSitio' }
    ];
  } finally {
    loading.value = false;
  }
};

const confirmarPago = async () => {
  procesando.value = true;
  pagoError.value = null;
  try {
    // Simulate payment confirmation
    await new Promise(r => setTimeout(r, 1500));
    pagoExito.value = true;
  } catch (err) {
    pagoError.value = 'Error al procesar el pago.';
  } finally {
    procesando.value = false;
  }
};

onMounted(fetchMetodos);
</script>

<style scoped>
.page-header { text-align: center; margin-bottom: 2rem; }
.page-header h2 { font-size: 2rem; color: var(--color-primary-dark); }
.page-header p { color: var(--color-text-secondary); }
.checkout-grid { display: grid; grid-template-columns: 1fr 380px; gap: 2rem; }
.metodos-section, .resumen-section { padding: 2rem; }
.metodos-section h3, .resumen-section h3 { color: var(--color-primary-dark); margin-bottom: 1.5rem; }
.metodos-list { display: flex; gap: 1rem; margin-bottom: 1.5rem; }
.metodo-card { flex: 1; padding: 1.25rem; border: 2px solid var(--color-border); border-radius: var(--radius-md); cursor: pointer; text-align: center; transition: all 0.2s; }
.metodo-card:hover { border-color: var(--color-primary); }
.metodo-card.selected { border-color: var(--color-primary-dark); background: var(--color-secondary); }
.metodo-icon { font-size: 2rem; margin-bottom: 0.5rem; }
.metodo-label { font-size: 0.85rem; font-weight: 500; }
.card-form { padding-top: 1rem; border-top: 1px solid var(--color-border); }
.form-group { margin-bottom: 1rem; }
.form-group label { display: block; margin-bottom: 0.5rem; font-weight: 500; font-size: 0.9rem; }
.form-row { display: flex; gap: 1rem; }
.form-row .form-group { flex: 1; }
.resumen-info { margin-bottom: 1.5rem; }
.resumen-info p { margin-bottom: 0.5rem; }
.confirmar-btn { width: 100%; }
.error-message { margin-top: 1rem; color: #e53935; background: #ffebee; padding: 0.75rem; border-radius: var(--radius-sm); text-align: center; }
.success-message { margin-top: 1rem; color: #2e7d32; background: #e8f5e9; padding: 1.5rem; border-radius: var(--radius-sm); text-align: center; }
.loading-state { text-align: center; padding: 4rem 0; }
.spinner { width: 40px; height: 40px; border: 4px solid var(--color-secondary); border-top-color: var(--color-primary-dark); border-radius: 50%; animation: spin 1s linear infinite; margin: 0 auto 1rem; }
@keyframes spin { to { transform: rotate(360deg); } }
@media (max-width: 768px) { .checkout-grid { grid-template-columns: 1fr; } .metodos-list { flex-direction: column; } }
</style>
