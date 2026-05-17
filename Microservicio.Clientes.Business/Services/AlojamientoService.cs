using Microservicio.Cliente.DatAccess.Entities.Alojamientos;
using Microservicio.Clientes.Business.DTOs.Alojamientos;
using Microservicio.Clientes.Business.Interfaces;
using Microservicio.Clientes.DataManagement.Interfaces;

namespace Microservicio.Clientes.Business.Services;

public class AlojamientoService : IAlojamientoService
{
    private readonly IAlojamientoRepository _alojamientoRepo;
    private readonly IHabitacionRepository _habitacionRepo;
    private readonly ICalendarioRepository _calendarioRepo;
    private readonly IRepository<TipoAlojamientoEntity> _tipoRepo;

    public AlojamientoService(
        IAlojamientoRepository alojamientoRepo,
        IHabitacionRepository habitacionRepo,
        ICalendarioRepository calendarioRepo,
        IRepository<TipoAlojamientoEntity> tipoRepo)
    {
        _alojamientoRepo = alojamientoRepo;
        _habitacionRepo = habitacionRepo;
        _calendarioRepo = calendarioRepo;
        _tipoRepo = tipoRepo;
    }

    public async Task<IEnumerable<AlojamientoResponse>> GetAllAsync()
    {
        var items = await _alojamientoRepo.GetAllAsync();
        return items.Select(MapAlojamiento);
    }

    public async Task<AlojamientoResponse?> GetByIdAsync(int id)
    {
        var a = await _alojamientoRepo.GetByIdAsync(id);
        return a == null ? null : MapAlojamiento(a);
    }

    public async Task<AlojamientoResponse?> GetWithHabitacionesAsync(int id)
    {
        var a = await _alojamientoRepo.GetWithHabitacionesAsync(id);
        return a == null ? null : MapAlojamiento(a);
    }

    public async Task<IEnumerable<AlojamientoResponse>> GetByCiudadAsync(string ciudad)
    {
        var items = await _alojamientoRepo.GetByCiudadAsync(ciudad);
        return items.Select(MapAlojamiento);
    }

    public async Task<AlojamientoResponse> CrearAsync(CrearAlojamientoRequest request)
    {
        var entity = new AlojamientoEntity
        {
            SocioId = request.SocioId,
            TipoAlojamientoId = request.TipoAlojamientoId,
            Ciudad = request.Ciudad,
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            Direccion = request.Direccion,
            Estrellas = request.Estrellas,
            AdmiteMascotas = request.AdmiteMascotas,
            TienePiscina = request.TienePiscina,
            TieneParqueadero = request.TieneParqueadero
        };

        var created = await _alojamientoRepo.AddAsync(entity);
        return MapAlojamiento(created);
    }

    public async Task<HabitacionResponse> CrearHabitacionAsync(CrearHabitacionRequest request)
    {
        var entity = new HabitacionEntity
        {
            AlojamientoId = request.AlojamientoId,
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            CapacidadAdultos = request.CapacidadAdultos,
            CapacidadNinos = request.CapacidadNinos,
            NumBanos = request.NumBanos,
            NumDormitorios = request.NumDormitorios,
            TieneCocina = request.TieneCocina,
            TieneAireAcondicionado = request.TieneAireAcondicionado,
            SuperficieM2 = request.SuperficieM2,
            PrecioNoche = request.PrecioNoche
        };

        var created = await _habitacionRepo.AddAsync(entity);
        return MapHabitacion(created);
    }

    public async Task<IEnumerable<HabitacionResponse>> GetHabitacionesByAlojamientoAsync(int alojamientoId)
    {
        var items = await _habitacionRepo.GetByAlojamientoIdAsync(alojamientoId);
        return items.Select(MapHabitacion);
    }

    public async Task<IEnumerable<DisponibilidadResponse>> GetDisponibilidadAsync(int habitacionId, DateOnly desde, DateOnly hasta)
    {
        var items = await _calendarioRepo.GetByHabitacionAsync(habitacionId, desde, hasta);
        return items.Select(c => new DisponibilidadResponse(c.HabitacionId, c.Fecha, c.Estado));
    }

    public async Task<IEnumerable<TipoAlojamientoResponse>> GetTiposAlojamientoAsync()
    {
        var items = await _tipoRepo.GetAllAsync();
        return items.Select(t => new TipoAlojamientoResponse(t.TipoAlojamientoId, t.Nombre, t.Descripcion));
    }

    public async Task<TipoAlojamientoResponse> CrearTipoAlojamientoAsync(CrearTipoAlojamientoRequest request)
    {
        var entity = new TipoAlojamientoEntity { Nombre = request.Nombre, Descripcion = request.Descripcion };
        var created = await _tipoRepo.AddAsync(entity);
        return new TipoAlojamientoResponse(created.TipoAlojamientoId, created.Nombre, created.Descripcion);
    }

    // ── Mappers privados ─────────────────────────────
    private static AlojamientoResponse MapAlojamiento(AlojamientoEntity a) => new(
        a.AlojamientoId, a.SocioId, a.Ciudad, a.Nombre, a.Descripcion, a.Direccion,
        a.Estrellas, a.CalificacionPromedio, a.TotalResenas, a.AdmiteMascotas,
        a.TienePiscina, a.TieneParqueadero, a.Estado,
        a.TipoAlojamiento?.Nombre,
        a.Fotos?.Select(f => new FotoResponse(f.FotoId, f.Url, f.Orden, f.Descripcion)).ToList(),
        a.Habitaciones?.Select(MapHabitacion).ToList()
    );

    private static HabitacionResponse MapHabitacion(HabitacionEntity h) => new(
        h.HabitacionId, h.Nombre, h.Descripcion, h.CapacidadAdultos, h.CapacidadNinos,
        h.NumBanos, h.NumDormitorios, h.TieneCocina, h.TieneAireAcondicionado,
        h.SuperficieM2, h.PrecioNoche
    );
}
