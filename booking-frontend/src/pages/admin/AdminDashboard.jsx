import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import api from '../../services/api';
import useAuthStore from '../../store/useAuthStore';
import './AdminLayout.css';

export default function AdminDashboard() {
  const { user } = useAuthStore();
  const esAdmin = user?.roles?.includes('Administrador');

  const [stats, setStats] = useState({ propiedades: 0, reservas: 0, clientes: 0, colaboradores: 0 });
  const [recentReservas, setRecentReservas] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadDashboard();
    // eslint-disable-next-line
  }, []);

  const loadDashboard = async () => {
    try {
      const endpoints = [
        esAdmin ? api.get('/propiedades/buscar') : api.get(`/propiedades/colaborador/${user?.colaboradorId}`),
        esAdmin ? api.get('/reservas/todas') : Promise.resolve({ data: { datos: [] } }),
        esAdmin ? api.get('/clientes') : Promise.resolve({ data: { datos: [] } }),
        esAdmin ? api.get('/colaboradores') : Promise.resolve({ data: { datos: [] } })
      ];

      const [propRes, reservasRes, clientesRes, colabRes] = await Promise.allSettled(endpoints);

      const reservasList = reservasRes.status === 'fulfilled' ? (reservasRes.value.data.datos || []) : [];

      setStats({
        propiedades: propRes.status === 'fulfilled' ? (propRes.value.data.datos?.totalRecords || propRes.value.data.datos?.items?.length || propRes.value.data.datos?.length || 0) : 0,
        reservas: reservasList.length,
        clientes: clientesRes.status === 'fulfilled' ? (clientesRes.value.data.datos?.totalCount || clientesRes.value.data.datos?.items?.length || 0) : 0,
        colaboradores: colabRes.status === 'fulfilled' ? (colabRes.value.data.datos?.length || 0) : 0,
      });

      setRecentReservas(reservasList.slice(0, 5));
    } catch { /* silent */ }
    finally { setLoading(false); }
  };

  const estadoColor = (e) => {
    switch (e?.toLowerCase()) {
      case 'confirmada': return 'badge-success';
      case 'pendiente': return 'badge-warning';
      case 'cancelada': return 'badge-danger';
      default: return 'badge-primary';
    }
  };

  return (
    <div>
      <div className="admin-page-header">
        <h1 className="admin-page-title">Dashboard</h1>
        <p className="admin-page-subtitle">Resumen general del sistema BookingPro</p>
      </div>

      <div className="stats-grid">
        <div className="stat-card">
          <div className="stat-card-label">Propiedades</div>
          <div className="stat-card-value">{stats.propiedades}</div>
          <div className="stat-card-note">Registradas en el sistema</div>
        </div>
        <div className="stat-card">
          <div className="stat-card-label">Reservas</div>
          <div className="stat-card-value">{stats.reservas}</div>
          <div className="stat-card-note">Total de reservaciones</div>
        </div>
        <div className="stat-card">
          <div className="stat-card-label">Clientes</div>
          <div className="stat-card-value">{stats.clientes}</div>
          <div className="stat-card-note">Usuarios registrados</div>
        </div>
        <div className="stat-card">
          <div className="stat-card-label">Colaboradores</div>
          <div className="stat-card-value">{stats.colaboradores}</div>
          <div className="stat-card-note">Socios del sistema</div>
        </div>
      </div>

      {/* Últimas Reservas */}
      <div className="admin-table-wrapper" style={{ marginBottom: 24 }}>
        <div className="admin-table-header">
          <h3 className="admin-table-title">Últimas Reservas</h3>
          <Link to="/admin/reservas" className="btn btn-outline" style={{ padding: '6px 16px', fontSize: '.82rem' }}>
            Ver Todas →
          </Link>
        </div>
        {loading ? (
          <div style={{ padding: 40, textAlign: 'center', color: 'var(--color-text-muted)' }}>Cargando...</div>
        ) : recentReservas.length === 0 ? (
          <div style={{ padding: 40, textAlign: 'center', color: 'var(--color-text-muted)' }}>
            Aún no hay reservas en el sistema.
          </div>
        ) : (
          <table className="admin-table">
            <thead>
              <tr>
                <th>Código</th>
                <th>Propiedad</th>
                <th>Check-in</th>
                <th>Total</th>
                <th>Estado</th>
              </tr>
            </thead>
            <tbody>
              {recentReservas.map((r) => (
                <tr key={r.reservaId || r.codigoReserva}>
                  <td style={{ fontWeight: 700, color: 'var(--color-primary)' }}>{r.codigoReserva}</td>
                  <td>{r.nombrePropiedad || 'N/D'}</td>
                  <td>{new Date(r.fechaCheckIn).toLocaleDateString('es')}</td>
                  <td style={{ fontWeight: 600 }}>${r.total?.toFixed(2)}</td>
                  <td><span className={`badge ${estadoColor(r.estado)}`}>{r.estado}</span></td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {/* Endpoints del Sistema */}
      <div className="admin-table-wrapper">
        <div className="admin-table-header">
          <h3 className="admin-table-title">Arquitectura de Servicios</h3>
        </div>
        <table className="admin-table">
          <thead>
            <tr>
              <th>Endpoint</th>
              <th>Protocolo</th>
              <th>Estado</th>
            </tr>
          </thead>
          <tbody>
            {[
              { endpoint: '/api/v1/propiedades/buscar', proto: 'REST', status: 'Activo' },
              { endpoint: '/api/v1/reservas', proto: 'REST + EDA', status: 'Activo' },
              { endpoint: '/api/v1/pagos (Idempotency)', proto: 'REST', status: 'Activo' },
              { endpoint: '/api/v1/clientes', proto: 'REST', status: 'Activo' },
              { endpoint: '/graphql', proto: 'GraphQL', status: 'Activo' },
              { endpoint: 'gRPC ClientesService', proto: 'gRPC / HTTP2', status: 'Activo' },
            ].map((r, i) => (
              <tr key={i}>
                <td><code style={{ fontSize: '.82rem' }}>{r.endpoint}</code></td>
                <td><span className="badge badge-primary">{r.proto}</span></td>
                <td><span className="badge badge-success">{r.status}</span></td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
