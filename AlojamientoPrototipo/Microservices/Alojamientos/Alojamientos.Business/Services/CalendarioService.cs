using Alojamientos.Business.DTOs;
using Alojamientos.Business.Exceptions;
using Alojamientos.Business.Interfaces;
using Alojamientos.DataManagement.Interfaces;
using Alojamientos.DataManagement.Models;

namespace Alojamientos.Business.Services;

public class CalendarioService : ICalendarioService
{
    private readonly ICalendarioDataService _calendarioDataService;
    private readonly IHabitacionesDataService _habitacionesDataService;
    private readonly IUnitOfWork _unitOfWork;

    public CalendarioService(
        ICalendarioDataService calendarioDataService,
        IHabitacionesDataService habitacionesDataService,
        IUnitOfWork unitOfWork)
    {
        _calendarioDataService = calendarioDataService;
        _habitacionesDataService = habitacionesDataService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<CalendarioResponse>> GetDisponibilidadMensualAsync(int habitacionId, int mes, int anio)
    {
        if (mes < 1 || mes > 12) throw new BusinessRuleException("El mes debe estar entre 1 y 12.");
        if (anio < 2020) throw new BusinessRuleException("Año inválido.");

        var data = await _calendarioDataService.GetByHabitacionIdAsync(habitacionId, mes, anio);
        return data.Select(c => new CalendarioResponse
        {
            CalendarioId = c.CalendarioId,
            HabitacionId = c.HabitacionId,
            Fecha = c.Fecha,
            Estado = c.Estado
        });
    }

    public async Task<IEnumerable<CalendarioResponse>> BloquearFechasAsync(BloquearFechasRequest request)
    {
        if (request.FechaFin < request.FechaInicio)
            throw new BusinessRuleException("La fecha de fin debe ser mayor o igual a la fecha de inicio.");

        // Validar que la habitación exista
        var habitacion = await _habitacionesDataService.GetByIdAsync(request.HabitacionId);
        if (habitacion == null) throw new NotFoundException($"Habitación {request.HabitacionId} no encontrada.");

        // Validar que no haya cruces con reservas existentes (ocupado o bloqueado)
        bool hayCruce = await _calendarioDataService.ExistsBloqueoOcupacionAsync(
            request.HabitacionId, request.FechaInicio, request.FechaFin);

        if (hayCruce)
            throw new BusinessRuleException("Ya existen fechas bloqueadas u ocupadas en el rango seleccionado.");

        // Generar lista de días a bloquear
        var diasBloquear = new List<CalendarioDisponibilidadDataModel>();
        for (var fecha = request.FechaInicio; fecha <= request.FechaFin; fecha = fecha.AddDays(1))
        {
            diasBloquear.Add(new CalendarioDisponibilidadDataModel
            {
                HabitacionId = request.HabitacionId,
                Fecha = fecha,
                Estado = "Bloqueado"
            });
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var result = await _calendarioDataService.CreateRangeAsync(diasBloquear);
            await _unitOfWork.CommitTransactionAsync();

            return result.Select(c => new CalendarioResponse
            {
                CalendarioId = c.CalendarioId,
                HabitacionId = c.HabitacionId,
                Fecha = c.Fecha,
                Estado = c.Estado
            });
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
