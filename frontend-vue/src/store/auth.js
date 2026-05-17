import { defineStore } from 'pinia';
import { jwtDecode } from 'jwt-decode';
import api from '@/services/api';

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: null,
    token: localStorage.getItem('booking_token') || null,
  }),
  getters: {
    isAuthenticated: (state) => !!state.token,
    isAdmin: (state) => state.user?.role === 'Administrador',
  },
  actions: {
    initialize() {
      if (this.token) {
        try {
          const decoded = jwtDecode(this.token);
          this.user = {
            id: decoded.nameid || decoded.sub,
            email: decoded.email,
            role: decoded.role || decoded.rol,
            nombre: decoded.unique_name || decoded.nombre
          };
        } catch (error) {
          this.logout();
        }
      }
    },
    async login(credentials) {
      const res = await api.post('/Auth/login', credentials);
      const { token } = res.data;
      this.setToken(token);
    },
    async register(data) {
      await api.post('/Usuarios/clientes/registrar', data);
    },
    setToken(token) {
      this.token = token;
      localStorage.setItem('booking_token', token);
      this.initialize();
    },
    logout() {
      this.user = null;
      this.token = null;
      localStorage.removeItem('booking_token');
    }
  }
});
