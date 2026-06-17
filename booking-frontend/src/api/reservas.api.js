import client from './client';

export const reservasApi = {
  crear: (data) => client.post('/reservas/booking', data),
  getByCodigo: (codigoReserva) => client.get(`/reservas-alojaexpress/${codigoReserva}`),
  getByClienteId: (clienteId) => client.get(`/reservas-alojaexpress/cliente/${clienteId}`),
  actualizarEstado: (id, estado) => client.patch(`/reservas-alojaexpress/${id}/estado`, { estado, nuevoEstado: estado }),
  cancelar: (id) => client.patch(`/reservas-alojaexpress/${id}/cancelar`),
  checkout: (data, config) => client.post('/reservas-alojaexpress/checkout', data, config),
  getTodas: () => client.get('/reservas-alojaexpress/todas'),
};
