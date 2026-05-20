import { useState, useEffect } from 'react';
import { HiOutlinePlus, HiOutlineOfficeBuilding } from 'react-icons/hi';
import toast from 'react-hot-toast';
import api from '../../services/api';
import './AdminLayout.css';

export default function AdminColaboradores() {
  const [colaboradores, setColaboradores] = useState([]);
  const [usuarios, setUsuarios] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [submitting, setSubmitting] = useState(false);

  const [formData, setFormData] = useState({
    usuarioId: '',
    nombreEmpresa: '',
    telefono: ''
  });

  useEffect(() => { load(); }, []);

  const load = async () => {
    setLoading(true);
    try {
      const [colabRes, userRes] = await Promise.allSettled([
        api.get('/colaboradores'),
        api.get('/auth/usuarios')
      ]);

      if (colabRes.status === 'fulfilled') setColaboradores(colabRes.value.data.datos || []);
      
      if (userRes.status === 'fulfilled') {
        // Filtrar usuarios que tienen rol Colaborador
        const allUsers = userRes.value.data.datos || [];
        setUsuarios(allUsers.filter(u => u.rolNombre === 'Colaborador'));
      }
    } catch {
      toast.error('Error al cargar datos.');
    } finally {
      setLoading(false);
    }
  };

  const handleInputChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSubmitting(true);
    try {
      await api.post('/colaboradores', {
        usuarioId: parseInt(formData.usuarioId),
        nombreEmpresa: formData.nombreEmpresa,
        telefono: formData.telefono
      });
      toast.success('Colaborador registrado.');
      setShowForm(false);
      setFormData({ usuarioId: '', nombreEmpresa: '', telefono: '' });
      load();
    } catch (err) {
      toast.error(err.response?.data?.mensaje || 'Error al registrar.');
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('¿Estás seguro de eliminar este colaborador? Las propiedades podrían quedar sin dueño.')) return;
    try {
      await api.delete(`/colaboradores/${id}`);
      toast.success('Colaborador eliminado.');
      load();
    } catch { toast.error('Error al eliminar.'); }
  };

  return (
    <div>
      <div className="admin-page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <div>
          <h1 className="admin-page-title">Colaboradores (Socios)</h1>
          <p className="admin-page-subtitle">Gestión de socios que publican hoteles en el sistema</p>
        </div>
        <button className="btn btn-primary" onClick={() => setShowForm(!showForm)}>
          <HiOutlinePlus size={18} /> {showForm ? 'Ocultar Formulario' : 'Nuevo Socio'}
        </button>
      </div>

      {showForm && (
        <div className="card" style={{ padding: 24, marginBottom: 24, border: '1px solid var(--color-border)' }}>
          <h3 style={{ marginTop: 0, marginBottom: 20 }}>Registrar Nuevo Socio</h3>
          <form onSubmit={handleSubmit}>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16, marginBottom: 16 }}>
              <div>
                <label className="form-label">Cuenta de Usuario (Rol: Colaborador)</label>
                <select required className="input-field" name="usuarioId" value={formData.usuarioId} onChange={handleInputChange}>
                  <option value="">-- Selecciona el usuario --</option>
                  {usuarios.map(u => (
                    <option key={u.usuarioId} value={u.usuarioId}>{u.nombreCompleto || u.email}</option>
                  ))}
                </select>
                {usuarios.length === 0 && <span style={{ fontSize: '.8rem', color: 'var(--color-danger)' }}>No hay usuarios con rol "Colaborador". Asígnales el rol en Gestión de Usuarios primero.</span>}
              </div>
              <div>
                <label className="form-label">Nombre de la Empresa o Cadena</label>
                <input required type="text" className="input-field" name="nombreEmpresa" value={formData.nombreEmpresa} onChange={handleInputChange} placeholder="Ej. Hoteles Sunset S.A." />
              </div>
            </div>

            <div style={{ marginBottom: 20 }}>
              <label className="form-label">Teléfono de Contacto Comercial</label>
              <input required type="text" className="input-field" name="telefono" value={formData.telefono} onChange={handleInputChange} placeholder="+593 99 123 4567" style={{ maxWidth: 300 }} />
            </div>

            <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 12 }}>
              <button type="button" className="btn btn-outline" onClick={() => setShowForm(false)}>Cancelar</button>
              <button type="submit" className="btn btn-primary" disabled={submitting || usuarios.length === 0}>
                {submitting ? 'Guardando...' : 'Registrar Socio'}
              </button>
            </div>
          </form>
        </div>
      )}

      <div className="admin-table-wrapper">
        <div className="admin-table-header">
          <h3 className="admin-table-title">Socios Registrados ({colaboradores.length})</h3>
        </div>
        {loading ? (
          <div style={{ padding: 40, textAlign: 'center', color: 'var(--color-text-muted)' }}>Cargando...</div>
        ) : colaboradores.length === 0 ? (
          <div style={{ padding: 40, textAlign: 'center', color: 'var(--color-text-muted)' }}>No hay colaboradores registrados.</div>
        ) : (
          <table className="admin-table">
            <thead>
              <tr>
                <th>ID Socio</th>
                <th>Usuario Relacionado</th>
                <th>Empresa / Cadena</th>
                <th>Contacto</th>
                <th>Propiedades</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {colaboradores.map((c) => (
                <tr key={c.colaboradorId}>
                  <td>{c.colaboradorId}</td>
                  <td style={{ fontSize: '.85rem', color: 'var(--color-text-muted)' }}>Usr ID: {c.usuarioId}</td>
                  <td>
                    <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                      <HiOutlineOfficeBuilding style={{ color: 'var(--color-primary)' }} />
                      <strong style={{ color: 'var(--color-text)' }}>{c.nombreEmpresa || c.nombreCompleto || 'N/D'}</strong>
                    </div>
                  </td>
                  <td>{c.telefono || c.email || '—'}</td>
                  <td><span className="badge badge-primary">{c.totalPropiedades || 0}</span></td>
                  <td>
                    <div className="admin-actions">
                      <button className="admin-btn-delete" onClick={() => handleDelete(c.colaboradorId)}>Eliminar</button>
                    </div>
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
