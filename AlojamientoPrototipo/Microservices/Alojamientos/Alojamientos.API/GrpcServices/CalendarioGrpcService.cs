using Grpc.Core;
using Shared.Protos;
using Alojamientos.Business.Interfaces;

namespace Alojamientos.API.GrpcServices;

public class CalendarioGrpcService : CalendarioGrpc.CalendarioGrpcBase
{
    private readonly ICalendarioService _calendarioService;
    private readonly ILogger<CalendarioGrpcService> _logger;

    public CalendarioGrpcService(ICalendarioService calendarioService, ILogger<CalendarioGrpcService> logger)
    {
        _calendarioService = calendarioService;
        _logger = logger;
    }

    public override async Task<DisponibilidadResponse> VerificarDisponibilidad(DisponibilidadRequest request, ServerCallContext context)
    {
        try
        {
            var fechaInicio = DateOnly.Parse(request.FechaInicio);
            var fechaFin = DateOnly.Parse(request.FechaFin);
            var fechaFinExclusiva = fechaFin.AddDays(-1);

            // Intentar bloquear transaccionalmente las fechas en el calendario como 'Ocupado'
            try
            {
                await _calendarioService.BloquearFechasAsync(new Alojamientos.Business.DTOs.BloquearFechasRequest
                {
                    HabitacionId = request.HabitacionId,
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFinExclusiva,
                    Estado = "Ocupado",
                    ReservaId = request.ReservaId,
                    Origen = request.Origen
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Disponibilidad no confirmada para Habitación {HabitacionId} en [{Inicio} - {Fin}]: {Msg}", 
                    request.HabitacionId, request.FechaInicio, request.FechaFin, ex.Message);
                
                return new DisponibilidadResponse
                {
                    Disponible = false,
                    Mensaje = $"Las fechas seleccionadas ya no están disponibles: {ex.Message}"
                };
            }

            return new DisponibilidadResponse
            {
                Disponible = true,
                Mensaje = "Fechas disponibles y reservadas."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar disponibilidad por gRPC");
            return new DisponibilidadResponse
            {
                Disponible = false,
                Mensaje = "Error interno al verificar la disponibilidad."
            };
        }
    }
}
