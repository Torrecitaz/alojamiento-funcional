using Microservicio.Clientes.Business.DTOs.Reservas;
using Microservicio.Clientes.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Microservicio.Clientes.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Reservas - Mateo Torres")]
public class ReservasController : ControllerBase
{
    private readonly IReservaService _service;

    public ReservasController(IReservaService service)
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

    [HttpGet("codigo/{codigo}")]
    public async Task<IActionResult> GetByCodigo(string codigo)
    {
        var result = await _service.GetByCodigoAsync(codigo);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("cliente/{clienteId}")]
    public async Task<IActionResult> GetByCliente(int clienteId)
        => Ok(await _service.GetByClienteIdAsync(clienteId));

    /// <summary>
    /// Crea una reserva completa con orquestación cross-DB.
    /// Valida disponibilidad, calcula noches, asigna código y bloquea fechas.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearReservaRequest request)
    {
        var result = await _service.CrearReservaAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.ReservaId }, result);
    }
}
