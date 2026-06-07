import { useEffect, useRef } from 'react';
import { HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';
import toast from 'react-hot-toast';
import useAuthStore from '../store/useAuthStore';

export default function useSignalR() {
  const { isAuthenticated, user } = useAuthStore();
  const connectionRef = useRef(null);

  useEffect(() => {
    // Only connect if the user is authenticated (can be client, collaborator, or admin)
    if (!isAuthenticated) {
      if (connectionRef.current) {
        connectionRef.current.stop();
        connectionRef.current = null;
      }
      return;
    }

    const connectionUrl = `${window.location.origin}/bookingHub`;
    const connection = new HubConnectionBuilder()
      .withUrl(connectionUrl)
      .configureLogging(LogLevel.Warning)
      .withAutomaticReconnect()
      .build();

    connectionRef.current = connection;

    // Listeners for real-time updates
    connection.on('OnReservaCreated', (reserva) => {
      console.log('⚡ SignalR: OnReservaCreated', reserva);
      const isMyReserva = user && (user.clienteId === reserva.clienteId || user.id === reserva.clienteId.toString());
      const isColabOrAdmin = user && (user.roles?.includes('Administrador') || user.roles?.includes('Colaborador'));
      
      if (isMyReserva) {
        toast.success(`¡Tu reserva #${reserva.codigoReserva} ha sido pre-registrada! Paga para confirmar.`, { icon: '📩' });
      } else if (isColabOrAdmin) {
        toast.success(`Nueva reserva pre-registrada: #${reserva.codigoReserva} (Total: $${reserva.total.toFixed(2)})`, { icon: '🛎️' });
      }
    });

    connection.on('OnReservaConfirmed', (reserva) => {
      console.log('⚡ SignalR: OnReservaConfirmed', reserva);
      const isColabOrAdmin = user && (user.roles?.includes('Administrador') || user.roles?.includes('Colaborador'));
      
      toast.success(`¡Reserva #${reserva.codigoReserva} CONFIRMADA exitosamente!`, {
        icon: '✅',
        style: { border: '2px solid #10B981', background: '#064E3B', color: '#fff' }
      });
    });

    connection.on('OnReservaCancelled', (reserva) => {
      console.log('⚡ SignalR: OnReservaCancelled', reserva);
      toast.error(`Reserva #${reserva.codigoReserva} ha sido CANCELADA.`, {
        icon: '❌',
        style: { border: '2px solid #EF4444', background: '#7F1D1D', color: '#fff' }
      });
    });

    connection.on('OnAvailabilityChanged', (change) => {
      console.log('⚡ SignalR: OnAvailabilityChanged', change);
      // Optional FOMO trigger or log
    });

    connection.on('OnAlojamientoEstadoChanged', (change) => {
      console.log('⚡ SignalR: OnAlojamientoEstadoChanged', change);
      const isColabOrAdmin = user && (user.roles?.includes('Administrador') || user.roles?.includes('Colaborador'));
      if (isColabOrAdmin) {
        toast(`Alojamiento #${change.alojamientoId} cambió su estado a: ${change.estado}`, { icon: '🏨' });
      }
    });

    // Start connection
    connection.start()
      .then(() => {
        console.log('🔌 Global SignalR connection active.');
      })
      .catch((err) => {
        console.error('❌ Global SignalR connection failed:', err);
      });

    // Clean up
    return () => {
      if (connection.state === HubConnectionState.Connected || connection.state === HubConnectionState.Connecting) {
        connection.stop()
          .then(() => console.log('🔌 Global SignalR connection stopped.'))
          .catch(err => console.error('Error stopping global SignalR:', err));
      }
    };
  }, [isAuthenticated, user]);
}
