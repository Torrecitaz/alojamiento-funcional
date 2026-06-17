import { useEffect, useState } from 'react';
import { HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';
import toast from 'react-hot-toast';
import useAuthStore from '../store/useAuthStore';

let globalConnection = null;
const connectionStateListeners = new Set();
let isConnectedState = false;

const updateState = (connected) => {
  isConnectedState = connected;
  connectionStateListeners.forEach(listener => listener(connected));
};

export default function useSignalR() {
  const { isAuthenticated, user } = useAuthStore();
  const [isConnected, setIsConnected] = useState(isConnectedState);

  useEffect(() => {
    const listener = (state) => setIsConnected(state);
    connectionStateListeners.add(listener);
    return () => {
      connectionStateListeners.delete(listener);
    };
  }, []);

  useEffect(() => {
    if (!isAuthenticated) {
      if (globalConnection) {
        const conn = globalConnection;
        globalConnection = null;
        updateState(false);
        conn.stop()
          .then(() => console.log('🔌 Global SignalR connection stopped.'))
          .catch(err => console.error('Error stopping global SignalR:', err));
      }
      return;
    }

    if (!globalConnection) {
      const backendUrl = import.meta.env.VITE_API_BASE_URL && import.meta.env.VITE_API_BASE_URL.startsWith('http')
        ? import.meta.env.VITE_API_BASE_URL.replace('/api/v1', '')
        : window.location.origin;
      const connectionUrl = `${backendUrl}/bookingHub`;

      const conn = new HubConnectionBuilder()
        .withUrl(connectionUrl)
        .configureLogging(LogLevel.Warning)
        .withAutomaticReconnect([0, 2000, 10000, 30000])
        .build();

      globalConnection = conn;

      // Event listeners for real-time updates
      conn.on('OnReservaCreated', (reserva) => {
        console.log('⚡ SignalR: OnReservaCreated', reserva);
        const isMyReserva = user && (user.clienteId === reserva.clienteId || user.id === reserva.clienteId.toString());
        const isColabOrAdmin = user && (user.roles?.includes('Administrador') || user.roles?.includes('Colaborador'));
        
        if (isMyReserva) {
          toast.success(`¡Tu reserva #${reserva.codigoReserva} ha sido pre-registrada! Paga para confirmar.`, { icon: '📩' });
        } else if (isColabOrAdmin) {
          toast.success(`Nueva reserva pre-registrada: #${reserva.codigoReserva} (Total: $${reserva.total.toFixed(2)})`, { icon: '🛎️' });
        }
      });

      conn.on('OnReservaConfirmed', (reserva) => {
        console.log('⚡ SignalR: OnReservaConfirmed', reserva);
        toast.success(`¡Reserva #${reserva.codigoReserva} CONFIRMADA exitosamente!`, {
          icon: '✅',
          style: { border: '2px solid #10B981', background: '#064E3B', color: '#fff' }
        });
      });

      conn.on('OnReservaCancelled', (reserva) => {
        console.log('⚡ SignalR: OnReservaCancelled', reserva);
        toast.error(`Reserva #${reserva.codigoReserva} ha sido CANCELADA.`, {
          icon: '❌',
          style: { border: '2px solid #EF4444', background: '#7F1D1D', color: '#fff' }
        });
      });

      conn.on('OnAlojamientoEstadoChanged', (change) => {
        console.log('⚡ SignalR: OnAlojamientoEstadoChanged', change);
        const isColabOrAdmin = user && (user.roles?.includes('Administrador') || user.roles?.includes('Colaborador'));
        if (isColabOrAdmin) {
          toast(`Alojamiento #${change.alojamientoId} cambió su estado a: ${change.estado}`, { icon: '🏨' });
        }
      });

      conn.onreconnecting((error) => {
        console.warn('🔌 SignalR: Connection lost. Reconnecting...', error);
        updateState(false);
      });

      conn.onreconnected((connectionId) => {
        console.log('🔌 SignalR: Connection re-established. Connection ID:', connectionId);
        updateState(true);
      });

      conn.onclose((error) => {
        console.error('🔌 SignalR: Connection closed.', error);
        updateState(false);
      });

      // Start connection
      conn.start()
        .then(() => {
          console.log('🔌 Global SignalR connection active.');
          updateState(true);
          
          if (user) {
            if (user.clienteId) {
              conn.invoke("JoinGroup", `cliente_${user.clienteId}`)
                .catch(err => console.error("Error joining client group:", err));
            }
            if (user.colaboradorId) {
              conn.invoke("JoinGroup", `colaborador_${user.colaboradorId}`)
                .catch(err => console.error("Error joining colaborador group:", err));
            }
            if (user.roles) {
              user.roles.forEach(role => {
                conn.invoke("JoinGroup", `role_${role}`)
                  .catch(err => console.error(`Error joining role group role_${role}:`, err));
              });
            }
          }
        })
        .catch((err) => {
          console.error('❌ Global SignalR connection failed:', err);
          updateState(false);
        });
    }
  }, [isAuthenticated, user]);

  return { connection: globalConnection, isConnected };
}
