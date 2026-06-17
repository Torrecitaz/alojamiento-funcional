import client from './client';

export const colaboradoresApi = {
  getAll: () => client.get('/colaboradores-alojaexpress'),
  crear: (data) => client.post('/colaboradores-alojaexpress', data),
  eliminar: (id) => client.delete(`/colaboradores-alojaexpress/${id}`),
};
