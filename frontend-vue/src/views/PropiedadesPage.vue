<template>
  <div class="propiedades-page container">
    <div class="page-header">
      <h2>Nuestras Propiedades</h2>
      <p>Encuentra el lugar perfecto para tu próxima estadía</p>
    </div>

    <div v-if="loading" class="loading-state">
      <div class="spinner"></div>
      <p>Cargando propiedades...</p>
    </div>
    
    <div v-else-if="error" class="error-state">
      <p>{{ error }}</p>
      <button @click="fetchPropiedades" class="btn-secondary">Reintentar</button>
    </div>

    <div v-else class="propiedades-grid">
      <div v-for="prop in propiedades" :key="prop.alojamientoId" class="card propiedad-card">
        <div class="propiedad-image-placeholder">
          <!-- Image placeholder for aesthetic -->
          <div class="image-overlay"></div>
        </div>
        <div class="propiedad-content">
          <div class="propiedad-header">
            <h3>{{ prop.nombre }}</h3>
            <span class="badge">{{ prop.tipoNombre || 'Alojamiento' }}</span>
          </div>
          <p class="propiedad-location">
            <i class="lucide-map-pin"></i> {{ prop.ciudad }}, {{ prop.pais }}
          </p>
          <p class="propiedad-desc">{{ prop.descripcion?.substring(0, 100) }}...</p>
          
          <div class="propiedad-footer">
            <div class="rating">
              <i class="lucide-star"></i> {{ prop.calificacion || 'Nuevo' }}
            </div>
            <router-link :to="`/propiedades/${prop.alojamientoId}`" class="btn-primary btn-sm">
              Ver Detalles
            </router-link>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import api from '@/services/api';

const propiedades = ref([]);
const loading = ref(true);
const error = ref(null);

const fetchPropiedades = async () => {
  loading.value = true;
  error.value = null;
  try {
    const res = await api.get('/alojamientos');
    // Map based on the ApiResponseListAlojamiento contract which wraps data
    propiedades.value = res.data.data || res.data;
  } catch (err) {
    console.error('Error fetching propiedades:', err);
    error.value = 'No se pudieron cargar las propiedades. Intenta nuevamente más tarde.';
  } finally {
    loading.value = false;
  }
};

onMounted(() => {
  fetchPropiedades();
});
</script>

<style scoped>
.page-header {
  margin-bottom: 3rem;
  text-align: center;
}

.page-header h2 {
  font-size: 2.5rem;
  color: var(--color-primary-dark);
}

.page-header p {
  color: var(--color-text-secondary);
  font-size: 1.1rem;
}

.loading-state, .error-state {
  text-align: center;
  padding: 4rem 0;
}

.spinner {
  width: 40px;
  height: 40px;
  border: 4px solid var(--color-secondary);
  border-top-color: var(--color-primary-dark);
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto 1rem;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.propiedades-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 2rem;
}

.propiedad-card {
  padding: 0;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.propiedad-image-placeholder {
  height: 200px;
  background: linear-gradient(135deg, var(--color-primary) 0%, var(--color-accent) 100%);
  position: relative;
}

.image-overlay {
  position: absolute;
  bottom: 0;
  left: 0;
  right: 0;
  height: 50%;
  background: linear-gradient(to top, rgba(0,0,0,0.1), transparent);
}

.propiedad-content {
  padding: 1.5rem;
  display: flex;
  flex-direction: column;
  flex: 1;
}

.propiedad-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 0.5rem;
}

.propiedad-header h3 {
  font-size: 1.25rem;
  margin-bottom: 0;
  color: var(--color-text-primary);
}

.badge {
  background-color: var(--color-secondary);
  color: var(--color-primary-dark);
  padding: 0.25rem 0.75rem;
  border-radius: var(--radius-full);
  font-size: 0.75rem;
  font-weight: 600;
}

.propiedad-location {
  color: var(--color-text-secondary);
  font-size: 0.875rem;
  margin-bottom: 1rem;
  display: flex;
  align-items: center;
  gap: 0.25rem;
}

.propiedad-desc {
  font-size: 0.9rem;
  color: var(--color-text-secondary);
  margin-bottom: 1.5rem;
  flex: 1;
}

.propiedad-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: auto;
  padding-top: 1rem;
  border-top: 1px solid var(--color-border);
}

.rating {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  font-weight: 600;
  color: #FFB300;
}
</style>
