import { useState, useEffect } from 'react';
import { HiOutlinePlus, HiOutlineOfficeBuilding, HiOutlineLocationMarker, HiOutlinePhotograph, HiOutlineX } from 'react-icons/hi';
import toast from 'react-hot-toast';
import { alojamientosApi } from '../../api/alojamientos.api';
import { colaboradoresApi } from '../../api/colaboradores.api';
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
  const [editingId, setEditingId] = useState(null);

  // Estados de galería de fotos
  const [activeGaleriaId, setActiveGaleriaId] = useState(null);
  const [fotosGaleria, setFotosGaleria] = useState([]);
  const [uploadingFoto, setUploadingFoto] = useState(false);

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
        esAdmin ? alojamientosApi.buscar() : alojamientosApi.getByColaboradorId(user?.colaboradorId),
        alojamientosApi.getCiudades(),
        alojamientosApi.getTiposAlojamiento(),
        esAdmin ? colaboradoresApi.getAll() : Promise.resolve({ data: { datos: [] } })
      ]);

      if (propRes.status === 'fulfilled') {
        const payload = propRes.value.data.datos;
        const list = esAdmin ? (payload?.items || []) : (payload || []);
        const normalized = list.map(item => ({
          ...item,
          propiedadId: item.propiedadId || item.alojamientoId,
          alojamientoId: item.alojamientoId || item.propiedadId
        }));
        setPropiedades(normalized);
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
        colaboradorId: esAdmin 
          ? (formData.colaboradorId ? parseInt(formData.colaboradorId) : 1) 
          : (user?.colaboradorId || 1)
      };

      if (editingId) {
        await alojamientosApi.actualizar(editingId, payload);
        toast.success('Propiedad actualizada exitosamente.');
      } else {
        await alojamientosApi.crear(payload);
        toast.success('Propiedad creada exitosamente.');
      }
      
      setShowForm(false);
      setEditingId(null);
      setFormData({
        nombre: '', descripcion: '', direccion: '', ciudadId: '', 
        tipoAlojamientoId: '', estrellas: 3, admiteMascotas: false, colaboradorId: ''
      });
      loadData();
    } catch (err) {
      toast.error(err.response?.data?.mensaje || `Error al ${editingId ? 'actualizar' : 'crear'} la propiedad.`);
    } finally {
      setSubmitting(false);
    }
  };

  const handleEditClick = (p) => {
    const matchedCiudad = ciudades.find(c => c.nombre === p.ciudad);
    const matchedTipo = tipos.find(t => t.nombre === p.tipoAlojamiento);

    setFormData({
      nombre: p.nombre,
      descripcion: p.descripcion || '',
      direccion: p.direccion,
      ciudadId: matchedCiudad ? matchedCiudad.ciudadId.toString() : '',
      tipoAlojamientoId: matchedTipo ? matchedTipo.tipoAlojamientoId.toString() : '',
      estrellas: p.estrellas,
      admiteMascotas: p.admiteMascotas || false,
      colaboradorId: p.colaboradorId || (esAdmin ? '' : (user?.colaboradorId || ''))
    });
    setEditingId(p.propiedadId);
    setShowForm(true);
  };

  const handleDuplicate = async (id) => {
    const loadingToast = toast.loading('Duplicando propiedad y habitaciones...');
    try {
      await alojamientosApi.duplicar(id);
      toast.success('Propiedad duplicada exitosamente.', { id: loadingToast });
      loadData();
    } catch (err) {
      toast.error(err.response?.data?.mensaje || 'Error al duplicar la propiedad.', { id: loadingToast });
    }
  };

  const loadFotos = async (propiedadId) => {
    try {
      const { data } = await alojamientosApi.getById(propiedadId);
      setFotosGaleria(data.datos?.fotos || []);
    } catch {
      toast.error('Error al cargar fotos de la galería.');
    }
  };

  const handleOpenGaleria = (propiedadId) => {
    setActiveGaleriaId(propiedadId);
    setFotosGaleria([]);
    loadFotos(propiedadId);
  };

  const handleUploadFoto = async (e) => {
    const file = e.target.files?.[0];
    if (!file) return;

    setUploadingFoto(true);
    const formDataPayload = new FormData();
    formDataPayload.append('file', file);

    const loadingToast = toast.loading('Subiendo imagen a la galería...');
    try {
      await alojamientosApi.agregarFoto(activeGaleriaId, formDataPayload);
      toast.success('Imagen subida exitosamente.', { id: loadingToast });
      loadFotos(activeGaleriaId);
    } catch (err) {
      toast.error(err.response?.data?.mensaje || 'Error al subir la imagen.', { id: loadingToast });
    } finally {
      setUploadingFoto(false);
    }
  };

  const toggleEstado = async (id, estadoActual) => {
    const nuevoEstado = estadoActual === 'Activa' ? 'Inactiva' : 'Activa';
    try {
      await alojamientosApi.actualizarEstado(id, nuevoEstado);
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
        <button 
          className="btn btn-primary" 
          onClick={() => {
            if (showForm) {
              setEditingId(null);
              setFormData({
                nombre: '', descripcion: '', direccion: '', ciudadId: '', 
                tipoAlojamientoId: '', estrellas: 3, admiteMascotas: false, colaboradorId: ''
              });
            }
            setShowForm(!showForm);
          }}
        >
          <HiOutlinePlus size={18} /> {showForm ? 'Ocultar Formulario' : 'Nueva Propiedad'}
        </button>
      </div>

      {showForm && (
        <div className="card" style={{ padding: 24, marginBottom: 24, border: '1px solid var(--color-border)' }}>
          <h3 style={{ marginTop: 0, marginBottom: 20 }}>{editingId ? 'Editar Propiedad' : 'Registrar Nueva Propiedad'}</h3>
          <form onSubmit={handleSubmit}>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16, marginBottom: 16 }}>
              <div>
                <label className="form-label">Nombre de la Propiedad</label>
                <input required type="text" className="input-field" name="nombre" value={formData.nombre} onChange={handleInputChange} placeholder="Ej. Hotel Paraíso" />
              </div>
              {esAdmin && (
                <div>
                  <label className="form-label">Colaborador Dueño</label>
                  <select className="input-field" name="colaboradorId" value={formData.colaboradorId} onChange={handleInputChange}>
                    <option value="">-- Ninguno / Administrador --</option>
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
              <button 
                type="button" 
                className="btn btn-outline" 
                onClick={() => {
                  setShowForm(false);
                  setEditingId(null);
                  setFormData({
                    nombre: '', descripcion: '', direccion: '', ciudadId: '', 
                    tipoAlojamientoId: '', estrellas: 3, admiteMascotas: false, colaboradorId: ''
                  });
                }}
              >
                Cancelar
              </button>
              <button type="submit" className="btn btn-primary" disabled={submitting}>
                {submitting ? 'Guardando...' : (editingId ? 'Actualizar Propiedad' : 'Guardar Propiedad')}
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
                    <span className={`badge ${p.estado === 'Activa' || p.estado === 'Activo' ? 'badge-success' : 'badge-danger'}`}>
                      {p.estado || 'Activa'}
                    </span>
                  </td>
                  <td>
                    <div style={{ display: 'flex', gap: 8 }}>
                      <button 
                        className="admin-btn-edit" 
                        onClick={() => handleEditClick(p)}
                        style={{ backgroundColor: 'var(--color-primary)', color: '#fff', border: 'none', borderRadius: '4px', padding: '4px 8px', cursor: 'pointer' }}
                      >
                        Editar
                      </button>
                      <button 
                        className="admin-btn-edit" 
                        onClick={() => handleDuplicate(p.propiedadId)}
                        style={{ backgroundColor: '#10b981', color: '#fff', border: 'none', borderRadius: '4px', padding: '4px 8px', cursor: 'pointer' }}
                      >
                        Duplicar
                      </button>
                      <button 
                        className="admin-btn-edit" 
                        onClick={() => handleOpenGaleria(p.propiedadId)}
                        style={{ backgroundColor: '#f59e0b', color: '#fff', border: 'none', borderRadius: '4px', padding: '4px 8px', cursor: 'pointer', display: 'flex', alignItems: 'center', gap: 4 }}
                      >
                        <HiOutlinePhotograph size={14} /> Fotos
                      </button>
                      <button 
                        className="admin-btn-edit"
                        onClick={() => toggleEstado(p.propiedadId, p.estado || 'Activa')}
                        style={{ padding: '4px 8px', borderRadius: '4px', border: '1px solid var(--color-border)' }}
                      >
                        {p.estado === 'Activa' || p.estado === 'Activo' ? 'Desactivar' : 'Activar'}
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {/* Modal de Galería de Fotos */}
      {activeGaleriaId && (
        <div style={{
          position: 'fixed', top: 0, left: 0, right: 0, bottom: 0,
          backgroundColor: 'rgba(0,0,0,0.5)', display: 'flex', justifyContent: 'center', alignItems: 'center',
          zIndex: 1000, padding: 20
        }}>
          <div className="card" style={{ width: '100%', maxWidth: 700, maxHeight: '90vh', overflowY: 'auto', padding: 24, position: 'relative' }}>
            <button 
              onClick={() => setActiveGaleriaId(null)}
              style={{ position: 'absolute', top: 16, right: 16, border: 'none', background: 'none', cursor: 'pointer', color: 'var(--color-text-muted)' }}
            >
              <HiOutlineX size={24} />
            </button>
            <h3 style={{ marginTop: 0, marginBottom: 20 }}>Galería de Fotos (Propiedad #{activeGaleriaId})</h3>
            
            {/* Zona de Subida */}
            <div style={{
              border: '2px dashed var(--color-border)', borderRadius: 8, padding: 30, textAlign: 'center',
              background: 'var(--color-bg-alt)', marginBottom: 24, cursor: 'pointer', position: 'relative'
            }}>
              <HiOutlinePhotograph size={36} style={{ color: 'var(--color-accent)', marginBottom: 8, opacity: 0.8 }} />
              <p style={{ margin: 0, fontWeight: 500 }}>Arrastra una imagen aquí o haz clic para subir</p>
              <p style={{ margin: 0, fontSize: '.8rem', color: 'var(--color-text-muted)', marginTop: 4 }}>Formatos aceptados: PNG, JPG, JPEG</p>
              <input 
                type="file" 
                accept="image/*"
                onChange={handleUploadFoto}
                disabled={uploadingFoto}
                style={{
                  position: 'absolute', top: 0, left: 0, right: 0, bottom: 0, opacity: 0, cursor: 'pointer'
                }}
              />
            </div>

            {/* Listado de Fotos */}
            <h4 style={{ marginBottom: 12 }}>Fotos Actuales ({fotosGaleria.length})</h4>
            {fotosGaleria.length === 0 ? (
              <p style={{ textAlign: 'center', color: 'var(--color-text-muted)', padding: 20 }}>No hay fotos registradas para este alojamiento.</p>
            ) : (
              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(140px, 1fr))', gap: 12 }}>
                {fotosGaleria.map((f, i) => (
                  <div key={i} style={{ borderRadius: 6, overflow: 'hidden', border: '1px solid var(--color-border)', position: 'relative', height: 100 }}>
                    <img src={f.url} alt={f.descripcion || 'Foto'} style={{ width: '100%', height: '100%', objectFit: 'cover' }} />
                  </div>
                ))}
              </div>
            )}
            
            <div style={{ display: 'flex', justifyContent: 'flex-end', marginTop: 24 }}>
              <button className="btn btn-outline" onClick={() => setActiveGaleriaId(null)}>Cerrar</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
