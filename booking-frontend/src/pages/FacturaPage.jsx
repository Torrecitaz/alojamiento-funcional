import { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import toast from 'react-hot-toast';
import api from '../services/api';

export default function FacturaPage() {
  const { codigo } = useParams();
  const [reserva, setReserva] = useState(null);
  const [pagos, setPagos] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchFacturaData = async () => {
      try {
        const resData = await api.get(`/reservas/${codigo}`);
        const reservaData = resData.data.datos;
        setReserva(reservaData);

        if (reservaData?.reservaId) {
          const pagosData = await api.get(`/pagos/por-reserva/${reservaData.reservaId}`);
          setPagos(pagosData.data.datos || []);
        }
      } catch {
        toast.error('Error al cargar la factura');
      } finally {
        setLoading(false);
      }
    };
    fetchFacturaData();
  }, [codigo]);

  const handlePrint = () => {
    window.print();
  };

  if (loading) return <div className="container" style={{ padding: '40px 0' }}><h3>Generando Factura...</h3></div>;
  if (!reserva) return <div className="container"><h3>Reserva no encontrada</h3></div>;

  return (
    <div className="container" style={{ padding: '40px 0', maxWidth: '800px' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 24 }}>
        <Link to="/mis-reservas" className="btn btn-outline" style={{ display: 'inline-block' }}>
          &larr; Volver a Mis Reservas
        </Link>
        <button onClick={handlePrint} className="btn btn-primary">
          🖨️ Imprimir Factura
        </button>
      </div>

      {/* Claude: Estiliza esta factura para que parezca un recibo oficial de "BookingPro" */}
      <div className="card" style={{ padding: 40, borderTop: '8px solid var(--color-primary)' }} id="factura-print-area">
        <div style={{ display: 'flex', justifyContent: 'space-between', borderBottom: '2px solid #eee', paddingBottom: 24, marginBottom: 24 }}>
          <div>
            <h1 style={{ margin: 0, color: 'var(--color-primary)' }}>BookingPro</h1>
            <p style={{ color: '#666', margin: 0 }}>Factura Oficial de Hospedaje</p>
          </div>
          <div style={{ textAlign: 'right' }}>
            <h2 style={{ margin: 0 }}>FACTURA #{reserva.codigoReserva}</h2>
            <p style={{ margin: 0 }}><strong>Fecha de Emisión:</strong> {new Date().toLocaleDateString()}</p>
            <p style={{ margin: 0 }}>
              <strong>Estado:</strong> 
              <span style={{ color: reserva.estado === 'Confirmada' ? 'green' : 'orange', marginLeft: 8 }}>
                {reserva.estado.toUpperCase()}
              </span>
            </p>
          </div>
        </div>

        <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 40 }}>
          <div>
            <h3>Datos del Cliente</h3>
            <p><strong>Nombre:</strong> {reserva.nombreCliente}</p>
          </div>
          <div style={{ textAlign: 'right' }}>
            <h3>Datos de la Propiedad</h3>
            <p><strong>Propiedad:</strong> {reserva.nombrePropiedad}</p>
            <p><strong>Check-in:</strong> {new Date(reserva.fechaCheckIn).toLocaleDateString()}</p>
            <p><strong>Check-out:</strong> {new Date(reserva.fechaCheckOut).toLocaleDateString()}</p>
          </div>
        </div>

        <table style={{ width: '100%', borderCollapse: 'collapse', marginBottom: 40 }}>
          <thead>
            <tr style={{ backgroundColor: '#f9fafb', borderBottom: '2px solid #e5e7eb' }}>
              <th style={{ padding: 12, textAlign: 'left' }}>Descripción</th>
              <th style={{ padding: 12, textAlign: 'right' }}>Total</th>
            </tr>
          </thead>
          <tbody>
            <tr style={{ borderBottom: '1px solid #e5e7eb' }}>
              <td style={{ padding: 12 }}>
                Estadía en {reserva.nombrePropiedad} <br/>
                <small style={{ color: '#666' }}>({reserva.numAdultos} adultos, {reserva.numNinos} niños)</small>
              </td>
              <td style={{ padding: 12, textAlign: 'right' }}>${(reserva.total / 1.15).toFixed(2)}</td>
            </tr>
            <tr style={{ borderBottom: '1px solid #e5e7eb' }}>
              <td style={{ padding: 12 }}>Impuestos (IVA 15%)</td>
              <td style={{ padding: 12, textAlign: 'right' }}>${(reserva.total - (reserva.total / 1.15)).toFixed(2)}</td>
            </tr>
          </tbody>
          <tfoot>
            <tr>
              <td style={{ padding: '16px 12px', textAlign: 'right', fontWeight: 'bold', fontSize: '1.2rem' }}>TOTAL PAGADO:</td>
              <td style={{ padding: '16px 12px', textAlign: 'right', fontWeight: 'bold', fontSize: '1.2rem', color: 'var(--color-primary)' }}>
                ${(reserva.total || 0).toFixed(2)} USD
              </td>
            </tr>
          </tfoot>
        </table>

        {pagos.length > 0 && (
          <div>
            <h4 style={{ borderBottom: '1px solid #eee', paddingBottom: 8 }}>Historial de Transacciones</h4>
            {pagos.map(p => (
              <p key={p.pagoId} style={{ fontSize: '0.9rem', color: '#666' }}>
                ✓ {new Date(p.fechaPago).toLocaleString()} - Ref: {p.referenciaPago || p.referenciaExterna || 'N/A'} - {p.tipoPago} - Monto: ${p.monto.toFixed(2)}
              </p>
            ))}
          </div>
        )}

      </div>
      
      {/* Estilos específicos para impresión */}
      <style>{`
        @media print {
          body * { visibility: hidden; }
          #factura-print-area, #factura-print-area * { visibility: visible; }
          #factura-print-area { position: absolute; left: 0; top: 0; width: 100%; }
        }
      `}</style>
    </div>
  );
}
