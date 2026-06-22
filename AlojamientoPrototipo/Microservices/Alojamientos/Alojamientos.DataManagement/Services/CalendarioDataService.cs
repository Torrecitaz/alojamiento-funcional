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
        var resultList = new List<CalendarioDisponibilidadEntity>();
        var list = models.ToList();
        if (list.Count == 0) return Array.Empty<CalendarioDisponibilidadDataModel>();

        var habitacionId = list[0].HabitacionId;
        var fechas = list.Select(m => m.Fecha).ToList();
        var minFecha = fechas.Min();
        var maxFecha = fechas.Max();

        // Buscar las entidades de disponibilidad ya existentes en la BD para este rango de fechas
        var existingEntities = (await _repository.FindAsync(c => 
            c.HabitacionId == habitacionId && 
            c.Fecha >= minFecha && 
            c.Fecha <= maxFecha)).ToDictionary(c => c.Fecha);

        var entitiesToInsert = new List<CalendarioDisponibilidadEntity>();

        foreach (var m in list)
        {
            if (existingEntities.TryGetValue(m.Fecha, out var existing))
            {
                // Si ya existe la fecha (por ejemplo, sembrada como 'Disponible'), actualizamos su estado
                existing.Estado = m.Estado;
                existing.FechaModificacion = DateTime.UtcNow;
                existing.ReservaId = m.ReservaId;
                existing.Origen = string.IsNullOrEmpty(m.Origen) ? "ALOJAEXPRESS" : m.Origen;
                
                await _repository.UpdateAsync(existing);
                resultList.Add(existing);
            }
            else
            {
                // Si la fecha no existe en absoluto, creamos una nueva entidad
                var newEntity = new CalendarioDisponibilidadEntity
                {
                    HabitacionId = m.HabitacionId,
                    Fecha = m.Fecha,
                    Estado = m.Estado,
                    FechaModificacion = DateTime.UtcNow,
                    ReservaId = m.ReservaId,
                    Origen = string.IsNullOrEmpty(m.Origen) ? "ALOJAEXPRESS" : m.Origen
                };
                entitiesToInsert.Add(newEntity);
                resultList.Add(newEntity);
            }
        }

        if (entitiesToInsert.Count > 0)
        {
            await _repository.AddRangeAsync(entitiesToInsert);
        }

        return resultList.Select(AlojamientosMapper.ToDataModel);
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

    public async Task<bool> ExistsBloqueoOcupacionWithLockAsync(int habitacionId, DateOnly fechaInicio, DateOnly fechaFin)
    {
        return await _repository.ExistsBloqueoOcupacionWithLockAsync(habitacionId, fechaInicio, fechaFin);
    }

    public async Task EliminarFechasAsync(int habitacionId, DateOnly fechaInicio, DateOnly fechaFin)
    {
        var entities = await _repository.FindAsync(c =>
            c.HabitacionId == habitacionId &&
            c.Fecha >= fechaInicio &&
            c.Fecha <= fechaFin);
        
        foreach (var entity in entities)
        {
            await _repository.DeleteAsync(entity);
        }
    }
}
