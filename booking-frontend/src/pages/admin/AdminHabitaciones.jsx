import { useState, useEffect } from 'react';
import { HiOutlinePlus, HiOutlineKey, HiOutlineTrash, HiOutlineSearch, HiOutlineOfficeBuilding } from 'react-icons/hi';
import toast from 'react-hot-toast';
import api from '../../services/api';
import useAuthStore from '../../store/useAuthStore';
import './AdminLayout.css';

export default function AdminHabitaciones() {
  const { user } = useAuthStore();
  const esAdmin = user?.roles?.includes('Administrador');

  const [propiedades, setPropiedades] = useState([]);
  const [propiedadSeleccionada, setPropiedadSeleccionada] = useState('');
  const [habitaciones, setHabitaciones] = useState([]);
  
  const [loading, setLoading] = useState(false);
  const [showForm, setShowForm] = useState(false);
  const [submitting, setSubmitting] = useState(false);

  // Form State
  const [formData, setFormData] = useState({
    nombre: '',
    descripcion: '',
    capacidadAdultos: 2,
    capacidadNinos: 0,
    numBanos: 1,
    numDormitorios: 1,
    superficieM2: '',
    admiteMascotas: false,
    tieneCocina: false,
    tieneAireAcondicionado: true
  });

  useEffect(() => {
    // Cargar propiedades para el select principal
    const fetchProps = esAdmin 
      ? api.get('/propiedades/buscar?PageSize=1000')
      : user?.colaboradorId 
        ? api.get(`/propiedades/colaborador/${user.colaboradorId}`)
        : Promise.resolve({ data: { datos: [] } });

    fetchProps
      .then(res => {
        const payload = res.data.datos;
        setPropiedades(esAdmin ? (payload?.items || []) : (payload || []));
      })
      .catch(() => toast.error('Error al cargar la lista de propiedades.'));
    // eslint-disable-next-line
  }, []);

  useEffect(() => {
    if (propiedadSeleccionada) {
      buscarHabitaciones();
    } else {
      setHabitaciones([]);
      setShowForm(false);
    }
  }, [propiedadSeleccionada]);

  const buscarHabitaciones = async () => {
    setLoading(true);
    try {
      const { data } = await api.get(`/habitaciones/por-propiedad/${propiedadSeleccionada}`);
      setHabitaciones(data.datos || []);
    } catch { 
      toast.error('Error al cargar habitaciones.'); 
      setHabitaciones([]); 
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
    if (!propiedadSeleccionada) return toast.error('Selecciona una propiedad primero.');

    setSubmitting(true);
    try {
      const payload = {
        ...formData,
        propiedadId: parseInt(propiedadSeleccionada),
        capacidadAdultos: parseInt(formData.capacidadAdultos),
        capacidadNinos: parseInt(formData.capacidadNinos),
        numBanos: parseInt(formData.numBanos),
        numDormitorios: parseInt(formData.numDormitorios),
        superficieM2: formData.superficieM2 ? parseInt(formData.superficieM2) : null
      };

      await api.post('/habitaciones', payload);
      toast.success('Habitación agregada exitosamente.');
      setShowForm(false);
      setFormData({
        nombre: '', descripcion: '', capacidadAdultos: 2, capacidadNinos: 0,
        numBanos: 1, numDormitorios: 1, superficieM2: '',
        admiteMascotas: false, tieneCocina: false, tieneAireAcondicionado: true
      });
      buscarHabitaciones();
    } catch (err) {
      toast.error(err.response?.data?.mensaje || 'Error al agregar la habitación.');
    } finally {
      setSubmitting(false);
    }
  };

  const eliminarHabitacion = async (id) => {
    if (!window.confirm('¿Estás seguro de eliminar esta habitación de forma lógica?')) return;
    try {
      await api.delete(`/habitaciones/${id}`);
      toast.success('Habitación eliminada.');
      buscarHabitaciones();
    } catch {
      toast.error('Error al eliminar la habitación.');
    }
  };

  return (
    <div>
      <div className="admin-page-header">
        <h1 className="admin-page-title">Habitaciones y Espacios</h1>
        <p className="admin-page-subtitle">Gestiona las habitaciones disponibles por cada propiedad</p>
      </div>

      {/* Filtro Principal */}
      <div className="card" style={{ padding: 20, marginBottom: 24, display: 'flex', gap: 16, alignItems: 'center', background: 'var(--color-bg)' }}>
        <HiOutlineSearch size={24} style={{ color: 'var(--color-primary)' }} />
        <div style={{ flex: 1 }}>
          <label className="form-label" style={{ marginBottom: 4 }}>Selecciona una Propiedad para administrar sus habitaciones</label>
          <select 
            className="input-field" 
            value={propiedadSeleccionada} 
            onChange={e => setPropiedadSeleccionada(e.target.value)}
            style={{ maxWidth: 400, fontWeight: 600 }}
          >
            <option value="">-- Buscar propiedad --</option>
            {propiedades.map(p => (
              <option key={p.propiedadId} value={p.propiedadId}>
                #{p.propiedadId} - {p.nombre} ({p.ciudad})
              </option>
            ))}
          </select>
        </div>
        {propiedadSeleccionada && (
          <button className="btn btn-primary" onClick={() => setShowForm(!showForm)}>
            <HiOutlinePlus size={18} /> {showForm ? 'Ocultar Formulario' : 'Nueva Habitación'}
          </button>
        )}
      </div>

      {/* Formulario */}
      {showForm && propiedadSeleccionada && (
        <div className="card" style={{ padding: 24, marginBottom: 24, border: '1px solid var(--color-border)' }}>
          <h3 style={{ marginTop: 0, marginBottom: 20 }}>Registrar Nueva Habitación</h3>
          <form onSubmit={handleSubmit}>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16, marginBottom: 16 }}>
              <div>
                <label className="form-label">Nombre / Etiqueta</label>
                <input required type="text" className="input-field" name="nombre" value={formData.nombre} onChange={handleInputChange} placeholder="Ej. Suite Presidencial, Habitación 101" />
              </div>
              <div>
                <label className="form-label">Superficie (m²)</label>
                <input type="number" className="input-field" name="superficieM2" value={formData.superficieM2} onChange={handleInputChange} placeholder="Opcional" />
              </div>
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: 16, marginBottom: 16 }}>
              <div>
                <label className="form-label">Adultos Máx.</label>
                <input required type="number" min="1" className="input-field" name="capacidadAdultos" value={formData.capacidadAdultos} onChange={handleInputChange} />
              </div>
              <div>
                <label className="form-label">Niños Máx.</label>
                <input required type="number" min="0" className="input-field" name="capacidadNinos" value={formData.capacidadNinos} onChange={handleInputChange} />
              </div>
              <div>
                <label className="form-label">Dormitorios</label>
                <input required type="number" min="1" className="input-field" name="numDormitorios" value={formData.numDormitorios} onChange={handleInputChange} />
              </div>
              <div>
                <label className="form-label">Baños</label>
                <input required type="number" min="1" className="input-field" name="numBanos" value={formData.numBanos} onChange={handleInputChange} />
              </div>
            </div>

            <div style={{ display: 'flex', gap: 24, marginBottom: 20, padding: '16px', background: 'var(--color-bg-alt)', borderRadius: 8 }}>
              <label style={{ display: 'flex', alignItems: 'center', gap: 8, cursor: 'pointer', fontWeight: 500 }}>
                <input type="checkbox" name="tieneAireAcondicionado" checked={formData.tieneAireAcondicionado} onChange={handleInputChange} /> A/C
              </label>
              <label style={{ display: 'flex', alignItems: 'center', gap: 8, cursor: 'pointer', fontWeight: 500 }}>
                <input type="checkbox" name="tieneCocina" checked={formData.tieneCocina} onChange={handleInputChange} /> Cocina
              </label>
              <label style={{ display: 'flex', alignItems: 'center', gap: 8, cursor: 'pointer', fontWeight: 500 }}>
                <input type="checkbox" name="admiteMascotas" checked={formData.admiteMascotas} onChange={handleInputChange} /> Mascotas
              </label>
            </div>

            <div style={{ marginBottom: 20 }}>
              <label className="form-label">Descripción Detallada</label>
              <textarea required className="input-field" name="descripcion" value={formData.descripcion} onChange={handleInputChange} rows={2} placeholder="Características especiales de la habitación..."></textarea>
            </div>

            <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 12 }}>
              <button type="button" className="btn btn-outline" onClick={() => setShowForm(false)}>Cancelar</button>
              <button type="submit" className="btn btn-primary" disabled={submitting}>
                {submitting ? 'Guardando...' : 'Guardar Habitación'}
              </button>
            </div>
          </form>
        </div>
      )}

      {/* Lista de Habitaciones */}
      {propiedadSeleccionada && (
        <div className="admin-table-wrapper">
          <div className="admin-table-header">
            <h3 className="admin-table-title">Inventario de Habitaciones ({habitaciones.length})</h3>
          </div>
          {loading ? (
            <div style={{ padding: 40, textAlign: 'center', color: 'var(--color-text-muted)' }}>Cargando...</div>
          ) : habitaciones.length === 0 ? (
            <div style={{ padding: 40, textAlign: 'center', color: 'var(--color-text-muted)' }}>
              Esta propiedad no tiene habitaciones registradas.
            </div>
          ) : (
            <table className="admin-table">
              <thead>
                <tr>
                  <th>Nombre</th>
                  <th>Capacidad</th>
                  <th>Distribución</th>
                  <th>A/C</th>
                  <th>Cocina</th>
                  <th>Mascotas</th>
                  <th>Acciones</th>
                </tr>
              </thead>
              <tbody>
                {habitaciones.map((h) => (
                  <tr key={h.habitacionId}>
                    <td>
                      <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                        <HiOutlineKey size={16} style={{ color: 'var(--color-accent)' }} />
                        <strong style={{ color: 'var(--color-primary)' }}>{h.nombre}</strong>
                      </div>
                      {h.superficieM2 && <span style={{ fontSize: '.8rem', color: 'var(--color-text-muted)' }}>{h.superficieM2} m²</span>}
                    </td>
                    <td>{h.capacidadAdultos} Ad., {h.capacidadNinos} Ni.</td>
                    <td>{h.numDormitorios} Dorm. / {h.numBanos} Baño(s)</td>
                    <td>{h.tieneAireAcondicionado ? '✓' : '—'}</td>
                    <td>{h.tieneCocina ? '✓' : '—'}</td>
                    <td>{h.admiteMascotas ? '✓' : '—'}</td>
                    <td>
                      <button 
                        className="admin-btn-delete"
                        onClick={() => eliminarHabitacion(h.habitacionId)}
                        title="Eliminar habitación"
                      >
                        <HiOutlineTrash size={16} />
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      )}
      
      {!propiedadSeleccionada && (
        <div style={{ padding: 60, textAlign: 'center', color: 'var(--color-text-muted)', border: '2px dashed var(--color-border)', borderRadius: 12 }}>
          <HiOutlineOfficeBuilding size={48} style={{ opacity: 0.5, marginBottom: 16 }} />
          <h3>Ninguna Propiedad Seleccionada</h3>
          <p>Usa el buscador superior para seleccionar un hotel y gestionar sus habitaciones.</p>
        </div>
      )}
    </div>
  );
}
