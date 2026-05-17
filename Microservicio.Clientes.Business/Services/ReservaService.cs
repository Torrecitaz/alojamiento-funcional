using Microservicio.Cliente.DatAccess.Entities.Reservas;
using Microservicio.Clientes.Business.DTOs.Reservas;
using Microservicio.Clientes.Business.Interfaces;
using Microservicio.Clientes.DataManagement.Interfaces;

namespace Microservicio.Clientes.Business.Services;

/// <summary>
/// Servicio de Reservas — Aquí ocurre la ORQUESTACIÓN CROSS-DB.
/// Coordina operaciones entre DB_Alojamientos y DB_Reservas.
/// </summary>
public class ReservaService : IReservaService
{
    private readonly IReservaRepository _reservaRepo;
    private readonly ICalendarioRepository _calendarioRepo;
    private readonly IHabitacionRepository _habitacionRepo;

    public ReservaService(
        IReservaRepository reservaRepo,
        ICalendarioRepository calendarioRepo,
        IHabitacionRepository habitacionRepo)
    {
        _reservaRepo = reservaRepo;
        _calendarioRepo = calendarioRepo;
        _habitacionRepo = habitacionRepo;
    }

    public async Task<IEnumerable<ReservaResponse>> GetAllAsync()
    {
        var items = await _reservaRepo.GetAllAsync();
        return items.Select(MapReserva);
    }

    public async Task<ReservaResponse?> GetByIdAsync(int id)
    {
        var r = await _reservaRepo.GetWithDetallesAsync(id);
        return r == null ? null : MapReserva(r);
    }

    public async Task<ReservaResponse?> GetByCodigoAsync(string codigo)
    {
        var r = await _reservaRepo.GetByCodigoAsync(codigo);
        return r == null ? null : MapReserva(r);
    }

    public async Task<IEnumerable<ReservaResponse>> GetByClienteIdAsync(int clienteId)
    {
        var items = await _reservaRepo.GetByClienteIdAsync(clienteId);
        return items.Select(MapReserva);
    }

    /// <summary>
    /// Flujo completo de creación de reserva con orquestación cross-DB:
    /// 1. Verifica disponibilidad en DB_Alojamientos (CalendarioDisponibilidad)
    /// 2. Calcula noches con fn_calcular_noches en DB_Reservas
    /// 3. Crea la reserva con un código temporal
    /// 4. Asigna código definitivo con sp_asignar_codigo_reserva
    /// 5. Bloquea las fechas en DB_Alojamientos
    /// </summary>
    public async Task<ReservaResponse> CrearReservaAsync(CrearReservaRequest request)
    {
        // PASO 1: Verificar disponibilidad en DB_Alojamientos
        foreach (var hab in request.Habitaciones)
        {
            var disponible = await _calendarioRepo.IsDisponibleAsync(
                hab.HabitacionId, request.FechaCheckIn, request.FechaCheckOut);

            if (!disponible)
                throw new InvalidOperationException(
                    $"La habitación {hab.HabitacionId} no está disponible para las fechas seleccionadas.");
        }

        // PASO 2: Calcular noches con la función de DB_Reservas
        var noches = await _reservaRepo.CalcularNochesFnAsync(request.FechaCheckIn, request.FechaCheckOut);

        // PASO 3: Calcular totales y crear entidad
        var detalles = request.Habitaciones.Select(h => new ReservaDetalleHabitacionEntity
        {
            HabitacionId = h.HabitacionId,
            PrecioPorNoche = h.PrecioPorNoche,
            NumNoches = noches,
            SubTotalHabitacion = h.PrecioPorNoche * noches
        }).ToList();

        var subTotal = detalles.Sum(d => d.SubTotalHabitacion);

        var reserva = new ReservaEntity
        {
            ClienteId = request.ClienteId,
            AlojamientoId = request.AlojamientoId,
            FechaCheckIn = request.FechaCheckIn,
            FechaCheckOut = request.FechaCheckOut,
            NumAdultos = request.NumAdultos,
            NumNinos = request.NumNinos,
            LlevaMascotas = request.LlevaMascotas,
            NumHabitaciones = request.Habitaciones.Count,
            SubTotal = subTotal,
            Total = subTotal, // Sin descuento por ahora
            CodigoReserva = $"TEMP-{Guid.NewGuid().ToString()[..8]}", // Temporal
            Detalles = detalles
        };

        var created = await _reservaRepo.AddAsync(reserva);

        // PASO 4: Asignar código definitivo con SP de DB_Reservas
        await _reservaRepo.AsignarCodigoReservaSPAsync(created.ReservaId);

        // PASO 5: Bloquear fechas en DB_Alojamientos (CalendarioDisponibilidad)
        foreach (var hab in request.Habitaciones)
        {
            await _calendarioRepo.BloquearFechasAsync(
                hab.HabitacionId, request.FechaCheckIn, request.FechaCheckOut);
        }

        // Recargar la reserva con el código actualizado
        var final = await _reservaRepo.GetWithDetallesAsync(created.ReservaId);
        return MapReserva(final!);
    }

    private static ReservaResponse MapReserva(ReservaEntity r) => new(
        r.ReservaId, r.ClienteId, r.AlojamientoId,
        r.FechaCheckIn, r.FechaCheckOut,
        r.NumAdultos, r.NumNinos, r.LlevaMascotas, r.NumHabitaciones,
        r.SubTotal, r.Total, r.Estado, r.CodigoReserva, r.FechaCreacion,
        r.Detalles?.Select(d => new DetalleHabitacionResponse(
            d.DetalleId, d.HabitacionId, d.PrecioPorNoche, d.NumNoches, d.SubTotalHabitacion
        )).ToList(),
        r.Descuento?.Codigo
    );
}
