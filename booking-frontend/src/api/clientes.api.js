import client from './client';

export const clientesApi = {
  registrar: (data) => client.post('/clientes-alojaexpress/registrar', data),
  getById: (id) => client.get(`/clientes-alojaexpress/${id}`),
  getByCedula: (cedula) => client.get(`/clientes-alojaexpress/cedula/${cedula}`),
  getAll: (params) => client.get('/clientes-alojaexpress', { params }),
};
