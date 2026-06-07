using System;
using System.Collections.Generic;

namespace ApiGateway.Models.Internal;

// Alojamientos microservice responses
public record AlojamientoInternalResponse(
    int AlojamientoId,
    int SocioId,
    int TipoAlojamientoId,
    string TipoAlojamientoNombre,
    string Nombre,
    string? Ciudad,
    string Direccion,
    string? Descripcion,
    int? Estrellas,
    decimal CalificacionPromedio,
    int TotalResenas,
    bool AdmiteMascotas,
    bool TienePiscina,
    bool TieneParqueadero,
    string Estado,
    DateTime FechaCreacion,
    string? Provincia = null,
    string? Pais = null,
    string? Politicas = null,
    string? CheckInTime = null,
    string? CheckOutTime = null,
    string? Servicios = null,
    double? Latitud = null,
    double? Longitud = null
);

public record HabitacionInternalResponse(
    int HabitacionId,
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
    decimal PrecioNoche,
    string Estado = "Activo",
    string? Fotos = null
);

public record FotoInternalResponse(
    int FotoId,
    int AlojamientoId,
    string Url,
    int Orden,
    string? Descripcion
);

public record TipoAlojamientoInternalResponse(
    int TipoAlojamientoId,
    string Nombre,
    string? Descripcion
);

public record CalendarioInternalResponse(
    int CalendarioId,
    int HabitacionId,
    DateOnly Fecha,
    string Estado
);

// Usuarios microservice responses
public record UsuarioInternalResponse(
    int UsuarioId,
    string Rol,
    string Email,
    string NombreCompleto,
    bool Estado,
    DateTime FechaCreacion
);

public record ClienteInternalResponse(
    int ClienteId,
    int? UsuarioId,
    string Cedula,
    string? FotoUrl,
    string? Telefono,
    string? Domicilio,
    string Email,
    int TotalReservas,
    DateTime FechaCreacion,
    UsuarioInternalResponse? Usuario
);


// Reservas microservice responses
public record ReservaInternalResponse(
    int ReservaId,
    int ClienteId,
    int AlojamientoId,
    DateOnly FechaCheckIn,
    DateOnly FechaCheckOut,
    int NumAdultos,
    int NumNinos,
    bool LlevaMascotas,
    int NumHabitaciones,
    decimal SubTotal,
    decimal Total,
    string Estado,
    string CodigoReserva,
    DateTime FechaCreacion,
    List<ReservaDetalleHabitacionInternalResponse> DetallesHabitacion,
    string? CodigoDescuentoAplicado,
    decimal? PorcentajeDescuento
);

public record ReservaDetalleHabitacionInternalResponse(
    int ReservaDetalleId,
    int HabitacionId,
    decimal PrecioPorNoche,
    int NumNoches,
    decimal SubTotalHabitacion
);

// Facturacion microservice responses
public record FacturaInternalResponse(
    int FacturaId,
    int ReservaId,
    int? MetodoPagoId,
    string? MetodoPagoTipo,
    decimal Monto,
    string Estado,
    DateTime? FechaPago,
    DateTime FechaCreacion
);

public record MetodoPagoInternalResponse(
    int MetodoPagoId,
    string Tipo
);
