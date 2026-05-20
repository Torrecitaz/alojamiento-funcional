import axios from 'axios';

// Usamos ruta relativa para que funcione tanto en local como en Render automáticamente
const API_BASE = '/api/v1';

const api = axios.create({
  baseURL: API_BASE,
  headers: { 'Content-Type': 'application/json' },
});

// Interceptor: inyectar JWT en cada petición autenticada
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('booking_token');
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
      localStorage.removeItem('booking_token');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default api;
