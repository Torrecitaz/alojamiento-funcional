import axios from 'axios';

const BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api/v2';
const client = axios.create({ baseURL: BASE_URL, timeout: 15000 });

// Inyectar JWT en cada request
client.interceptors.request.use((config) => {
  const token = localStorage.getItem('alojaexpress_token');
  if (token) config.headers.Authorization = `Bearer ${token}`;
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
