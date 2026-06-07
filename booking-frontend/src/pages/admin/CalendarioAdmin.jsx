import { useState, useEffect, useRef } from 'react';
import { 
  HiOutlineCalendar, 
  HiOutlineLockClosed, 
  HiOutlineKey, 
  HiOutlineOfficeBuilding, 
  HiChevronLeft, 
  HiChevronRight, 
  HiCheckCircle, 
  HiBan 
} from 'react-icons/hi';
import toast from 'react-hot-toast';
import api from '../../services/api';
import useAuthStore from '../../store/useAuthStore';
import { HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';
import './CalendarioAdmin.css';

export default function CalendarioAdmin() {
  const { user } = useAuthStore();
  const esAdmin = user?.roles?.includes('Administrador');

  const [propiedades, setPropiedades] = useState([]);
  const [propiedadSeleccionada, setPropiedadSeleccionada] = useState('');
  const [habitaciones, setHabitaciones] = useState([]);
  const [habitacionSeleccionada, setHabitacionSeleccionada] = useState('');
  
  // Date State
  const [mes, setMes] = useState(new Date().getMonth() + 1); // 1-indexed (1-12)
  const [anio, setAnio] = useState(new Date().getFullYear());
  
  // Availability Data
  const [disponibilidad, setDisponibilidad] = useState([]);
  const [loading, setLoading] = useState(false);
  const [submitting, setSubmitting] = useState(false);

  // Manual Block/Release form
  const [fechaInicio, setFechaInicio] = useState('');
  const [fechaFin, setFechaFin] = useState('');
  const [tipoOperacion, setTipoOperacion] = useState('bloquear'); // 'bloquear' | 'liberar'
  const [estadoBloqueo, setEstadoBloqueo] = useState('Bloqueado');

  // SignalR State
  const [signalrConectado, setSignalrConectado] = useState(false);
  const signalrConnectionRef = useRef(null);

  // 1. Cargar Propiedades
  useEffect(() => {
    const fetchProps = esAdmin 
      ? api.get('/propiedades/buscar?PageSize=1000')
      : user?.colaboradorId 
        ? api.get(`/propiedades/colaborador/${user.colaboradorId}`)
        : Promise.resolve({ data: { datos: [] } });

    fetchProps
      .then(res => {
        const payload = res.data.datos;
        const list = esAdmin ? (payload?.items || []) : (payload || []);
        setPropiedades(list);
        if (list.length > 0) {
          setPropiedadSeleccionada(list[0].propiedadId.toString());
        }
      })
      .catch(() => toast.error('Error al cargar la lista de propiedades.'));
  }, [esAdmin, user?.colaboradorId]);

  // 2. Cargar Habitaciones cuando cambia la propiedad
  useEffect(() => {
    if (!propiedadSeleccionada) {
      setHabitaciones([]);
      setHabitacionSeleccionada('');
      return;
    }

    const fetchHabitaciones = async () => {
      try {
        const { data } = await api.get(`/habitaciones/por-propiedad/${propiedadSeleccionada}`);
        const list = data.datos || [];
        setHabitaciones(list);
        if (list.length > 0) {
          setHabitacionSeleccionada(list[0].habitacionId.toString());
        } else {
          setHabitacionSeleccionada('');
        }
      } catch {
        toast.error('Error al cargar las habitaciones.');
        setHabitaciones([]);
        setHabitacionSeleccionada('');
      }
    };

    fetchHabitaciones();
  }, [propiedadSeleccionada]);

  // 3. Cargar disponibilidad de la habitación
  const cargarDisponibilidad = async () => {
    if (!habitacionSeleccionada) {
      setDisponibilidad([]);
      return;
    }
    setLoading(true);
    try {
      // Endpoint: GET /api/v1/calendario/habitacion/{habitacionId}?mes={mes}&anio={anio}
      const { data } = await api.get(`/calendario/habitacion/${habitacionSeleccionada}`, {
        params: { mes, anio }
      });
      setDisponibilidad(data || []);
    } catch {
      toast.error('Error al cargar la disponibilidad del calendario.');
      setDisponibilidad([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    cargarDisponibilidad();
  }, [habitacionSeleccionada, mes, anio]);

  // 4. Configurar SignalR en tiempo real
  useEffect(() => {
    if (!habitacionSeleccionada) return;

    // Crear conexión a /bookingHub
    const connectionUrl = `${window.location.origin}/bookingHub`;
    const connection = new HubConnectionBuilder()
      .withUrl(connectionUrl)
      .configureLogging(LogLevel.Warning)
      .withAutomaticReconnect()
      .build();

    signalrConnectionRef.current = connection;

    // Escuchar el evento de disponibilidad
    connection.on('OnAvailabilityChanged', (message) => {
      if (message && message.habitacionId.toString() === habitacionSeleccionada.toString()) {
        const eventDate = new Date(message.fecha);
        const eventMonth = eventDate.getMonth() + 1;
        const eventYear = eventDate.getFullYear();

        // Si es del mes/año que se está visualizando, refrescar
        if (eventMonth === mes && eventYear === anio) {
          console.log('⚡ SignalR: Disponibilidad de habitación cambiada en tiempo real. Recargando...', message);
          cargarDisponibilidad();
        }
      }
    });

    connection.on('OnReservaConfirmed', (message) => {
      console.log('⚡ SignalR: Reserva confirmada. Recargando...', message);
      cargarDisponibilidad();
    });

    connection.on('OnReservaCancelled', (message) => {
      console.log('⚡ SignalR: Reserva cancelada. Recargando...', message);
      cargarDisponibilidad();
    });

    // Iniciar conexión
    connection.start()
      .then(() => {
        setSignalrConectado(true);
        console.log('🔌 Conectado exitosamente a SignalR en api-gateway');
      })
      .catch((err) => {
        setSignalrConectado(false);
        console.error('❌ Error al conectar a SignalR:', err);
      });

    // Limpieza
    return () => {
      if (connection.state === HubConnectionState.Connected || connection.state === HubConnectionState.Connecting) {
        connection.stop()
          .then(() => console.log('🔌 Conexión de SignalR detenida.'))
          .catch(err => console.error('Error al desconectar SignalR:', err));
      }
    };
  }, [habitacionSeleccionada, mes, anio]);

  // Manejo de mes anterior/siguiente
  const mesAnterior = () => {
    if (mes === 1) {
      setMes(12);
      setAnio(prev => prev - 1);
    } else {
      setMes(prev => prev - 1);
    }
  };

  const mesSiguiente = () => {
    if (mes === 12) {
      setMes(1);
      setAnio(prev => prev + 1);
    } else {
      setMes(prev => prev + 1);
    }
  };

  // Enviar Bloqueo o Liberación de Fechas
  const handleOperacion = async (e) => {
    e.preventDefault();
    if (!habitacionSeleccionada) return toast.error('Selecciona una habitación primero.');
    if (!fechaInicio || !fechaFin) return toast.error('Ingresa las fechas de inicio y fin.');
    if (new Date(fechaFin) < new Date(fechaInicio)) return toast.error('La fecha fin debe ser mayor o igual a la fecha inicio.');

    setSubmitting(true);
    try {
      if (tipoOperacion === 'bloquear') {
        const payload = {
          habitacionId: parseInt(habitacionSeleccionada),
          fechaInicio,
          fechaFin,
          estado: estadoBloqueo
        };
        await api.post('/calendario/bloquear', payload);
        toast.success(`Fechas bloqueadas exitosamente como: ${estadoBloqueo}`);
      } else {
        const payload = {
          habitacionId: parseInt(habitacionSeleccionada),
          fechaInicio,
          fechaFin
        };
        await api.post('/calendario/liberar', payload);
        toast.success('Fechas liberadas exitosamente.');
      }
      cargarDisponibilidad();
      setFechaInicio('');
      setFechaFin('');
    } catch (err) {
      toast.error(err.response?.data?.mensaje || 'Error al procesar la operación en el calendario.');
    } finally {
      setSubmitting(false);
    }
  };

  // Formatear fecha para el input
  const formatInputDate = (dayNumber) => {
    const formattedMonth = mes.toString().padStart(2, '0');
    const formattedDay = dayNumber.toString().padStart(2, '0');
    return `${anio}-${formattedMonth}-${formattedDay}`;
  };

  // Al hacer clic en un día del calendario
  const handleDayClick = (dayNumber) => {
    const clickedDate = formatInputDate(dayNumber);
    if (!fechaInicio || (fechaInicio && fechaFin)) {
      setFechaInicio(clickedDate);
      setFechaFin('');
    } else {
      if (new Date(clickedDate) < new Date(fechaInicio)) {
        setFechaInicio(clickedDate);
      } else {
        setFechaFin(clickedDate);
      }
    }
  };

  // Generar grid de días
  const getNombreMes = (m) => {
    const meses = [
      'Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio',
      'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'
    ];
    return meses[m - 1];
  };

  const getDaysInMonth = (m, a) => {
    return new Date(a, m, 0).getDate();
  };

  const getStartDayOfWeek = (m, a) => {
    // 0 es Domingo, 1 es Lunes, etc.
    const day = new Date(a, m - 1, 1).getDay();
    // Ajustar para que el Lunes sea 0 y el Domingo sea 6
    return day === 0 ? 6 : day - 1;
  };

  const daysInMonth = getDaysInMonth(mes, anio);
  const startDayOfWeek = getStartDayOfWeek(mes, anio);

  // Crear la lista de celdas para el render
  const cells = [];
  // Celdas vacías del mes anterior
  for (let i = 0; i < startDayOfWeek; i++) {
    cells.push({ isEmpty: true, key: `empty-${i}` });
  }

  // Días del mes actual
  for (let d = 1; d <= daysInMonth; d++) {
    const dayDateStr = formatInputDate(d);
    // Buscar en la disponibilidad traída del backend
    const dbDay = disponibilidad.find(item => {
      const dbDateStr = item.fecha.substring(0, 10);
      return dbDateStr === dayDateStr;
    });

    const estado = dbDay ? dbDay.estado : 'Disponible';
    cells.push({
      isEmpty: false,
      dayNumber: d,
      dateString: dayDateStr,
      estado,
      key: `day-${d}`
    });
  }

  return (
    <div>
      <div className="admin-page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 12 }}>
        <div>
          <h1 className="admin-page-title">Calendario de Disponibilidad</h1>
          <p className="admin-page-subtitle">Visualiza el estado ocupacional y administra los bloqueos de fechas de tus habitaciones</p>
        </div>
        <div className={`signalr-indicator ${signalrConectado ? '' : 'offline'}`}>
          <span className="signalr-dot"></span>
          {signalrConectado ? 'Sincronizado en tiempo real' : 'Sin conexión a notificaciones'}
        </div>
      </div>

      {/* Selectores superiores */}
      <div className="card" style={{ padding: '20px', marginBottom: '24px', display: 'flex', gap: '20px', flexWrap: 'wrap', background: 'var(--color-bg)' }}>
        <div style={{ flex: '1 1 300px' }}>
          <label className="form-label" style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
            <HiOutlineOfficeBuilding size={16} /> Seleccionar Propiedad
          </label>
          <select 
            className="input-field" 
            value={propiedadSeleccionada} 
            onChange={e => setPropiedadSeleccionada(e.target.value)}
            style={{ fontWeight: 600 }}
          >
            <option value="">-- Seleccionar propiedad --</option>
            {propiedades.map(p => (
              <option key={p.propiedadId} value={p.propiedadId}>
                #{p.propiedadId} - {p.nombre} ({p.ciudad})
              </option>
            ))}
          </select>
        </div>

        <div style={{ flex: '1 1 300px' }}>
          <label className="form-label" style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
            <HiOutlineKey size={16} /> Seleccionar Habitación
          </label>
          <select 
            className="input-field" 
            value={habitacionSeleccionada} 
            onChange={e => setHabitacionSeleccionada(e.target.value)}
            style={{ fontWeight: 600 }}
            disabled={!propiedadSeleccionada}
          >
            <option value="">-- Seleccionar habitación --</option>
            {habitaciones.map(h => (
              <option key={h.habitacionId} value={h.habitacionId}>
                {h.nombre} - Capacidad: {h.capacidadAdultos} Ad., {h.capacidadNinos} Ni.
              </option>
            ))}
          </select>
        </div>
      </div>

      {habitacionSeleccionada ? (
        <div className="calendar-container">
          
          {/* Tarjeta del Calendario Grid */}
          <div className="calendar-card">
            
            {/* Cabecera del mes y botones de navegación */}
            <div className="calendar-header-actions">
              <button className="calendar-nav-btn" onClick={mesAnterior}>
                <HiChevronLeft size={20} /> Anterior
              </button>
              <div className="calendar-current-month">
                {getNombreMes(mes)} {anio}
              </div>
              <button className="calendar-nav-btn" onClick={mesSiguiente}>
                Siguiente <HiChevronRight size={20} />
              </button>
            </div>

            {loading ? (
              <div style={{ padding: '80px 0', textAlign: 'center', color: 'var(--color-text-muted)' }}>
                Cargando calendario...
              </div>
            ) : (
              <>
                <div className="calendar-grid">
                  {/* Cabecera de días de la semana */}
                  {['Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb', 'Dom'].map(d => (
                    <div key={d} className="calendar-day-header">{d}</div>
                  ))}

                  {/* Celdas del mes */}
                  {cells.map((cell) => {
                    if (cell.isEmpty) {
                      return <div key={cell.key} className="calendar-day-cell empty-cell"></div>;
                    }

                    // Determinar clase según estado
                    let statusClass = 'day-disponible';
                    let statusIcon = <HiCheckCircle size={16} />;
                    if (cell.estado === 'Ocupado' || cell.estado === 'Reservado') {
                      statusClass = 'day-ocupado';
                      statusIcon = <HiOutlineLockClosed size={16} />;
                    } else if (cell.estado === 'Bloqueado') {
                      statusClass = 'day-bloqueado';
                      statusIcon = <HiBan size={16} />;
                    }

                    // Check if selected in current range selection
                    const isSelectedStart = fechaInicio === cell.dateString;
                    const isSelectedEnd = fechaFin === cell.dateString;
                    const isWithinRange = fechaInicio && fechaFin && 
                      new Date(cell.dateString) >= new Date(fechaInicio) && 
                      new Date(cell.dateString) <= new Date(fechaFin);

                    const selectionStyle = (isSelectedStart || isSelectedEnd || isWithinRange)
                      ? { border: '2px solid var(--color-primary, #2b6cb0)', boxShadow: '0 0 8px rgba(43,108,176,0.4)' }
                      : {};

                    return (
                      <div 
                        key={cell.key} 
                        className={`calendar-day-cell ${statusClass}`}
                        style={selectionStyle}
                        onClick={() => handleDayClick(cell.dayNumber)}
                      >
                        <span className="calendar-day-number">{cell.dayNumber}</span>
                        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', width: '100%' }}>
                          <span className="day-status-label">{cell.estado}</span>
                          {statusIcon}
                        </div>
                      </div>
                    );
                  })}
                </div>

                {/* Leyenda */}
                <div className="calendar-legend">
                  <div className="legend-item">
                    <div className="legend-color legend-disponible"></div>
                    <span>Disponible</span>
                  </div>
                  <div className="legend-item">
                    <div className="legend-color legend-ocupado"></div>
                    <span>Ocupado / Reservado</span>
                  </div>
                  <div className="legend-item">
                    <div className="legend-color legend-bloqueado"></div>
                    <span>Bloqueado Manualmente</span>
                  </div>
                </div>
              </>
            )}
          </div>

          {/* Panel Lateral de Bloqueo / Liberación */}
          <div className="control-card">
            <h3 className="control-card-title">Acciones sobre Fechas</h3>
            
            <form onSubmit={handleOperacion}>
              <div className="control-form-group">
                <label className="control-form-label">Tipo de Operación</label>
                <div style={{ display: 'flex', gap: 10 }}>
                  <button 
                    type="button" 
                    className={`btn ${tipoOperacion === 'bloquear' ? 'btn-primary' : 'btn-outline'}`}
                    style={{ flex: 1, padding: '8px' }}
                    onClick={() => setTipoOperacion('bloquear')}
                  >
                    Bloquear
                  </button>
                  <button 
                    type="button" 
                    className={`btn ${tipoOperacion === 'liberar' ? 'btn-primary' : 'btn-outline'}`}
                    style={{ flex: 1, padding: '8px' }}
                    onClick={() => setTipoOperacion('liberar')}
                  >
                    Liberar
                  </button>
                </div>
              </div>

              <div className="control-form-group">
                <label className="control-form-label">Fecha de Inicio</label>
                <input 
                  type="date" 
                  className="input-field" 
                  value={fechaInicio} 
                  onChange={e => setFechaInicio(e.target.value)}
                  required 
                />
              </div>

              <div className="control-form-group">
                <label className="control-form-label">Fecha de Fin</label>
                <input 
                  type="date" 
                  className="input-field" 
                  value={fechaFin} 
                  onChange={e => setFechaFin(e.target.value)}
                  required 
                />
              </div>

              {tipoOperacion === 'bloquear' && (
                <div className="control-form-group">
                  <label className="control-form-label">Estado del Bloqueo</label>
                  <select 
                    className="input-field"
                    value={estadoBloqueo}
                    onChange={e => setEstadoBloqueo(e.target.value)}
                  >
                    <option value="Bloqueado">Bloqueo Administrativo</option>
                    <option value="Ocupado">Ocupado / Reservado Externo</option>
                  </select>
                </div>
              )}

              <p style={{ fontSize: '.78rem', color: 'var(--color-text-muted)', lineHeight: '1.4', margin: '12px 0' }}>
                Tip: Haz clic directamente en los días del calendario para seleccionar el rango de fechas rápidamente.
              </p>

              <button 
                type="submit" 
                className="btn btn-primary" 
                style={{ width: '100%', marginTop: '8px', padding: '10px 0' }}
                disabled={submitting}
              >
                {submitting ? 'Procesando...' : tipoOperacion === 'bloquear' ? 'Aplicar Bloqueo' : 'Liberar Fechas'}
              </button>
            </form>
          </div>

        </div>
      ) : (
        <div style={{ padding: 60, textAlign: 'center', color: 'var(--color-text-muted)', border: '2px dashed var(--color-border)', borderRadius: 12 }}>
          <HiOutlineCalendar size={48} style={{ opacity: 0.5, marginBottom: 16 }} />
          <h3>Sin Habitación Seleccionada</h3>
          <p>Selecciona un hotel y una habitación de las listas superiores para visualizar y gestionar la disponibilidad.</p>
        </div>
      )}
    </div>
  );
}
