import { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { HiOutlineStar, HiOutlineLocationMarker, HiOutlineArrowLeft } from 'react-icons/hi';
import { Bed, Bath, PawPrint, Snowflake, ChefHat, Ruler } from 'lucide-react';
import toast from 'react-hot-toast';
import api from '../services/api';
import useAuthStore from '../store/useAuthStore';
import './PropiedadDetallePage.css';

export default function PropiedadDetallePage() {
  const { id } = useParams();
  const { isAuthenticated, user } = useAuthStore();
  const [propiedad, setPropiedad] = useState(null);
  const [habitaciones, setHabitaciones] = useState([]);
  const [loading, setLoading] = useState(true);

  // Booking Form State
  const [checkIn, setCheckIn] = useState('');
  const [checkOut, setCheckOut] = useState('');
  const [adultos, setAdultos] = useState(1);
  const [ninos, setNinos] = useState(0);
  const [mascotas, setMascotas] = useState(false);
  const [selectedHabitacion, setSelectedHabitacion] = useState('');
  const [bookingLoading, setBookingLoading] = useState(false);

  useEffect(() => {
    fetchData();
  }, [id]);

  const fetchData = async () => {
    setLoading(true);
    try {
      const [propRes, habRes] = await Promise.all([
        api.get(`/propiedades/${id}`),
        api.get(`/habitaciones/por-propiedad/${id}`).catch(() => ({ data: { datos: [] } })),
      ]);
      setPropiedad(propRes.data.datos);
      setHabitaciones(habRes.data.datos || []);
    } catch {
      toast.error('Error al cargar la propiedad.');
    } finally {
      setLoading(false);
    }
  };

  const handleBookingSubmit = async (e) => {
    e.preventDefault();
    if (!selectedHabitacion) return toast.error('Selecciona una habitación.');
    if (!checkIn || !checkOut) return toast.error('Selecciona fechas válidas.');

    setBookingLoading(true);
    try {
      const payload = {
        clienteId: user.clienteId || parseInt(user.id),
        propiedadId: parseInt(id),
        habitacionIds: [parseInt(selectedHabitacion)],
        fechaCheckIn: checkIn,
        fechaCheckOut: checkOut,
        numAdultos: parseInt(adultos),
        numNinos: parseInt(ninos),
        llevaMascotas: mascotas,
        monedaId: 1, 
        metodoPagoId: 1 
      };

      const result = await api.post('/reservas', payload);
      const nuevaReserva = result.data.datos;
      
      toast.success('¡Reserva creada! Por favor completa el pago.');
      window.location.href = `/checkout/${nuevaReserva.codigoReserva}`; // Or use navigate if using useNavigate
    } catch (err) {
      toast.error(err.response?.data?.mensaje || 'Error al crear la reserva.');
    } finally {
      setBookingLoading(false);
    }
  };

  const renderStars = (n) =>
    Array.from({ length: 5 }, (_, i) => (
      <HiOutlineStar key={i} style={{ fill: i < n ? 'var(--color-accent)' : 'none', color: i < n ? 'var(--color-accent)' : 'var(--color-text-muted)', fontSize: '1.2rem' }} />
    ));

  if (loading) {
    return (
      <div className="detalle-page container">
        <div className="skeleton" style={{ height: 340, borderRadius: 16, marginBottom: 32 }} />
        <div className="skeleton" style={{ height: 28, width: '40%', marginBottom: 12 }} />
        <div className="skeleton" style={{ height: 18, width: '30%', marginBottom: 24 }} />
        <div className="skeleton" style={{ height: 100, marginBottom: 24 }} />
      </div>
    );
  }

  if (!propiedad) {
    return (
      <div className="detalle-page container">
        <div className="empty-state">
          <h3>Propiedad no encontrada</h3>
          <Link to="/propiedades" className="btn btn-outline">Volver al catálogo</Link>
        </div>
      </div>
    );
  }

  return (
    <div className="detalle-page container">
      <Link to="/propiedades" className="back-link">
        <HiOutlineArrowLeft /> Volver al catálogo
      </Link>

      <div className="detalle-hero">
        <div className="detalle-hero-bg">
          <HiOutlineLocationMarker size={48} />
          <span>{propiedad.ciudad}</span>
        </div>
        <div className="detalle-hero-overlay">
          <div className="stars">{renderStars(propiedad.estrellas)}</div>
          <span className={`badge ${propiedad.estado === 'Activa' ? 'badge-success' : 'badge-warning'}`}>
            {propiedad.estado}
          </span>
        </div>
      </div>

      <div className="detalle-content">
        <div className="detalle-main">
          <h1 className="detalle-title">{propiedad.nombre}</h1>
          <p className="detalle-location">
            <HiOutlineLocationMarker /> {propiedad.ciudad} — {propiedad.direccion}
          </p>
          <p className="detalle-desc">{propiedad.descripcion || 'Sin descripción disponible.'}</p>

          <div className="detalle-stats">
            <div className="stat-item">
              <span className="stat-value">★ {propiedad.calificacionPromedio?.toFixed(1)}</span>
              <span className="stat-label">Calificación</span>
            </div>
            <div className="stat-item">
              <span className="stat-value">{propiedad.totalResenas}</span>
              <span className="stat-label">Reseñas</span>
            </div>
            <div className="stat-item">
              <span className="stat-value">{propiedad.admiteMascotas ? 'Sí' : 'No'}</span>
              <span className="stat-label">Mascotas</span>
            </div>
          </div>

          {/* Habitaciones */}
          <h2 className="section-subtitle-alt">Habitaciones Disponibles</h2>
          {habitaciones.length === 0 ? (
            <p className="no-data">No hay habitaciones registradas aún.</p>
          ) : (
            <div className="habitaciones-grid">
              {habitaciones.map((h) => (
                <div className="card hab-card" key={h.habitacionId}>
                  <div className="hab-header">
                    <h3>{h.nombre}</h3>
                    <span className={`badge ${h.estado ? 'badge-success' : 'badge-danger'}`}>
                      {h.estado ? 'Disponible' : 'Ocupada'}
                    </span>
                  </div>
                  <p className="hab-desc">{h.descripcion || 'Sin descripción.'}</p>
                  <div className="hab-amenities">
                    <span><Bed size={16} /> {h.capacidadAdultos}A / {h.capacidadNinos}N</span>
                    <span><Bath size={16} /> {h.numBanos} baños</span>
                    {h.superficieM2 && <span><Ruler size={16} /> {h.superficieM2}m²</span>}
                    {h.admiteMascotas && <span className="amenity-highlight"><PawPrint size={16} /> Mascotas</span>}
                    {h.tieneAireAcondicionado && <span><Snowflake size={16} /> A/C</span>}
                    {h.tieneCocina && <span><ChefHat size={16} /> Cocina</span>}
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        <aside className="detalle-sidebar">
          <div className="card booking-card">
            <h3>Reservar esta propiedad</h3>
            {!isAuthenticated ? (
              <>
                <p>Inicia sesión para crear una reserva segura con protección anti-overbooking.</p>
                <Link to="/login" className="btn btn-accent" style={{ width: '100%', marginTop: 16 }}>
                  Iniciar Sesión
                </Link>
              </>
            ) : (
              <form className="booking-form" onSubmit={handleBookingSubmit}>
                <div className="form-group">
                  <label>Habitación</label>
                  <select 
                    className="input-field" 
                    value={selectedHabitacion} 
                    onChange={e => setSelectedHabitacion(e.target.value)}
                    required
                  >
                    <option value="">Selecciona una habitación</option>
                    {habitaciones.filter(h => h.estado).map(h => (
                      <option key={h.habitacionId} value={h.habitacionId}>{h.nombre} — {h.capacidadAdultos}A/{h.capacidadNinos}N, {h.numBanos} baño(s)</option>
                    ))}
                  </select>
                </div>
                
                <div className="form-row">
                  <div className="form-group">
                    <label>Check-in</label>
                    <input type="date" className="input-field" value={checkIn} onChange={e => setCheckIn(e.target.value)} required />
                  </div>
                  <div className="form-group">
                    <label>Check-out</label>
                    <input type="date" className="input-field" value={checkOut} onChange={e => setCheckOut(e.target.value)} required />
                  </div>
                </div>

                <div className="form-row">
                  <div className="form-group">
                    <label>Adultos</label>
                    <input type="number" min="1" className="input-field" value={adultos} onChange={e => setAdultos(e.target.value)} required />
                  </div>
                  <div className="form-group">
                    <label>Niños</label>
                    <input type="number" min="0" className="input-field" value={ninos} onChange={e => setNinos(e.target.value)} required />
                  </div>
                </div>

                <div className="form-group" style={{ display: 'flex', alignItems: 'center', gap: 8, marginTop: 12, marginBottom: 24 }}>
                  <input type="checkbox" id="mascotas" checked={mascotas} onChange={e => setMascotas(e.target.checked)} />
                  <label htmlFor="mascotas" style={{ margin: 0 }}>Llevo mascotas</label>
                </div>

                <button type="submit" className="btn btn-accent" style={{ width: '100%' }} disabled={bookingLoading}>
                  {bookingLoading ? 'Procesando...' : 'Confirmar Reserva'}
                </button>
              </form>
            )}
          </div>
        </aside>
      </div>
    </div>
  );
}
