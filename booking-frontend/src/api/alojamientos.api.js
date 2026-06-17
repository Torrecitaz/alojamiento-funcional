import client from './client';

export const alojamientosApi = {
  getAll: (params) => client.get('/alojamientos-alojaexpress', { params }),
  getById: (id) => client.get(`/alojamientos-alojaexpress/${id}`),
  crear: (data) => client.post('/alojamientos-alojaexpress', data),
  actualizar: (id, data) => client.put(`/alojamientos-alojaexpress/${id}`, data),
  actualizarEstado: (id, nuevoEstado) => client.patch(`/alojamientos-alojaexpress/${id}/estado`, { nuevoEstado }),
  buscar: (params) => client.get('/alojamientos-alojaexpress/buscar', { params }),
  getByColaboradorId: (colaboradorId) => client.get(`/alojamientos-alojaexpress/colaborador/${colaboradorId}`),
  getTipos: () => client.get('/alojamientos-alojaexpress/tipos'),
  getTiposAlojamiento: () => client.get('/alojamientos-alojaexpress/tipos-alojamiento'),
  getCiudades: () => client.get('/alojamientos-alojaexpress/ciudades'),
  duplicar: (id) => client.post(`/alojamientos-alojaexpress/duplicar/${id}`),
  agregarFoto: (id, formData) => client.post(`/alojamientos-alojaexpress/${id}/fotos`, formData, {
    headers: { 'Content-Type': 'multipart/form-data' }
  }),
};
