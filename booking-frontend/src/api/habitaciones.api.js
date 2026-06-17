import client from './client';

export const habitacionesApi = {
  getByAlojamientoId: (alojamientoId) => client.get(`/habitaciones-alojaexpress/alojamiento/${alojamientoId}`),
  crear: (data) => client.post('/habitaciones-alojaexpress', data),
  actualizar: (id, data) => client.put(`/habitaciones-alojaexpress/${id}`, data),
  eliminar: (id) => client.delete(`/habitaciones-alojaexpress/${id}`),
  getDisponibilidad: (id, params) => client.get(`/alojamientos-alojaexpress/${id}/disponibilidad`, { params }),
};
