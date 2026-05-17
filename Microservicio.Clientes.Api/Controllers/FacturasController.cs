using Microservicio.Clientes.Business.DTOs.Facturacion;
using Microservicio.Clientes.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Microservicio.Clientes.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Facturas - Mateo Torres")]
public class FacturasController : ControllerBase
{
    private readonly IFacturaService _service;

    public FacturasController(IFacturaService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("reserva/{reservaId}")]
    public async Task<IActionResult> GetByReserva(int reservaId)
        => Ok(await _service.GetByReservaIdAsync(reservaId));

    /// <summary>
    /// Crea una Factura completa usando el SP sp_registrar_factura_completa.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearFacturaRequest request)
    {
        await _service.CrearFacturaAsync(request);
        return Created("", new { mensaje = "Factura registrada exitosamente" });
    }

    [HttpGet("metodos-pago")]
    public async Task<IActionResult> GetMetodosPago()
        => Ok(await _service.GetMetodosPagoAsync());
}
