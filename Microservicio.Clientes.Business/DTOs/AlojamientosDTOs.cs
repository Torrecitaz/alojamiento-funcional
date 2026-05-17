namespace Microservicio.Clientes.Business.DTOs.Alojamientos;

// ── Request DTOs ─────────────────────────────────────
public record CrearAlojamientoRequest(
    int SocioId,
    int TipoAlojamientoId,
    string Ciudad,
    string Nombre,
    string? Descripcion,
    string Direccion,
    int? Estrellas,
    bool AdmiteMascotas,
    bool TienePiscina,
    bool TieneParqueadero
);

public record CrearHabitacionRequest(
    int AlojamientoId,
    string Nombre,
    string? Descripcion,
    int CapacidadAdultos,
    int CapacidadNinos,
    int NumBanos,
    int NumDormitorios,
    bool TieneCocina,
    bool TieneAireAcondicionado,
    decimal? SuperficieM2,
    decimal PrecioNoche
);

public record CrearTipoAlojamientoRequest(
    string Nombre,
    string? Descripcion
);

// ── Response DTOs ────────────────────────────────────
public record AlojamientoResponse(
    int AlojamientoId,
    int SocioId,
    string? Ciudad,
    string Nombre,
    string? Descripcion,
    string Direccion,
    int? Estrellas,
    decimal CalificacionPromedio,
    int TotalResenas,
    bool AdmiteMascotas,
    bool TienePiscina,
    bool TieneParqueadero,
    string Estado,
    string? TipoAlojamiento,
    List<FotoResponse>? Fotos,
    List<HabitacionResponse>? Habitaciones
);

public record HabitacionResponse(
    int HabitacionId,
    string Nombre,
    string? Descripcion,
    int CapacidadAdultos,
    int CapacidadNinos,
    int NumBanos,
    int NumDormitorios,
    bool TieneCocina,
    bool TieneAireAcondicionado,
    decimal? SuperficieM2,
    decimal PrecioNoche
);

public record FotoResponse(
    int FotoId,
    string Url,
    int Orden,
    string? Descripcion
);

public record TipoAlojamientoResponse(
    int TipoAlojamientoId,
    string Nombre,
    string? Descripcion
);

public record DisponibilidadResponse(
    int HabitacionId,
    DateOnly Fecha,
    string Estado
);
