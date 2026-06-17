using System;
using System.ComponentModel.DataAnnotations;

namespace Usuarios.Business.DTOs.Colaboradores;

public record CrearColaboradorRequest(
    [Required] int UsuarioId,
    [Required] [MaxLength(200)] string NombreEmpresa,
    [MaxLength(50)] string? Telefono
);

public record ColaboradorResponse(
    int ColaboradorId,
    int? UsuarioId,
    string NombreEmpresa,
    string? Telefono,
    DateTime FechaCreacion,
    string Email,
    string NombreCompleto,
    int TotalPropiedades = 0
);
