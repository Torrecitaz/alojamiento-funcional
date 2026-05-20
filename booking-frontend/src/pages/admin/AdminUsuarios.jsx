import { useState, useEffect } from 'react';
import { HiOutlineUserCircle, HiOutlineShieldCheck, HiOutlineBan, HiOutlineCheckCircle } from 'react-icons/hi';
import toast from 'react-hot-toast';
import api from '../../services/api';
import './AdminLayout.css';

export default function AdminUsuarios() {
  const [usuarios, setUsuarios] = useState([]);
  const [roles, setRoles] = useState([]);
  const [loading, setLoading] = useState(true);
  const [filterRol, setFilterRol] = useState('');

  useEffect(() => { load(); }, []);

  const load = async () => {
    setLoading(true);
    try {
      const [usersRes, rolesRes] = await Promise.allSettled([
        api.get('/auth/usuarios'),
        api.get('/auth/roles'),
      ]);
      if (usersRes.status === 'fulfilled') setUsuarios(usersRes.value.data.datos || []);
      if (rolesRes.status === 'fulfilled') setRoles(rolesRes.value.data.datos || []);
    } catch {
      toast.error('Error al cargar usuarios.');
    } finally {
      setLoading(false);
    }
  };

  const cambiarRol = async (userId, rolId) => {
    try {
      await api.patch(`/auth/usuarios/${userId}/rol`, { rolId: parseInt(rolId) });
      toast.success('Rol actualizado. El usuario debe reiniciar sesión para ver los cambios.', { duration: 5000 });
      load();
    } catch (err) {
      toast.error(err.response?.data?.mensaje || 'Error al cambiar rol.');
    }
  };

  const toggleEstado = async (userId, estadoActual) => {
    const nuevoEstado = !estadoActual;
    const confirmMsg = nuevoEstado ? '¿Activar esta cuenta?' : '¿Suspender esta cuenta? El usuario no podrá iniciar sesión.';
    if (!window.confirm(confirmMsg)) return;

    try {
      await api.patch(`/auth/usuarios/${userId}/estado`, { activo: nuevoEstado });
      toast.success(nuevoEstado ? 'Cuenta activada.' : 'Cuenta suspendida.');
      load();
    } catch (err) {
      toast.error(err.response?.data?.mensaje || 'Error al cambiar estado.');
    }
  };

  const filtered = filterRol
    ? usuarios.filter(u => u.rolNombre === filterRol)
    : usuarios;

  const rolColor = (rol) => {
    switch (rol?.toLowerCase()) {
      case 'administrador': return 'badge-danger';
      case 'colaborador': return 'badge-warning';
      case 'cliente': return 'badge-success';
      default: return 'badge-primary';
    }
  };

  return (
    <div>
      <div className="admin-page-header">
        <h1 className="admin-page-title">Gestión de Usuarios</h1>
        <p className="admin-page-subtitle">Administra cuentas, roles y permisos del sistema</p>
      </div>

      {/* Stats rápidos */}
      <div className="stats-grid" style={{ marginBottom: 24 }}>
        <div className="stat-card">
          <div className="stat-card-label">Total Usuarios</div>
          <div className="stat-card-value">{usuarios.length}</div>
        </div>
        <div className="stat-card">
          <div className="stat-card-label">Administradores</div>
          <div className="stat-card-value">{usuarios.filter(u => u.rolNombre === 'Administrador').length}</div>
        </div>
        <div className="stat-card">
          <div className="stat-card-label">Colaboradores</div>
          <div className="stat-card-value">{usuarios.filter(u => u.rolNombre === 'Colaborador').length}</div>
        </div>
        <div className="stat-card">
          <div className="stat-card-label">Clientes</div>
          <div className="stat-card-value">{usuarios.filter(u => u.rolNombre === 'Cliente').length}</div>
        </div>
      </div>

      {/* Filtro de rol */}
      <div style={{ display: 'flex', gap: 12, marginBottom: 20, alignItems: 'center' }}>
        <label style={{ fontWeight: 600, color: 'var(--color-text-muted)', fontSize: '.85rem' }}>Filtrar por rol:</label>
        <select
          className="input-field"
          value={filterRol}
          onChange={e => setFilterRol(e.target.value)}
          style={{ maxWidth: 200 }}
        >
          <option value="">Todos</option>
          {roles.map(r => (
            <option key={r.rolId} value={r.nombre}>{r.nombre}</option>
          ))}
        </select>
        {filterRol && (
          <button className="btn btn-outline" style={{ padding: '6px 14px', fontSize: '.82rem' }} onClick={() => setFilterRol('')}>
            Limpiar
          </button>
        )}
      </div>

      {/* Tabla de usuarios */}
      <div className="admin-table-wrapper">
        <div className="admin-table-header">
          <h3 className="admin-table-title">Usuarios ({filtered.length})</h3>
        </div>
        {loading ? (
          <div style={{ padding: 40, textAlign: 'center', color: 'var(--color-text-muted)' }}>Cargando usuarios...</div>
        ) : filtered.length === 0 ? (
          <div style={{ padding: 40, textAlign: 'center', color: 'var(--color-text-muted)' }}>No se encontraron usuarios.</div>
        ) : (
          <table className="admin-table">
            <thead>
              <tr>
                <th>ID</th>
                <th>Usuario</th>
                <th>Email</th>
                <th>Rol</th>
                <th>Estado</th>
                <th>Último Acceso</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map(u => (
                <tr key={u.usuarioId} style={{ opacity: u.estado ? 1 : 0.5 }}>
                  <td>{u.usuarioId}</td>
                  <td>
                    <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                      <HiOutlineUserCircle size={20} style={{ color: 'var(--color-primary)' }} />
                      <span style={{ fontWeight: 600 }}>{u.nombreCompleto || 'Sin nombre'}</span>
                    </div>
                  </td>
                  <td style={{ fontSize: '.88rem' }}>{u.email}</td>
                  <td>
                    <select
                      className="input-field"
                      value={u.rolId}
                      onChange={e => cambiarRol(u.usuarioId, e.target.value)}
                      style={{ padding: '4px 8px', fontSize: '.82rem', minWidth: 130 }}
                    >
                      {roles.map(r => (
                        <option key={r.rolId} value={r.rolId}>{r.nombre}</option>
                      ))}
                    </select>
                  </td>
                  <td>
                    <span className={`badge ${u.estado ? 'badge-success' : 'badge-danger'}`}>
                      {u.estado ? 'Activo' : 'Suspendido'}
                    </span>
                  </td>
                  <td style={{ fontSize: '.82rem', color: 'var(--color-text-muted)' }}>
                    {u.ultimoAcceso ? new Date(u.ultimoAcceso).toLocaleDateString('es') : 'Nunca'}
                  </td>
                  <td>
                    <div className="admin-actions">
                      {u.estado ? (
                        <button
                          className="admin-btn-delete"
                          onClick={() => toggleEstado(u.usuarioId, u.estado)}
                          title="Suspender cuenta"
                        >
                          <HiOutlineBan size={14} /> Suspender
                        </button>
                      ) : (
                        <button
                          className="admin-btn-edit"
                          onClick={() => toggleEstado(u.usuarioId, u.estado)}
                          title="Activar cuenta"
                        >
                          <HiOutlineCheckCircle size={14} /> Activar
                        </button>
                      )}
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
