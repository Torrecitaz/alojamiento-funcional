import axios from 'axios';

// Usamos variable de entorno si está configurada, de lo contrario ruta relativa
const API_BASE = import.meta.env.VITE_API_BASE_URL || '/api/v1';

const api = axios.create({
  baseURL: API_BASE,
  headers: { 'Content-Type': 'application/json' },
});

// Interceptor: inyectar JWT en cada petición autenticada
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('alojaexpress_token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Interceptor de respuesta: manejo centralizado de errores
api.interceptors.response.use(
  (res) => res,
  (error) => {
    const status = error.response?.status;
    if (status === 401) {
      localStorage.removeItem('alojaexpress_token');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default api;
