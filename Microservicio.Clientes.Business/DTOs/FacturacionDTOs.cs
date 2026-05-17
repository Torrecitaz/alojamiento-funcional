namespace Microservicio.Clientes.Business.DTOs.Facturacion;

// ── Request DTOs ─────────────────────────────────────
public record CrearFacturaRequest(
    int ReservaId,
    int MetodoPagoId,
    decimal Monto,
    string Descripcion
);

// ── Response DTOs ────────────────────────────────────
public record FacturaResponse(
    int FacturaId,
    int ReservaId,
    int? MetodoPagoId,
    decimal Monto,
    string Estado,
    DateTime? FechaPago,
    DateTime FechaCreacion,
    string? MetodoPagoTipo,
    List<DetalleFacturaResponse>? Detalles
);

public record DetalleFacturaResponse(
    int DetalleFacturaId,
    string Descripcion,
    int Cantidad,
    decimal PrecioUnitario
);

public record MetodoPagoResponse(
    int MetodoPagoId,
    string Tipo
);
