using Alojamientos.Business.DTOs.Alojamientos;
using Alojamientos.Business.Exceptions;
using Alojamientos.Business.Interfaces;
using Alojamientos.Business.Mappers;
using Alojamientos.DataManagement.Interfaces;
using Alojamientos.DataManagement.Models;
using MassTransit;

namespace Alojamientos.Business.Services;

public class AlojamientosService : IAlojamientosService
{
    private readonly IAlojamientosDataService _dataService;
    private readonly IPublishEndpoint _publishEndpoint;

    public AlojamientosService(IAlojamientosDataService dataService, IPublishEndpoint publishEndpoint)
    {
        _dataService = dataService;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<IEnumerable<AlojamientoResponse>> GetAllAsync()
    {
        var models = await _dataService.GetAllAsync();
        return models.Select(AlojamientosBusinessMapper.ToResponse);
    }

    public async Task<AlojamientoResponse?> GetByIdAsync(int id)
    {
        var model = await _dataService.GetByIdAsync(id);
        return model != null ? AlojamientosBusinessMapper.ToResponse(model) : null;
    }

    public async Task<AlojamientoResponse> CrearAsync(CrearAlojamientoRequest request)
    {
        var model = new AlojamientoDataModel
        {
            SocioId = request.SocioId,
            TipoAlojamientoId = request.TipoAlojamientoId,
            Nombre = request.Nombre,
            Ciudad = request.Ciudad,
            Direccion = request.Direccion,
            Descripcion = request.Descripcion,
            AdmiteMascotas = request.AdmiteMascotas,
            TienePiscina = request.TienePiscina,
            TieneParqueadero = request.TieneParqueadero,
            Estado = "Activo",
            Provincia = request.Provincia,
            Pais = request.Pais,
            Politicas = request.Politicas,
            CheckInTime = request.CheckInTime,
            CheckOutTime = request.CheckOutTime,
            Servicios = request.Servicios,
            Latitud = request.Latitud,
            Longitud = request.Longitud
        };

        var created = await _dataService.CreateAsync(model);

        // Publicar evento de creación
        await _publishEndpoint.Publish(new Shared.Kernel.Events.AlojamientoEstadoChangedEvent
        {
            AlojamientoId = created.AlojamientoId,
            Estado = created.Estado
        });

        return AlojamientosBusinessMapper.ToResponse(created);
    }

    public async Task ActualizarAsync(int id, ActualizarAlojamientoRequest request)
    {
        var existing = await _dataService.GetByIdAsync(id)
            ?? throw new AlojamientoNotFoundException(id);

        existing.Nombre = request.Nombre;
        existing.Ciudad = request.Ciudad;
        existing.Direccion = request.Direccion;
        existing.Descripcion = request.Descripcion;
        existing.TipoAlojamientoId = request.TipoAlojamientoId;
        existing.AdmiteMascotas = request.AdmiteMascotas;
        existing.TienePiscina = request.TienePiscina;
        existing.TieneParqueadero = request.TieneParqueadero;
        if (request.Estrellas.HasValue) existing.Estrellas = request.Estrellas.Value;

        existing.Provincia = request.Provincia;
        existing.Pais = request.Pais;
        existing.Politicas = request.Politicas;
        existing.CheckInTime = request.CheckInTime;
        existing.CheckOutTime = request.CheckOutTime;
        existing.Servicios = request.Servicios;
        existing.Latitud = request.Latitud;
        existing.Longitud = request.Longitud;
        if (!string.IsNullOrEmpty(request.Estado)) existing.Estado = request.Estado;

        await _dataService.UpdateAsync(existing);

        // Publicar evento de actualización
        await _publishEndpoint.Publish(new Shared.Kernel.Events.AlojamientoEstadoChangedEvent
        {
            AlojamientoId = existing.AlojamientoId,
            Estado = existing.Estado
        });
    }

    public async Task EliminarAsync(int id)
    {
        var existing = await _dataService.GetByIdAsync(id)
            ?? throw new AlojamientoNotFoundException(id);

        existing.Estado = "Inactivo";
        await _dataService.UpdateAsync(existing);

        // Publicar evento de desactivación
        await _publishEndpoint.Publish(new Shared.Kernel.Events.AlojamientoEstadoChangedEvent
        {
            AlojamientoId = existing.AlojamientoId,
            Estado = existing.Estado
        });
    }
}
