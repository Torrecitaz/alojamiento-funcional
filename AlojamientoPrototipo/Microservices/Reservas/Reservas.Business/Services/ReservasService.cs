using Reservas.Business.DTOs;
using Reservas.Business.Exceptions;
using Reservas.Business.Interfaces;
using Reservas.Business.Mappers;
using Reservas.DataManagement.Interfaces;
using Reservas.DataManagement.Models;
using MassTransit;

namespace Reservas.Business.Services;

public class ReservasService : IReservasService
{
    private readonly IReservasDataService _reservasDataService;
    private readonly IDescuentosDataService _descuentosDataService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly Shared.Protos.CalendarioGrpc.CalendarioGrpcClient _calendarioGrpcClient;
    private readonly IPublishEndpoint _publishEndpoint;

    public ReservasService(
        IReservasDataService reservasDataService,
        IDescuentosDataService descuentosDataService,
        IUnitOfWork unitOfWork,
        Shared.Protos.CalendarioGrpc.CalendarioGrpcClient calendarioGrpcClient,
        IPublishEndpoint publishEndpoint)
    {
        _reservasDataService = reservasDataService;
        _descuentosDataService = descuentosDataService;
        _unitOfWork = unitOfWork;
        _calendarioGrpcClient = calendarioGrpcClient;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<ReservaResponse> GetByIdAsync(int id)
    {
        var reserva = await _reservasDataService.GetByIdAsync(id);
        if (reserva == null) throw new ReservaNotFoundException(id);
        return ReservasBusinessMapper.ToResponse(reserva);
    }

    public async Task<IEnumerable<ReservaResponse>> GetByClienteIdAsync(int clienteId)
    {
        var reservas = await _reservasDataService.GetByClienteIdAsync(clienteId);
        return reservas.Select(ReservasBusinessMapper.ToResponse);
    }

    public async Task<IEnumerable<ReservaResumenResponse>> GetResumenByClienteIdAsync(int clienteId)
    {
        var reservas = await _reservasDataService.GetByClienteIdAsync(clienteId);
        return reservas.Select(ReservasBusinessMapper.ToResumenResponse);
    }

    public async Task<ReservaResponse> CrearAsync(CrearReservaRequest request)
    {
        // 1. Validación de fechas
        if (request.FechaCheckOut <= request.FechaCheckIn)
            throw new FechasInvalidasException("La fecha de CheckOut debe ser posterior al CheckIn.");

        // 2. Validación de Descuento
        DescuentoDataModel? descuento = null;
        if (!string.IsNullOrEmpty(request.CodigoDescuento))
        {
            descuento = await _descuentosDataService.GetByCodigoAsync(request.CodigoDescuento);
            if (descuento == null || !descuento.Activo)
                throw new DescuentoInvalidoException(request.CodigoDescuento);
        }

        // 3. Generación del código de reserva antes de la verificación para asociarlo al bloqueo del calendario
        string codigoReserva = $"RES-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}";
        string grpcReservaId = request.ExternalId?.ToString() ?? codigoReserva;
        string grpcOrigen = request.ExternalId.HasValue ? "BOOKING" : "ALOJAEXPRESS";

        // 4. Verificación de Disponibilidad vía gRPC (Sincrónico y Rápido)
        foreach (var habReq in request.Habitaciones)
        {
            var disponibilidad = await _calendarioGrpcClient.VerificarDisponibilidadAsync(new Shared.Protos.DisponibilidadRequest
            {
                HabitacionId = habReq.HabitacionId,
                FechaInicio = request.FechaCheckIn.ToString("yyyy-MM-dd"),
                FechaFin = request.FechaCheckOut.ToString("yyyy-MM-dd"),
                ReservaId = grpcReservaId,
                Origen = grpcOrigen
            });

            if (!disponibilidad.Disponible)
            {
                throw new BusinessRuleException($"Habitación {habReq.HabitacionId} no disponible: {disponibilidad.Mensaje}");
            }
        }

        // 5. Generación de detalles y subtotal
        var detalles = new List<ReservaDetalleHabitacionDataModel>();
        decimal subTotal = 0;

        foreach (var req in request.Habitaciones)
        {
            var subTotalHab = req.PrecioPorNoche * req.NumNoches;
            detalles.Add(new ReservaDetalleHabitacionDataModel
            {
                HabitacionId = req.HabitacionId,
                PrecioPorNoche = req.PrecioPorNoche,
                NumNoches = req.NumNoches,
                SubTotalHabitacion = subTotalHab
            });
            subTotal += subTotalHab;
        }

        // 6. Cálculo del total con descuento
        decimal total = subTotal;
        if (descuento != null)
        {
            var montoDescuento = subTotal * (descuento.Porcentaje / 100m);
            total -= montoDescuento;
        }

        // 7. Preparar modelo de datos
        var model = new ReservaDataModel
        {
            ClienteId = request.ClienteId,
            AlojamientoId = request.AlojamientoId,
            FechaCheckIn = request.FechaCheckIn,
            FechaCheckOut = request.FechaCheckOut,
            NumAdultos = request.NumAdultos,
            NumNinos = request.NumNinos,
            LlevaMascotas = request.LlevaMascotas,
            NumHabitaciones = request.Habitaciones.Count,
            DescuentoId = descuento?.DescuentoId,
            SubTotal = subTotal,
            Total = total,
            Estado = "Pendiente",
            CodigoReserva = codigoReserva,
            ExternalId = request.ExternalId,
            DetallesHabitacion = detalles
        };

        // 6. Transacción
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var created = await _reservasDataService.CreateAsync(model);
            await _unitOfWork.CommitTransactionAsync();
            
            // Publicar evento ReservaCreatedEvent
            await _publishEndpoint.Publish(new Shared.Kernel.Events.ReservaCreatedEvent
            {
                ReservaId = created.ReservaId,
                AlojamientoId = created.AlojamientoId,
                ClienteId = created.ClienteId,
                FechaCheckIn = created.FechaCheckIn,
                FechaCheckOut = created.FechaCheckOut,
                Total = created.Total,
                CodigoReserva = created.CodigoReserva
            });

            return await GetByIdAsync(created.ReservaId);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task ActualizarEstadoAsync(int id, ActualizarEstadoReservaRequest request)
    {
        var existing = await _reservasDataService.GetByIdAsync(id);
        if (existing == null) throw new ReservaNotFoundException(id);

        await _reservasDataService.UpdateStatusAsync(id, request.Estado);
        
        if (request.Estado.Equals("Confirmada", StringComparison.OrdinalIgnoreCase))
        {
            await _publishEndpoint.Publish(new Shared.Kernel.Events.ReservaConfirmedEvent
            {
                ReservaId = existing.ReservaId,
                CodigoReserva = existing.CodigoReserva,
                AlojamientoId = existing.AlojamientoId,
                FechaCheckIn = existing.FechaCheckIn,
                FechaCheckOut = existing.FechaCheckOut
            });
        }
        else if (request.Estado.Equals("Cancelada", StringComparison.OrdinalIgnoreCase))
        {
            await _publishEndpoint.Publish(new Shared.Kernel.Events.ReservaCancelledEvent
            {
                ReservaId = existing.ReservaId,
                CodigoReserva = existing.CodigoReserva,
                AlojamientoId = existing.AlojamientoId
            });
        }
    }

    public async Task<ReservaResponse?> GetByCodigoAsync(string codigo)
    {
        var reserva = await _reservasDataService.GetByCodigoAsync(codigo);
        return reserva != null ? ReservasBusinessMapper.ToResponse(reserva) : null;
    }

    public async Task<ReservaResponse?> GetByExternalIdAsync(Guid externalId)
    {
        var reserva = await _reservasDataService.GetByExternalIdAsync(externalId);
        return reserva != null ? ReservasBusinessMapper.ToResponse(reserva) : null;
    }
}
