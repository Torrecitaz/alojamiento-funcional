import client from './client';

export const fotosApi = {
  getByAlojamientoId: (alojamientoId) => client.get(`/fotos-alojaexpress/alojamiento/${alojamientoId}`),
  agregar: (alojamientoId, formData) => client.post(`/fotos-alojaexpress/alojamiento/${alojamientoId}`, formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  }),
  eliminar: (id) => client.delete(`/fotos-alojaexpress/${id}`),
};
