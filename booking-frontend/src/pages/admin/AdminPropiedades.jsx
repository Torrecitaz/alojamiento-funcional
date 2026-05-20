import { useState, useEffect } from 'react';
import { HiOutlinePlus, HiOutlineOfficeBuilding, HiOutlineLocationMarker } from 'react-icons/hi';
import toast from 'react-hot-toast';
import api from '../../services/api';
import useAuthStore from '../../store/useAuthStore';
import './AdminLayout.css';

export default function AdminPropiedades() {
  const { user } = useAuthStore();
  const esAdmin = user?.roles?.includes('Administrador');
  const esColaborador = user?.roles?.includes('Colaborador');

  const [propiedades, setPropiedades] = useState([]);
  const [ciudades, setCiudades] = useState([]);
  const [tipos, setTipos] = useState([]);
  const [colaboradores, setColaboradores] = useState([]);
  
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [submitting, setSubmitting] = useState(false);

  // Form State
  const [formData, setFormData] = useState({
    nombre: '',
    descripcion: '',
    direccion: '',
    ciudadId: '',
    tipoAlojamientoId: '',
    estrellas: 3,
    admiteMascotas: false,
    colaboradorId: esAdmin ? '' : (user?.colaboradorId || '')
  });

  useEffect(() => { 
    loadData(); 
    // eslint-disable-next-line
  }, []);

  const loadData = async () => {
    setLoading(true);
    try {
      const [propRes, ciudRes, tipoRes, colabRes] = await Promise.allSettled([
        esAdmin ? api.get('/propiedades/buscar') : api.get(`/propiedades/colaborador/${user?.colaboradorId}`),
        api.get('/maestros/ciudades'),
        api.get('/maestros/tipos-alojamiento'),
        esAdmin ? api.get('/colaboradores') : Promise.resolve({ data: { datos: [] } })
      ]);

      if (propRes.status === 'fulfilled') {
        const payload = propRes.value.data.datos;
        setPropiedades(esAdmin ? (payload?.items || []) : (payload || []));
      }
      if (ciudRes.status === 'fulfilled') setCiudades(ciudRes.value.data.datos || []);
      if (tipoRes.status === 'fulfilled') setTipos(tipoRes.value.data.datos || []);
      if (colabRes.status === 'fulfilled' && esAdmin) setColaboradores(colabRes.value.data.datos || []);
    } catch {
      toast.error('Error al cargar datos maestros.');
    } finally {
      setLoading(false);
    }
  };

  const handleInputChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSubmitting(true);
    try {
      const payload = {
        ...formData,
        ciudadId: parseInt(formData.ciudadId),
        tipoAlojamientoId: parseInt(formData.tipoAlojamientoId),
        estrellas: parseInt(formData.estrellas),
        colaboradorId: esAdmin ? parseInt(formData.colaboradorId) : user?.colaboradorId
      };

      await api.post('/propiedades', payload);
      toast.success('Propiedad creada exitosamente.');
      setShowForm(false);
      setFormData({
        nombre: '', descripcion: '', direccion: '', ciudadId: '', 
        tipoAlojamientoId: '', estrellas: 3, admiteMascotas: false, colaboradorId: ''
      });
      loadData();
    } catch (err) {
      toast.error(err.response?.data?.mensaje || 'Error al crear la propiedad.');
    } finally {
      setSubmitting(false);
    }
  };

  const toggleEstado = async (id, estadoActual) => {
    const nuevoEstado = estadoActual === 'Activa' ? 'Inactiva' : 'Activa';
    try {
      await api.patch(`/propiedades/${id}/estado`, { nuevoEstado });
      toast.success(`Propiedad marcada como ${nuevoEstado}`);
      loadData();
    } catch {
      toast.error('Error al cambiar el estado.');
    }
  };

  return (
    <div>
      <div className="admin-page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <div>
          <h1 className="admin-page-title">Propiedades</h1>
          <p className="admin-page-subtitle">Gestión del inventario de hoteles y alojamientos</p>
        </div>
        <button className="btn btn-primary" onClick={() => setShowForm(!showForm)}>
          <HiOutlinePlus size={18} /> {showForm ? 'Ocultar Formulario' : 'Nueva Propiedad'}
        </button>
      </div>

      {showForm && (
        <div className="card" style={{ padding: 24, marginBottom: 24, border: '1px solid var(--color-border)' }}>
          <h3 style={{ marginTop: 0, marginBottom: 20 }}>Registrar Nueva Propiedad</h3>
          <form onSubmit={handleSubmit}>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16, marginBottom: 16 }}>
              <div>
                <label className="form-label">Nombre de la Propiedad</label>
                <input required type="text" className="input-field" name="nombre" value={formData.nombre} onChange={handleInputChange} placeholder="Ej. Hotel Paraíso" />
              </div>
              {esAdmin && (
                <div>
                  <label className="form-label">Colaborador Dueño</label>
                  <select required className="input-field" name="colaboradorId" value={formData.colaboradorId} onChange={handleInputChange}>
                    <option value="">-- Seleccionar --</option>
                    {colaboradores.map(c => <option key={c.colaboradorId} value={c.colaboradorId}>{c.nombreEmpresa || c.nombreCompleto}</option>)}
                  </select>
                </div>
              )}
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16, marginBottom: 16 }}>
              <div>
                <label className="form-label">Ciudad</label>
                <select required className="input-field" name="ciudadId" value={formData.ciudadId} onChange={handleInputChange}>
                  <option value="">-- Seleccionar --</option>
                  {ciudades.map(c => <option key={c.ciudadId} value={c.ciudadId}>{c.nombre}, {c.pais}</option>)}
                </select>
              </div>
              <div>
                <label className="form-label">Tipo de Alojamiento</label>
                <select required className="input-field" name="tipoAlojamientoId" value={formData.tipoAlojamientoId} onChange={handleInputChange}>
                  <option value="">-- Seleccionar --</option>
                  {tipos.map(t => <option key={t.tipoAlojamientoId} value={t.tipoAlojamientoId}>{t.nombre}</option>)}
                </select>
              </div>
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: 16, marginBottom: 16 }}>
              <div>
                <label className="form-label">Dirección Exacta</label>
                <input required type="text" className="input-field" name="direccion" value={formData.direccion} onChange={handleInputChange} />
              </div>
              <div>
                <label className="form-label">Estrellas</label>
                <select className="input-field" name="estrellas" value={formData.estrellas} onChange={handleInputChange}>
                  {[1,2,3,4,5].map(n => <option key={n} value={n}>{n} Estrellas</option>)}
                </select>
              </div>
              <div style={{ display: 'flex', alignItems: 'flex-end', paddingBottom: 10 }}>
                <label style={{ display: 'flex', alignItems: 'center', gap: 8, cursor: 'pointer' }}>
                  <input type="checkbox" name="admiteMascotas" checked={formData.admiteMascotas} onChange={handleInputChange} />
                  Admite Mascotas
                </label>
              </div>
            </div>

            <div style={{ marginBottom: 20 }}>
              <label className="form-label">Descripción</label>
              <textarea required className="input-field" name="descripcion" value={formData.descripcion} onChange={handleInputChange} rows={3}></textarea>
            </div>

            <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 12 }}>
              <button type="button" className="btn btn-outline" onClick={() => setShowForm(false)}>Cancelar</button>
              <button type="submit" className="btn btn-primary" disabled={submitting}>
                {submitting ? 'Guardando...' : 'Guardar Propiedad'}
              </button>
            </div>
          </form>
        </div>
      )}

      <div className="admin-table-wrapper">
        <div className="admin-table-header">
          <h3 className="admin-table-title">Inventario ({propiedades.length})</h3>
        </div>
        {loading ? (
          <div style={{ padding: 40, textAlign: 'center', color: 'var(--color-text-muted)' }}>Cargando propiedades...</div>
        ) : propiedades.length === 0 ? (
          <div style={{ padding: 40, textAlign: 'center', color: 'var(--color-text-muted)' }}>No hay propiedades registradas.</div>
        ) : (
          <table className="admin-table">
            <thead>
              <tr>
                <th>ID</th>
                <th>Nombre</th>
                <th>Ubicación</th>
                <th>Estrellas</th>
                <th>Estado</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {propiedades.map((p) => (
                <tr key={p.propiedadId}>
                  <td>{p.propiedadId}</td>
                  <td>
                    <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                      <HiOutlineOfficeBuilding size={18} style={{ color: 'var(--color-accent)' }} />
                      <strong style={{ color: 'var(--color-text)' }}>{p.nombre}</strong>
                    </div>
                  </td>
                  <td>
                    <div style={{ display: 'flex', alignItems: 'center', gap: 4, fontSize: '.85rem' }}>
                      <HiOutlineLocationMarker size={14} /> {p.ciudad || 'N/D'}
                    </div>
                  </td>
                  <td style={{ color: '#fbbf24' }}>{'★'.repeat(p.estrellas)}</td>
                  <td>
                    <span className={`badge ${p.estado === 'Activa' ? 'badge-success' : 'badge-danger'}`}>
                      {p.estado || 'Activa'}
                    </span>
                  </td>
                  <td>
                    <button 
                      className="admin-btn-edit"
                      onClick={() => toggleEstado(p.propiedadId, p.estado || 'Activa')}
                    >
                      {p.estado === 'Activa' ? 'Desactivar' : 'Activar'}
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
