import { create } from 'zustand';
import { jwtDecode } from 'jwt-decode';

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

  login: (loginResponse) => {
    const decoded = jwtDecode(loginResponse.token);
    const userId = decoded.sub; // The JWT 'sub' claim contains the UsuarioId

    localStorage.setItem('alojaexpress_token', loginResponse.token);
    localStorage.setItem('alojaexpress_user', JSON.stringify({
      id: userId,
      clienteId: loginResponse.clienteId,
      colaboradorId: loginResponse.colaboradorId,
      nombreCompleto: loginResponse.nombreCompleto,
      email: loginResponse.email,
      roles: loginResponse.roles,
    }));
    set({
      token: loginResponse.token,
      user: {
        id: userId,
        clienteId: loginResponse.clienteId,
        colaboradorId: loginResponse.colaboradorId,
        nombreCompleto: loginResponse.nombreCompleto,
        email: loginResponse.email,
        roles: loginResponse.roles,
      },
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
