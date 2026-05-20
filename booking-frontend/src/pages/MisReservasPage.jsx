import { useState, useEffect } from 'react';
import { HiOutlineCalendar, HiOutlineEye } from 'react-icons/hi';
import toast from 'react-hot-toast';
import api from '../services/api';
import useAuthStore from '../store/useAuthStore';
import './MisReservasPage.css';

export default function MisReservasPage() {
  const [reservas, setReservas] = useState([]);
  const [loading, setLoading] = useState(true);
  const { isAuthenticated, user } = useAuthStore();

  useEffect(() => {
    if (isAuthenticated) fetchReservas();
    else setLoading(false);
  }, [isAuthenticated]);

  const fetchReservas = async () => {
    setLoading(true);
    try {
      const { data } = await api.get(`/reservas/cliente/${user.clienteId || user.id}`);
      setReservas(data.datos || []);
    } catch {
      toast.error('No se pudieron cargar las reservas.');
    } finally {
      setLoading(false);
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

  if (!isAuthenticated) {
    return (
      <div className="reservas-page container">
        <div className="empty-state">
          <h3>Inicia sesión para ver tus reservas</h3>
        </div>
      </div>
    );
  }

  return (
    <div className="reservas-page container">
      <h1 className="page-title">Mis Reservas</h1>
      <p className="page-subtitle">Consulta el historial y estado de tus reservas.</p>

      {loading ? (
        <div className="reservas-list">
          {[1,2,3].map(i => (
            <div className="card reserva-skeleton" key={i}>
              <div className="skeleton" style={{ height: 22, width: '30%', marginBottom: 10 }} />
              <div className="skeleton" style={{ height: 16, width: '50%', marginBottom: 8 }} />
              <div className="skeleton" style={{ height: 16, width: '40%' }} />
            </div>
          ))}
        </div>
      ) : reservas.length === 0 ? (
        <div className="empty-state">
          <HiOutlineCalendar size={48} />
          <h3>No tienes reservas aún</h3>
          <p>Explora nuestras propiedades y haz tu primera reserva.</p>
        </div>
      ) : (
        <div className="reservas-list">
          {reservas.map((r) => (
            <div className="card reserva-card" key={r.reservaId}>
              <div className="reserva-top">
                <div>
                  <h3 className="reserva-code">{r.codigoReserva}</h3>
                  <p className="reserva-prop">{r.nombrePropiedad}</p>
                </div>
                <span className={`badge ${estadoColor(r.estado)}`}>{r.estado}</span>
              </div>
              <div className="reserva-details">
                <div className="reserva-detail">
                  <span className="detail-label">Check-in</span>
                  <span className="detail-value">{new Date(r.fechaCheckIn).toLocaleDateString('es')}</span>
                </div>
                <div className="reserva-detail">
                  <span className="detail-label">Check-out</span>
                  <span className="detail-value">{new Date(r.fechaCheckOut).toLocaleDateString('es')}</span>
                </div>
                <div className="reserva-detail">
                  <span className="detail-label">Noches</span>
                  <span className="detail-value">{r.nochesTotal}</span>
                </div>
                <div className="reserva-detail">
                  <span className="detail-label">Total</span>
                  <span className="detail-value detail-price">${r.total?.toFixed(2)}</span>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
