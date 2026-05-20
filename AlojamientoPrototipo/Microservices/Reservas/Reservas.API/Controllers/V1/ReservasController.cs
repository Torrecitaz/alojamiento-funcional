using Reservas.Business.DTOs;
using Reservas.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Reservas.API.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
public class ReservasController : ControllerBase
{
    private readonly IReservasService _service;

    public ReservasController(IReservasService service) => _service = service;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _service.GetByIdAsync(id));

    [HttpGet("cliente/{clienteId}")]
    public async Task<IActionResult> GetByClienteId(int clienteId)
        => Ok(await _service.GetByClienteIdAsync(clienteId));

    [HttpGet("resumen/cliente/{clienteId}")]
    public async Task<IActionResult> GetResumenByClienteId(int clienteId)
        => Ok(await _service.GetResumenByClienteIdAsync(clienteId));

    [HttpPost]
    public async Task<IActionResult> CrearReserva([FromBody] CrearReservaRequest request)
    {
        var result = await _service.CrearAsync(request);
        return Created($"/api/v1/reservas/{result.ReservaId}", result);
    }

    [HttpPatch("{id}/estado")]
    public async Task<IActionResult> ActualizarEstado(int id, [FromBody] ActualizarEstadoReservaRequest request)
    {
        await _service.ActualizarEstadoAsync(id, request);
        return NoContent();
    }
}
