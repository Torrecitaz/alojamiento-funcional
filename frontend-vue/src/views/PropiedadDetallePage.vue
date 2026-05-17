<template>
  <div class="detalle-page container">
    <div v-if="loading" class="loading-state">
      <div class="spinner"></div>
      <p>Cargando detalles del alojamiento...</p>
    </div>

    <div v-else-if="error" class="error-state">
      <p>{{ error }}</p>
      <router-link to="/propiedades" class="btn-secondary">Volver a Propiedades</router-link>
    </div>

    <div v-else-if="alojamiento" class="detalle-content">
      <router-link to="/propiedades" class="back-link">← Volver a Propiedades</router-link>

      <div class="detalle-hero">
        <div class="hero-image"></div>
        <div class="hero-info card">
          <h1>{{ alojamiento.nombre }}</h1>
          <p class="ubicacion">📍 {{ alojamiento.ciudad }}, {{ alojamiento.direccion }}</p>
          <div class="meta-badges">
            <span class="badge" v-if="alojamiento.tipoAlojamiento">{{ alojamiento.tipoAlojamiento }}</span>
            <span class="badge" v-if="alojamiento.estrellas">⭐ {{ alojamiento.estrellas }} estrellas</span>
            <span class="badge" v-if="alojamiento.tienePiscina">🏊 Piscina</span>
            <span class="badge" v-if="alojamiento.admiteMascotas">🐾 Mascotas</span>
            <span class="badge" v-if="alojamiento.tieneParqueadero">🅿️ Parqueadero</span>
          </div>
          <p class="descripcion">{{ alojamiento.descripcion || 'Sin descripción disponible.' }}</p>
          <div class="estado-tag" :class="alojamiento.estado?.toLowerCase()">{{ alojamiento.estado }}</div>
        </div>
      </div>

      <!-- Disponibilidad -->
      <div class="disponibilidad-section card">
        <h2>Consultar Disponibilidad</h2>
        <div class="form-row">
          <div class="form-group">
            <label>Fecha Check-in</label>
            <input type="date" v-model="fechaDesde" />
          </div>
          <div class="form-group">
            <label>Fecha Check-out</label>
            <input type="date" v-model="fechaHasta" />
          </div>
          <div class="form-group">
            <label>Adultos</label>
            <input type="number" v-model.number="adultos" min="1" max="10" />
          </div>
          <button class="btn-primary" @click="checkDisponibilidad" :disabled="checkingDispo">
            {{ checkingDispo ? 'Consultando...' : 'Buscar Habitaciones' }}
          </button>
        </div>

        <div v-if="habitaciones.length" class="habitaciones-list">
          <h3>Habitaciones Disponibles ({{ habitaciones.length }})</h3>
          <div v-for="hab in habitaciones" :key="hab.habitacionId" class="habitacion-card card">
            <div class="hab-info">
              <h4>{{ hab.nombre }}</h4>
              <p>{{ hab.descripcion || 'Habitación confortable' }}</p>
              <div class="hab-features">
                <span v-if="hab.capacidadAdultos">👤 {{ hab.capacidadAdultos }} adultos</span>
                <span v-if="hab.capacidadNinos">👶 {{ hab.capacidadNinos }} niños</span>
                <span v-if="hab.tieneCocina">🍳 Cocina</span>
                <span v-if="hab.tieneAireAcondicionado">❄️ A/C</span>
              </div>
            </div>
            <div class="hab-price">
              <p class="price">${{ hab.precioNoche?.toFixed(2) }}<small>/noche</small></p>
              <p class="price-total" v-if="hab.precioTotal">Total: ${{ hab.precioTotal?.toFixed(2) }}</p>
            </div>
          </div>
        </div>

        <div v-if="dispoError" class="error-message">{{ dispoError }}</div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import api from '@/services/api';

const route = useRoute();
const alojamiento = ref(null);
const loading = ref(true);
const error = ref(null);

const fechaDesde = ref('');
const fechaHasta = ref('');
const adultos = ref(2);
const habitaciones = ref([]);
const checkingDispo = ref(false);
const dispoError = ref(null);

const fetchDetalle = async () => {
  loading.value = true;
  try {
    const res = await api.get(`/alojamientos/${route.params.id}`);
    alojamiento.value = res.data.data || res.data;
  } catch (err) {
    error.value = 'No se pudo cargar el alojamiento.';
  } finally {
    loading.value = false;
  }
};

const checkDisponibilidad = async () => {
  if (!fechaDesde.value || !fechaHasta.value) {
    dispoError.value = 'Selecciona ambas fechas.';
    return;
  }
  checkingDispo.value = true;
  dispoError.value = null;
  try {
    const res = await api.get(`/alojamientos/${route.params.id}/disponibilidad`, {
      params: { fechaDesde: fechaDesde.value, fechaHasta: fechaHasta.value, adultos: adultos.value }
    });
    const data = res.data.data || res.data;
    habitaciones.value = data.habitacionesDisponibles || data.habitaciones || [];
    if (!habitaciones.value.length) dispoError.value = 'No hay habitaciones disponibles para esas fechas.';
  } catch (err) {
    dispoError.value = err.response?.data?.message || 'Error al consultar disponibilidad.';
  } finally {
    checkingDispo.value = false;
  }
};

onMounted(fetchDetalle);
</script>

<style scoped>
.back-link { display: inline-block; margin-bottom: 1.5rem; color: var(--color-primary-dark); text-decoration: none; font-weight: 500; }
.back-link:hover { text-decoration: underline; }
.detalle-hero { display: grid; grid-template-columns: 1fr 1fr; gap: 2rem; margin-bottom: 2rem; }
.hero-image { background: linear-gradient(135deg, var(--color-primary) 0%, var(--color-accent) 100%); border-radius: var(--radius-lg); min-height: 300px; }
.hero-info { padding: 2rem; }
.hero-info h1 { font-size: 2rem; color: var(--color-primary-dark); margin-bottom: 0.5rem; }
.ubicacion { color: var(--color-text-secondary); margin-bottom: 1rem; }
.meta-badges { display: flex; flex-wrap: wrap; gap: 0.5rem; margin-bottom: 1rem; }
.badge { background-color: var(--color-secondary); color: var(--color-primary-dark); padding: 0.25rem 0.75rem; border-radius: var(--radius-full); font-size: 0.8rem; font-weight: 500; }
.descripcion { color: var(--color-text-secondary); line-height: 1.6; margin-bottom: 1rem; }
.estado-tag { display: inline-block; padding: 0.25rem 1rem; border-radius: var(--radius-full); font-size: 0.8rem; font-weight: 600; }
.estado-tag.pendiente { background: #fff3e0; color: #e65100; }
.estado-tag.activo { background: #e8f5e9; color: #2e7d32; }
.disponibilidad-section { padding: 2rem; }
.disponibilidad-section h2 { color: var(--color-primary-dark); margin-bottom: 1.5rem; }
.form-row { display: flex; gap: 1rem; align-items: flex-end; flex-wrap: wrap; margin-bottom: 1.5rem; }
.form-row .form-group { flex: 1; min-width: 150px; }
.form-row .form-group label { display: block; margin-bottom: 0.5rem; font-weight: 500; }
.habitaciones-list { margin-top: 1.5rem; }
.habitaciones-list h3 { margin-bottom: 1rem; color: var(--color-primary-dark); }
.habitacion-card { display: flex; justify-content: space-between; align-items: center; padding: 1.5rem; margin-bottom: 1rem; }
.hab-features { display: flex; gap: 1rem; margin-top: 0.5rem; font-size: 0.85rem; color: var(--color-text-secondary); }
.hab-price { text-align: right; }
.price { font-size: 1.5rem; font-weight: 700; color: var(--color-primary-dark); }
.price small { font-size: 0.8rem; font-weight: 400; }
.price-total { font-size: 0.9rem; color: var(--color-text-secondary); }
.loading-state, .error-state { text-align: center; padding: 4rem 0; }
.spinner { width: 40px; height: 40px; border: 4px solid var(--color-secondary); border-top-color: var(--color-primary-dark); border-radius: 50%; animation: spin 1s linear infinite; margin: 0 auto 1rem; }
@keyframes spin { to { transform: rotate(360deg); } }
.error-message { margin-top: 1rem; color: #e53935; background: #ffebee; padding: 0.75rem; border-radius: var(--radius-sm); text-align: center; }
@media (max-width: 768px) { .detalle-hero { grid-template-columns: 1fr; } }
</style>
