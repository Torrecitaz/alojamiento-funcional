import { useState, useEffect } from 'react';
import toast from 'react-hot-toast';
import api from '../../services/api';
import './AdminLayout.css';

export default function AdminReservas() {
  const [clienteId, setClienteId] = useState('');
  const [reservas, setReservas] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    cargarTodas();
  }, []);

  const cargarTodas = async () => {
    setLoading(true);
    try {
      const { data } = await api.get('/reservas/todas');
      setReservas(data.datos || []);
    } catch {
      toast.error('Error al cargar reservas.');
      setReservas([]);
    } finally {
      setLoading(false);
    }
  };

  const buscarPorCliente = async () => {
    if (!clienteId) { cargarTodas(); return; }
    setLoading(true);
    try {
      const { data } = await api.get(`/reservas/cliente/${clienteId}`);
      setReservas(data.datos || []);
    } catch {
      toast.error('Error al cargar reservas.');
      setReservas([]);
    } finally {
      setLoading(false);
    }
  };

  const cambiarEstado = async (reservaId, nuevoEstado) => {
    try {
      await api.patch(`/reservas/${reservaId}/estado`, { reservaId, nuevoEstado });
      toast.success(`Reserva actualizada a ${nuevoEstado}`);
      cargarTodas();
    } catch (err) {
      toast.error(err.response?.data?.mensaje || 'Error al cambiar estado.');
    }
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
        <h1 className="admin-page-title">Reservas</h1>
        <p className="admin-page-subtitle">Gestión completa de todas las reservas del sistema</p>
      </div>

      <div style={{ display: 'flex', gap: 12, marginBottom: 24 }}>
        <input
          type="number"
          className="input-field"
          placeholder="Filtrar por ID de cliente (vacío = todas)"
          value={clienteId}
          onChange={(e) => setClienteId(e.target.value)}
          style={{ maxWidth: 320 }}
        />
        <button className="btn btn-primary" onClick={buscarPorCliente} disabled={loading}>
          {loading ? 'Cargando...' : 'Filtrar'}
        </button>
        {clienteId && (
          <button className="btn btn-outline" onClick={() => { setClienteId(''); cargarTodas(); }}>
            Ver Todas
          </button>
        )}
      </div>

      <div className="admin-table-wrapper">
        <div className="admin-table-header">
          <h3 className="admin-table-title">Reservas ({reservas.length})</h3>
        </div>
        {loading ? (
          <div style={{ padding: 40, textAlign: 'center', color: 'var(--color-text-muted)' }}>
            Cargando reservas...
          </div>
        ) : reservas.length === 0 ? (
          <div style={{ padding: 40, textAlign: 'center', color: 'var(--color-text-muted)' }}>
            No hay reservas registradas.
          </div>
        ) : (
          <table className="admin-table">
            <thead>
              <tr>
                <th>Código</th>
                <th>Propiedad</th>
                <th>Check-in</th>
                <th>Check-out</th>
                <th>Total</th>
                <th>Estado</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {reservas.map((r) => (
                <tr key={r.reservaId || r.codigoReserva}>
                  <td style={{ fontWeight: 700, color: 'var(--color-primary)' }}>{r.codigoReserva}</td>
                  <td>{r.nombrePropiedad || 'N/D'}</td>
                  <td>{new Date(r.fechaCheckIn).toLocaleDateString('es')}</td>
                  <td>{new Date(r.fechaCheckOut).toLocaleDateString('es')}</td>
                  <td style={{ fontWeight: 600 }}>${r.total?.toFixed(2)}</td>
                  <td><span className={`badge ${estadoColor(r.estado)}`}>{r.estado}</span></td>
                  <td>
                    <div className="admin-actions">
                      {r.estado === 'Pendiente' && (
                        <>
                          <button className="admin-btn-edit" onClick={() => cambiarEstado(r.reservaId, 'Confirmada')}>Confirmar</button>
                          <button className="admin-btn-delete" onClick={() => cambiarEstado(r.reservaId, 'Cancelada')}>Cancelar</button>
                        </>
                      )}
                      {r.estado === 'Confirmada' && (
                        <button className="admin-btn-edit" onClick={() => cambiarEstado(r.reservaId, 'Completada')}>Completar</button>
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
