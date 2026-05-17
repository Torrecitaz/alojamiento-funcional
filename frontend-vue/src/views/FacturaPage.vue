<template>
  <div class="factura-page container">
    <div class="page-header">
      <h2>Factura / Comprobante</h2>
    </div>

    <div v-if="loading" class="loading-state">
      <div class="spinner"></div>
      <p>Cargando factura...</p>
    </div>

    <div v-else-if="error" class="error-state card">
      <p>{{ error }}</p>
      <router-link to="/mis-reservas" class="btn-secondary">Volver a Mis Reservas</router-link>
    </div>

    <div v-else-if="factura" class="factura-content card">
      <div class="factura-header">
        <h3>AlojamientoMR</h3>
        <p class="factura-id">Factura #{{ factura.facturaId }}</p>
      </div>

      <div class="factura-detalle">
        <div class="detalle-row">
          <span>Código Reserva:</span>
          <strong>{{ factura.codigoReserva }}</strong>
        </div>
        <div class="detalle-row">
          <span>Monto:</span>
          <strong>${{ factura.monto?.toFixed(2) }} {{ factura.moneda }}</strong>
        </div>
        <div class="detalle-row">
          <span>Método de Pago:</span>
          <strong>{{ factura.metodoPago }}</strong>
        </div>
        <div class="detalle-row">
          <span>Estado:</span>
          <span class="estado-badge" :class="factura.estado?.toLowerCase()">{{ factura.estado }}</span>
        </div>
        <div class="detalle-row" v-if="factura.fechaPago">
          <span>Fecha de Pago:</span>
          <strong>{{ new Date(factura.fechaPago).toLocaleDateString() }}</strong>
        </div>
        <div class="detalle-row" v-if="factura.fechaCreacion">
          <span>Fecha Creación:</span>
          <strong>{{ new Date(factura.fechaCreacion).toLocaleDateString() }}</strong>
        </div>
      </div>

      <div class="factura-footer">
        <p>Desarrollado por Mateo Torres © 2026</p>
        <router-link to="/mis-reservas" class="btn-secondary">Volver</router-link>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import api from '@/services/api';

const route = useRoute();
const factura = ref(null);
const loading = ref(true);
const error = ref(null);

const fetchFactura = async () => {
  loading.value = true;
  try {
    const res = await api.get(`/facturas/reserva/${route.params.codigo}`);
    factura.value = res.data.data || res.data;
  } catch (err) {
    error.value = err.response?.data?.message || 'Factura no encontrada para esta reserva.';
  } finally {
    loading.value = false;
  }
};

onMounted(fetchFactura);
</script>

<style scoped>
.page-header { text-align: center; margin-bottom: 2rem; }
.page-header h2 { font-size: 2rem; color: var(--color-primary-dark); }
.factura-content { max-width: 600px; margin: 0 auto; padding: 2.5rem; }
.factura-header { text-align: center; margin-bottom: 2rem; padding-bottom: 1.5rem; border-bottom: 2px solid var(--color-primary); }
.factura-header h3 { color: var(--color-primary-dark); font-size: 1.5rem; }
.factura-id { color: var(--color-text-secondary); font-family: monospace; }
.factura-detalle { display: flex; flex-direction: column; gap: 1rem; }
.detalle-row { display: flex; justify-content: space-between; align-items: center; padding: 0.75rem 0; border-bottom: 1px solid var(--color-border); }
.detalle-row span { color: var(--color-text-secondary); }
.estado-badge { padding: 0.25rem 0.75rem; border-radius: var(--radius-full); font-size: 0.8rem; font-weight: 600; }
.estado-badge.aprobado { background: #e8f5e9; color: #2e7d32; }
.estado-badge.pendiente { background: #fff3e0; color: #e65100; }
.estado-badge.rechazado { background: #ffebee; color: #c62828; }
.factura-footer { margin-top: 2rem; text-align: center; padding-top: 1.5rem; border-top: 1px solid var(--color-border); }
.factura-footer p { color: var(--color-text-secondary); font-size: 0.85rem; margin-bottom: 1rem; }
.loading-state, .error-state { text-align: center; padding: 4rem 0; }
.spinner { width: 40px; height: 40px; border: 4px solid var(--color-secondary); border-top-color: var(--color-primary-dark); border-radius: 50%; animation: spin 1s linear infinite; margin: 0 auto 1rem; }
@keyframes spin { to { transform: rotate(360deg); } }
</style>
