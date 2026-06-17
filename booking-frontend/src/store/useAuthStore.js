import { create } from 'zustand';
import { jwtDecode } from 'jwt-decode';
import { clientesApi } from '../api/clientes.api';

const getInitialUser = () => {
  try {
    const saved = localStorage.getItem('alojaexpress_user');
    return saved ? JSON.parse(saved) : null;
  } catch (e) {
    console.error("Error parsing saved user", e);
    localStorage.removeItem('alojaexpress_user');
    return null;
  }
};

const useAuthStore = create((set) => ({
  token: localStorage.getItem('alojaexpress_token') || null,
  user: getInitialUser(),
  isAuthenticated: !!localStorage.getItem('alojaexpress_token'),

  login: async (loginResponse) => {
    const decoded = jwtDecode(loginResponse.token);
    const userId = decoded.sub; // The JWT 'sub' claim contains the UsuarioId

    let clienteId = loginResponse.clienteId;

    // Fallback: si clienteId es null, buscar el clienteId por email consultando la lista de clientes
    if (!clienteId && loginResponse.email) {
      try {
        const { data } = await clientesApi.getAll({ page: 1, size: 200 });
        const clients = data?.datos || data?.items || (Array.isArray(data) ? data : []);
        const matchingClient = clients.find(c => c.email?.toLowerCase() === loginResponse.email.toLowerCase());
        if (matchingClient) {
          clienteId = matchingClient.clienteId;
        }
      } catch (e) {
        console.error("Error resolviendo clienteId por email:", e);
      }
    }

    const userData = {
      id: userId,
      clienteId: clienteId,
      colaboradorId: loginResponse.colaboradorId,
      nombreCompleto: loginResponse.nombreCompleto,
      email: loginResponse.email,
      roles: loginResponse.roles,
    };

    localStorage.setItem('alojaexpress_token', loginResponse.token);
    localStorage.setItem('alojaexpress_user', JSON.stringify(userData));
    set({
      token: loginResponse.token,
      user: userData,
      isAuthenticated: true,
    });
  },

  logout: () => {
    localStorage.removeItem('alojaexpress_token');
    localStorage.removeItem('alojaexpress_user');
    set({ token: null, user: null, isAuthenticated: false });
  },
}));

export default useAuthStore;
