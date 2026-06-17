import client from './client';

export const calendarioApi = {
  getDisponibilidad: (habitacionId, params) => client.get(`/calendario-alojaexpress/habitacion/${habitacionId}`, { params }),
  bloquear: (data) => client.post('/calendario-alojaexpress/bloquear', data),
  liberar: (data) => client.post('/calendario-alojaexpress/liberar', data),
};
