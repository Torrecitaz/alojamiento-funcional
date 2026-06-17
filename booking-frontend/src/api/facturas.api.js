import client from './client';

export const facturasApi = {
  crear: (data) => client.post('/facturas-alojaexpress', data),
  getByReservaId: (reservaId) => client.get(`/facturas-alojaexpress/reserva/${reservaId}`),
};
