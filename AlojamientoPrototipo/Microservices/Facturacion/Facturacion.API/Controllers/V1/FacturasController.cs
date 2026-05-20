using Facturacion.Business.DTOs;
using Facturacion.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Facturacion.API.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
public class FacturasController : ControllerBase
{
    private readonly IFacturasService _service;

    public FacturasController(IFacturasService service) => _service = service;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _service.GetByIdAsync(id));

    [HttpGet("reserva/{reservaId}")]
    public async Task<IActionResult> GetByReservaId(int reservaId)
        => Ok(await _service.GetByReservaIdAsync(reservaId));

    [HttpGet("resumen/reserva/{reservaId}")]
    public async Task<IActionResult> GetResumenByReservaId(int reservaId)
        => Ok(await _service.GetResumenByReservaIdAsync(reservaId));

    [HttpPost]
    public async Task<IActionResult> CrearFactura([FromBody] CrearFacturaRequest request)
    {
        var result = await _service.CrearAsync(request);
        return Created($"/api/v1/facturas/{result.FacturaId}", result);
    }

    [HttpPatch("{id}/aprobar")]
    public async Task<IActionResult> AprobarFactura(int id)
    {
        await _service.AprobarFacturaAsync(id);
        return NoContent();
    }

    [HttpPatch("{id}/rechazar")]
    public async Task<IActionResult> RechazarFactura(int id)
    {
        await _service.RechazarFacturaAsync(id);
        return NoContent();
    }
}
