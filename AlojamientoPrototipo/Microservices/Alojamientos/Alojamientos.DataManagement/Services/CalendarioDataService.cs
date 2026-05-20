using System;
using Alojamientos.DataAccess.Entities;
using Alojamientos.DataAccess.Repositories.Interfaces;
using Alojamientos.DataManagement.Interfaces;
using Alojamientos.DataManagement.Mappers;
using Alojamientos.DataManagement.Models;

namespace Alojamientos.DataManagement.Services;

public class CalendarioDataService : ICalendarioDataService
{
    private readonly ICalendarioDisponibilidadRepository _repository;

    public CalendarioDataService(ICalendarioDisponibilidadRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<CalendarioDisponibilidadDataModel>> GetByHabitacionIdAsync(int habitacionId, int mes, int anio)
    {
        var entities = await _repository.FindAsync(c => 
            c.HabitacionId == habitacionId && 
            c.Fecha.Month == mes && 
            c.Fecha.Year == anio);
            
        return entities.Select(AlojamientosMapper.ToDataModel);
    }

    public async Task<IEnumerable<CalendarioDisponibilidadDataModel>> CreateRangeAsync(IEnumerable<CalendarioDisponibilidadDataModel> models)
    {
        var entities = models.Select(m => new CalendarioDisponibilidadEntity
        {
            HabitacionId = m.HabitacionId,
            Fecha = m.Fecha,
            Estado = m.Estado,
            FechaModificacion = DateTime.UtcNow
        }).ToList();

        await _repository.AddRangeAsync(entities);
        return entities.Select(AlojamientosMapper.ToDataModel);
    }

    public async Task<bool> ExistsBloqueoOcupacionAsync(int habitacionId, DateOnly fechaInicio, DateOnly fechaFin)
    {
        var entities = await _repository.FindAsync(c => 
            c.HabitacionId == habitacionId &&
            c.Fecha >= fechaInicio && 
            c.Fecha <= fechaFin &&
            (c.Estado == "Ocupado" || c.Estado == "Bloqueado"));
            
        return entities.Any();
    }
}
