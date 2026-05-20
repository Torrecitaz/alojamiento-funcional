import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { HiOutlineCreditCard, HiOutlineCash, HiOutlineShieldCheck, HiOutlineCalendar, HiOutlineUsers, HiOutlineHome } from 'react-icons/hi';
import toast from 'react-hot-toast';
import api from '../services/api';
import './CheckoutPage.css';

const METODOS_PAGO = [
  { id: 'Tarjeta', label: 'Tarjeta de Crédito/Débito', icon: <HiOutlineCreditCard size={24} /> },
  { id: 'EnSitio', label: 'Pago en Sitio (Check-in)', icon: <HiOutlineCash size={24} /> },
];

export default function CheckoutPage() {
  const { codigo } = useParams();
  const navigate = useNavigate();
  const [reserva, setReserva] = useState(null);
  const [loading, setLoading] = useState(true);
  const [processing, setProcessing] = useState(false);
  const [metodoPago, setMetodoPago] = useState('Tarjeta');

  useEffect(() => {
    const fetchReserva = async () => {
      try {
        const { data } = await api.get(`/reservas/${codigo}`);
        setReserva(data.datos);
        
        if (data.datos.estado === 'Confirmada') {
          navigate(`/factura/${codigo}`);
        }
      } catch {
        toast.error('Error al cargar la reserva');
        navigate('/mis-reservas');
      } finally {
        setLoading(false);
      }
    };
    fetchReserva();
  }, [codigo, navigate]);

  const handlePayment = async (e) => {
    e.preventDefault();
    setProcessing(true);
    
    const idempotencyKey = `pay_${codigo}_${Date.now()}`;

    try {
      const payload = {
        reservaId: reserva.reservaId,
        monto: reserva.total,
        monedaId: 1,
        tipoPago: metodoPago,
        referenciaPago: `BP-${Date.now()}`
      };

      await api.post('/pagos', payload, {
        headers: { 'Idempotency-Key': idempotencyKey }
      });

      toast.success('¡Pago procesado exitosamente!');
      navigate(`/factura/${codigo}`);
    } catch (err) {
      toast.error(err.response?.data?.mensaje || 'El pago ha fallado.');
    } finally {
      setProcessing(false);
    }
  };

  if (loading) {
    return (
      <div className="checkout-page container">
        <div className="checkout-loading">
          <div className="spinner" />
          <p>Cargando información de tu reserva...</p>
        </div>
      </div>
    );
  }

  if (!reserva) return null;

  const noches = Math.ceil(
    (new Date(reserva.fechaCheckOut) - new Date(reserva.fechaCheckIn)) / (1000 * 60 * 60 * 24)
  );

  return (
    <div className="checkout-page container">
      <div className="checkout-header">
        <HiOutlineShieldCheck size={28} />
        <div>
          <h1>Checkout Seguro</h1>
          <p className="checkout-subtitle">Reserva #{reserva.codigoReserva}</p>
        </div>
      </div>

      <div className="checkout-grid">
        {/* Columna izquierda: Resumen */}
        <div className="checkout-summary">
          <div className="card summary-card">
            <h3 className="summary-title">Resumen de Reserva</h3>
            
            <div className="summary-item">
              <HiOutlineHome size={18} />
              <div>
                <span className="summary-label">Propiedad</span>
                <strong>{reserva.propiedadNombre || `Propiedad #${reserva.propiedadId}`}</strong>
              </div>
            </div>

            <div className="summary-item">
              <HiOutlineCalendar size={18} />
              <div>
                <span className="summary-label">Fechas</span>
                <strong>
                  {new Date(reserva.fechaCheckIn).toLocaleDateString('es-ES', { day: '2-digit', month: 'short', year: 'numeric' })}
                  {' → '}
                  {new Date(reserva.fechaCheckOut).toLocaleDateString('es-ES', { day: '2-digit', month: 'short', year: 'numeric' })}
                </strong>
                <span className="summary-detail">{noches} noche{noches !== 1 ? 's' : ''}</span>
              </div>
            </div>

            <div className="summary-item">
              <HiOutlineUsers size={18} />
              <div>
                <span className="summary-label">Huéspedes</span>
                <strong>{reserva.numAdultos} adulto{reserva.numAdultos !== 1 ? 's' : ''}{reserva.numNinos > 0 ? `, ${reserva.numNinos} niño${reserva.numNinos !== 1 ? 's' : ''}` : ''}</strong>
              </div>
            </div>

            <div className="summary-divider" />

            <div className="summary-totals">
              <div className="total-row">
                <span>Subtotal ({noches} noches × {reserva.numHabitaciones} hab.)</span>
                <span>${reserva.subTotal?.toFixed(2) || reserva.total?.toFixed(2)}</span>
              </div>
              {(reserva.descuento ?? 0) > 0 && (
                <div className="total-row discount">
                  <span>Descuento</span>
                  <span>-${reserva.descuento.toFixed(2)}</span>
                </div>
              )}
              <div className="total-row total-final">
                <span>Total a pagar</span>
                <span>${reserva.total.toFixed(2)} USD</span>
              </div>
            </div>
          </div>
        </div>

        {/* Columna derecha: Formulario de pago */}
        <div className="checkout-payment">
          <div className="card payment-card">
            <h3 className="payment-title">Método de Pago</h3>

            <div className="payment-methods">
              {METODOS_PAGO.map(m => (
                <label
                  key={m.id}
                  className={`payment-method-option ${metodoPago === m.id ? 'selected' : ''}`}
                >
                  <input
                    type="radio"
                    name="metodoPago"
                    value={m.id}
                    checked={metodoPago === m.id}
                    onChange={() => setMetodoPago(m.id)}
                  />
                  <span className="pm-icon">{m.icon}</span>
                  <span className="pm-label">{m.label}</span>
                </label>
              ))}
            </div>

            {metodoPago === 'Tarjeta' && (
              <div className="card-form">
                <div className="form-group">
                  <label>Número de Tarjeta</label>
                  <input type="text" className="input-field" placeholder="4242 4242 4242 4242" defaultValue="4242 4242 4242 4242" />
                </div>
                <div className="form-row">
                  <div className="form-group">
                    <label>Expiración</label>
                    <input type="text" className="input-field" placeholder="MM/AA" defaultValue="12/28" />
                  </div>
                  <div className="form-group">
                    <label>CVV</label>
                    <input type="text" className="input-field" placeholder="***" defaultValue="123" />
                  </div>
                </div>
                <div className="form-group">
                  <label>Titular</label>
                  <input type="text" className="input-field" placeholder="Nombre en la tarjeta" defaultValue="Mathias" />
                </div>
              </div>
            )}

            {metodoPago === 'EnSitio' && (
              <div className="card-form ensitio-notice">
                <HiOutlineCash size={32} />
                <p>El pago se realizará directamente al momento del check-in en la propiedad. Tu reserva quedará en estado <strong>"Confirmada"</strong> y se esperará el pago presencial.</p>
              </div>
            )}

            <form onSubmit={handlePayment}>
              <button 
                type="submit" 
                className="btn btn-accent checkout-btn"
                disabled={processing}
              >
                {processing ? (
                  <><span className="spinner-sm" /> Procesando Pago Seguro...</>
                ) : (
                  <>
                    <HiOutlineShieldCheck size={20} />
                    {metodoPago === 'EnSitio' ? 'Confirmar Reserva' : `Pagar $${reserva.total.toFixed(2)} USD`}
                  </>
                )}
              </button>
            </form>

            <p className="security-notice">
              <HiOutlineShieldCheck size={14} /> Transacción protegida con encriptación SSL y validación anti-duplicado.
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
