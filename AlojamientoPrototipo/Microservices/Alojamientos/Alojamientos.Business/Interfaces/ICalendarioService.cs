using Alojamientos.Business.DTOs;

namespace Alojamientos.Business.Interfaces;

public interface ICalendarioService
{
    Task<IEnumerable<CalendarioResponse>> GetDisponibilidadMensualAsync(int habitacionId, int mes, int anio);
    Task<IEnumerable<CalendarioResponse>> BloquearFechasAsync(BloquearFechasRequest request);
}
