using Reservas.DataAccess.Entities;
using Reservas.DataAccess.Repositories.Interfaces;
using Reservas.DataManagement.Interfaces;
using Reservas.DataManagement.Mappers;
using Reservas.DataManagement.Models;

namespace Reservas.DataManagement.Services;

public class ReservasDataService : IReservasDataService
{
    private readonly IReservasRepository _repo;

    public ReservasDataService(IReservasRepository repo) => _repo = repo;

    public async Task<ReservaDataModel?> GetByIdAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        return entity != null ? ReservasMapper.ToDataModel(entity) : null;
    }

    public async Task<IEnumerable<ReservaDataModel>> GetByClienteIdAsync(int clienteId)
    {
        var entities = await _repo.FindAsync(r => r.ClienteId == clienteId);
        return entities.OrderByDescending(r => r.FechaCreacion).Select(ReservasMapper.ToDataModel);
    }

    public async Task<ReservaDataModel> CreateAsync(ReservaDataModel model)
    {
        var entity = new ReservaEntity
        {
            DescuentoId = model.DescuentoId,
            ClienteId = model.ClienteId,
            AlojamientoId = model.AlojamientoId,
            FechaCheckIn = model.FechaCheckIn,
            FechaCheckOut = model.FechaCheckOut,
            NumAdultos = model.NumAdultos,
            NumNinos = model.NumNinos,
            LlevaMascotas = model.LlevaMascotas,
            NumHabitaciones = model.NumHabitaciones,
            SubTotal = model.SubTotal,
            Total = model.Total,
            Estado = model.Estado,
            CodigoReserva = model.CodigoReserva
        };

        foreach(var det in model.DetallesHabitacion)
        {
            entity.DetallesHabitacion.Add(new ReservaDetalleHabitacionEntity
            {
                HabitacionId = det.HabitacionId,
                PrecioPorNoche = det.PrecioPorNoche,
                NumNoches = det.NumNoches,
                SubTotalHabitacion = det.SubTotalHabitacion
            });
        }

        var created = await _repo.AddAsync(entity);
        return ReservasMapper.ToDataModel(created);
    }

    public async Task UpdateStatusAsync(int id, string nuevoEstado)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null) throw new KeyNotFoundException($"Reserva {id} no encontrada");

        entity.Estado = nuevoEstado;
        entity.FechaModificacion = DateTime.UtcNow;
        await _repo.UpdateAsync(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null) throw new KeyNotFoundException($"Reserva {id} no encontrada");
        await _repo.DeleteAsync(entity);
    }
}
