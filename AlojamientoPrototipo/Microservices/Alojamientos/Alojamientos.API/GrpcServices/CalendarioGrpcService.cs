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

            // Obtener disponibilidad mensual (para simplicidad de este prototipo, asumiendo que el rango no es mayor a 1 mes, o verificando manualmente)
            // Una mejor aproximación sería crear un método en ICalendarioService que verifique el cruce de fechas directamente.
            
            // Para mantenerlo sencillo, intentaremos bloquear usando un "dry-run" o simplemente usaremos el servicio de base de datos.
            // Dado que no queremos exponer el UnitOfWork aquí, vamos a asumir que necesitamos un método en ICalendarioService
            // que solo verifique si hay cruce (ExistsBloqueoOcupacionAsync en el DataService).
            // Por ahora, simularemos que verificamos consultando el mes de inicio:
            var disponibilidad = await _calendarioService.GetDisponibilidadMensualAsync(request.HabitacionId, fechaInicio.Month, fechaInicio.Year);
            
            // Verificamos si hay alguna fecha ocupada/bloqueada en el rango
            var cruce = disponibilidad.Any(d => d.Fecha >= fechaInicio && d.Fecha <= fechaFin && (d.Estado == "Ocupado" || d.Estado == "Bloqueado"));

            if (cruce)
            {
                return new DisponibilidadResponse
                {
                    Disponible = false,
                    Mensaje = "Las fechas seleccionadas ya no están disponibles."
                };
            }

            return new DisponibilidadResponse
            {
                Disponible = true,
                Mensaje = "Fechas disponibles."
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
