namespace Usuarios.Business.DTOs;

public record LocalizacionResponse
{
    public int LocalizacionId { get; init; }
    public string? Descripcion { get; init; }
    public string? PoligonoWkt { get; init; }
}

public record CrearLocalizacionRequest
{
    public string? Descripcion { get; init; }
    public string? PoligonoWkt { get; init; }
}
