using System.ComponentModel.DataAnnotations;

namespace Reservas.Business.DTOs;

public record ActualizarEstadoReservaRequest(
    [Required] [MaxLength(30)] string Estado
);
