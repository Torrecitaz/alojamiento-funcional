using Alojamientos.DataManagement.Models;

namespace Alojamientos.DataManagement.Interfaces;

public interface ICalendarioDataService
{
    Task<IEnumerable<CalendarioDisponibilidadDataModel>> GetByHabitacionIdAsync(int habitacionId, int mes, int anio);
    Task<IEnumerable<CalendarioDisponibilidadDataModel>> CreateRangeAsync(IEnumerable<CalendarioDisponibilidadDataModel> models);
    Task<bool> ExistsBloqueoOcupacionAsync(int habitacionId, DateOnly fechaInicio, DateOnly fechaFin);
}
