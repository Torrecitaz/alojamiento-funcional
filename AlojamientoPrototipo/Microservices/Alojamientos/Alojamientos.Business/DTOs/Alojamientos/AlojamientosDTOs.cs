using System;
using System.ComponentModel.DataAnnotations;

namespace Alojamientos.Business.DTOs.Alojamientos;

public record CrearAlojamientoRequest(
    [Required] int SocioId, 
    [Required] int TipoAlojamientoId,
    [Required] [MaxLength(200)] string Nombre,
    [MaxLength(100)] string? Ciudad,
    [Required] [MaxLength(300)] string Direccion,
    string? Descripcion,
    bool AdmiteMascotas = false,
    bool TienePiscina = false,
    bool TieneParqueadero = false,
    [MaxLength(100)] string? Provincia = null,
    [MaxLength(100)] string? Pais = null,
    string? Politicas = null,
    [MaxLength(50)] string? CheckInTime = null,
    [MaxLength(50)] string? CheckOutTime = null,
    string? Servicios = null,
    double? Latitud = null,
    double? Longitud = null
);

public record ActualizarAlojamientoRequest(
    [Required] [MaxLength(200)] string Nombre,
    [MaxLength(100)] string? Ciudad,
    [Required] [MaxLength(300)] string Direccion,
    string? Descripcion,
    [Required] int TipoAlojamientoId,
    bool AdmiteMascotas,
    bool TienePiscina,
    bool TieneParqueadero,
    int? Estrellas,
    [MaxLength(100)] string? Provincia = null,
    [MaxLength(100)] string? Pais = null,
    string? Politicas = null,
    [MaxLength(50)] string? CheckInTime = null,
    [MaxLength(50)] string? CheckOutTime = null,
    string? Servicios = null,
    double? Latitud = null,
    double? Longitud = null,
    [MaxLength(20)] string? Estado = null
);

public record AlojamientoResponse(
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
