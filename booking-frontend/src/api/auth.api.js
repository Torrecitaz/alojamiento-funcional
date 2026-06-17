import client from './client';

export const authApi = {
  login: (credentials) => client.post('/auth-alojaexpress/login', credentials),
  getUsuarios: () => client.get('/auth-alojaexpress/usuarios'),
  getRoles: () => client.get('/auth-alojaexpress/roles'),
  cambiarRol: (userId, rolId) => client.patch(`/auth-alojaexpress/usuarios/${userId}/rol`, { rolId }),
  cambiarEstado: (userId, activo) => client.patch(`/auth-alojaexpress/usuarios/${userId}/estado`, { activo }),
};
