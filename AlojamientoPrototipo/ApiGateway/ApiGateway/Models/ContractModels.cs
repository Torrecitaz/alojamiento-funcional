using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using System.Text.Json.Serialization;

namespace ApiGateway.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }

    [JsonPropertyName("datos")]
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ApiResponse<T> Ok(T? data, string? message = null) => new()
    {
        Success = true,
        Data = data,
        Message = message,
        Errors = new()
    };

    public static ApiResponse<T> Fail(string message, List<string>? errors = null) => new()
    {
        Success = false,
        Data = default,
        Message = message,
        Errors = errors ?? new()
    };
}

public record AlojamientoDto
{
    public int AlojamientoId { get; init; }
    public int PropiedadId => AlojamientoId; // Fallback mapping for admin frontend compatibility
    public string Nombre { get; init; } = string.Empty;
    public string TipoAlojamiento { get; init; } = string.Empty;
    public string Ciudad { get; init; } = string.Empty;
    public string Direccion { get; init; } = string.Empty;
    public decimal PrecioNocheMinimo { get; init; }
    public string Moneda { get; init; } = "USD";
    public int? Estrellas { get; init; }
    public string? ImagenUrl { get; init; }
    public bool AdmiteMascotas { get; init; }
    public bool TienePiscina { get; init; }
    public bool TieneParqueadero { get; init; }
    public bool Disponible { get; init; }
}

public record AlojamientoDetalleDto : AlojamientoDto
{
    public string? Descripcion { get; init; }
    public decimal CalificacionPromedio { get; init; }
    public int TotalResenas { get; init; }
    public List<FotoDto> Fotos { get; init; } = new();
}

public record FotoDto
{
    public string Url { get; init; } = string.Empty;
    public string? Descripcion { get; init; }
}

public record TipoAlojamientoDto
{
    public int TipoAlojamientoId { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string? Descripcion { get; init; }
}

public record DisponibilidadDto
{
    public int AlojamientoId { get; init; }
    public DateOnly FechaDesde { get; init; }
    public DateOnly FechaHasta { get; init; }
    public int TotalNoches { get; init; }
    public List<HabitacionDisponibleDto> HabitacionesDisponibles { get; init; } = new();
}

public record HabitacionDisponibleDto
{
    public int HabitacionId { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string? Descripcion { get; init; }
    public decimal PrecioNoche { get; init; }
    public decimal PrecioTotal { get; init; }
    public string Moneda { get; init; } = "USD";
    public int CapacidadAdultos { get; init; }
    public int CapacidadNinos { get; init; }
    public int NumDormitorios { get; init; }
    public int NumBanos { get; init; }
    public bool TieneCocina { get; init; }
    public bool TieneAireAcondicionado { get; init; }
    public decimal? SuperficieM2 { get; init; }
}

public record ClienteDto
{
    public int ClienteId { get; init; }
    public string NombreCompleto { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Cedula { get; init; } = string.Empty;
    public string? Telefono { get; init; }
    public string? Domicilio { get; init; }
    public int TotalReservas { get; init; }
    public DateTime FechaCreacion { get; init; }
}

public record RegistrarClienteRequest
{
    [Required]
    public string Email { get; init; } = string.Empty;
    
    [Required]
    public string Password { get; init; } = string.Empty;
    
    [Required]
    public string NombreCompleto { get; init; } = string.Empty;
    
    public string? Cedula { get; init; }
    
    public string? Telefono { get; init; }
    
    public string? Domicilio { get; init; }
}

public record ReservaDto
{
    public int ReservaId { get; init; }
    public string CodigoReserva { get; init; } = string.Empty;
    public int AlojamientoId { get; init; }
    public string NombreAlojamiento { get; init; } = string.Empty;
    public string NombrePropiedad { get; init; } = string.Empty;
    public string NombreCliente { get; init; } = string.Empty;
    public DateOnly FechaCheckIn { get; init; }
    public DateOnly FechaCheckOut { get; init; }
    public int NumNoches { get; init; }
    public int NumAdultos { get; init; }
    public int NumNinos { get; init; }
    public bool LlevaMascotas { get; init; }
    public int NumHabitaciones { get; init; }
    public decimal SubTotal { get; init; }
    public decimal? Descuento { get; init; }
    public decimal Total { get; init; }
    public string Moneda { get; init; } = "USD";
    public string Estado { get; init; } = string.Empty;
    public DateTime FechaCreacion { get; init; }
}

public record CrearReservaRequest
{
    [Required]
    public int ClienteId { get; init; }
    
    [Required]
    public int AlojamientoId { get; init; }
    
    [Required]
    public DateOnly FechaCheckIn { get; init; }
    
    [Required]
    public DateOnly FechaCheckOut { get; init; }
    
    [Required]
    public int NumAdultos { get; init; }
    
    public int NumNinos { get; init; }
    public bool LlevaMascotas { get; init; }
    public string? CodigoDescuento { get; init; }
    public Guid? ExternalId { get; init; }
    
    [Required]
    public List<CrearReservaHabitacionDto> Habitaciones { get; init; } = new();
}

public record CrearReservaHabitacionDto
{
    [Required]
    public int HabitacionId { get; init; }
    
    [Required]
    public decimal PrecioPorNoche { get; init; }
    
    [Required]
    public int NumNoches { get; init; }
}

public record FacturaDto
{
    public int FacturaId { get; init; }
    public int ReservaId { get; init; }
    public string CodigoReserva { get; init; } = string.Empty;
    public decimal Monto { get; init; }
    public string Moneda { get; init; } = "USD";
    public string MetodoPago { get; init; } = string.Empty;
    public string Estado { get; init; } = string.Empty;
    public DateTime? FechaPago { get; init; }
    public DateTime FechaCreacion { get; init; }
}

public record MetodoPagoDto
{
    public int MetodoPagoId { get; init; }
    public string Tipo { get; init; } = string.Empty;
}

/// <summary>
/// Payload que envía Booking al hacer checkout.
/// idCarrito = codigoReserva (UUID string).
/// metodoPagoId = ExternalId del método de pago (UUID string registrado en la BD).
/// </summary>
public record CheckoutBookingRequest
{
    /// <summary>UUID del carrito/reserva en Booking (equivale al ReservaId interno).</summary>
    [Required]
    public string IdCarrito { get; init; } = string.Empty;

    /// <summary>UUID del método de pago en Booking (se resuelve al MetodoPagoId interno).</summary>
    [Required]
    public string MetodoPagoId { get; init; } = string.Empty;

    /// <summary>Moneda solicitada (por defecto USD).</summary>
    public string Currency { get; init; } = "USD";
}

public record CheckoutBookingResponse
{
    public int ReservaId { get; init; }
    public string CodigoReserva { get; init; } = string.Empty;
    public int FacturaId { get; init; }
    public decimal Monto { get; init; }
    public string Moneda { get; init; } = "USD";
    public string Estado { get; init; } = string.Empty;
}

public record ReservaBookingRequestDtoV2
{
    [Required]
    public int ClienteId { get; init; }
    
    [Required]
    public int HabitacionId { get; init; }
    
    [Required]
    public DateOnly FechaCheckIn { get; init; }
    
    [Required]
    public DateOnly FechaCheckOut { get; init; }
    
    [Required]
    public int NumAdultos { get; init; }
    
    public int NumNinos { get; init; }
    public bool LlevaMascotas { get; init; }
    public string? CodigoDescuento { get; init; }
}

public record LoginRequestDto
{
    [Required]
    public string Email { get; init; } = string.Empty;
    
    [Required]
    public string Password { get; init; } = string.Empty;
}

public record LoginResponseDto
{
    public string Token { get; init; } = string.Empty;
    public string NombreCompleto { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public List<string> Roles { get; init; } = new();
    public int? ClienteId { get; init; }
    public int? ColaboradorId { get; init; }
}

public class DownstreamLoginResponse
{
    [JsonPropertyName("datos")]
    public LoginResponseDto? Datos { get; set; }
}

