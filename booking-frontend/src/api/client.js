import axios from 'axios';

const BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api/v2';
const client = axios.create({ baseURL: BASE_URL, timeout: 90000 }); // 90 segundos para tolerar el cold start de Render Free Tier

// Inyectar JWT en cada request
// Inyectar JWT y cabecera de idempotencia en peticiones
client.interceptors.request.use((config) => {
  const token = localStorage.getItem('alojaexpress_token');
  if (token) config.headers.Authorization = `Bearer ${token}`;

  // Si es una petición POST y no tiene cabecera de idempotencia, generar una automáticamente
  if (config.method?.toLowerCase() === 'post') {
    const hasIdempotency = config.headers['X-Idempotency-Key'] || config.headers['Idempotency-Key'];
    if (!hasIdempotency) {
      const uuid = typeof crypto !== 'undefined' && crypto.randomUUID
        ? crypto.randomUUID()
        : 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
            const r = (Math.random() * 16) | 0;
            const v = c === 'x' ? r : (r & 0x3) | 0x8;
            return v.toString(16);
          });
      config.headers['X-Idempotency-Key'] = uuid;
      config.headers['Idempotency-Key'] = uuid;
    }
  }

  return config;
});

// Manejo centralizado de errores
client.interceptors.response.use(
  (res) => res,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('alojaexpress_token');
      localStorage.removeItem('alojaexpress_user');
      window.location.href = '/login';
    }
    const msg = error.response?.data?.mensaje || error.response?.data?.message;
    if (msg) error.backendMessage = msg;
    return Promise.reject(error);
  }
);

export default client;
