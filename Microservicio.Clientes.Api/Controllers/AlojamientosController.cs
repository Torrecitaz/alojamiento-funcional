using Microservicio.Clientes.Business.DTOs.Alojamientos;
using Microservicio.Clientes.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Microservicio.Clientes.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Alojamientos - Mateo Torres")]
public class AlojamientosController : ControllerBase
{
    private readonly IAlojamientoService _service;

    public AlojamientosController(IAlojamientoService service)
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

    [HttpGet("{id}/detalle")]
    public async Task<IActionResult> GetWithHabitaciones(int id)
    {
        var result = await _service.GetWithHabitacionesAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("buscar/{ciudad}")]
    public async Task<IActionResult> GetByCiudad(string ciudad)
        => Ok(await _service.GetByCiudadAsync(ciudad));

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearAlojamientoRequest request)
    {
        var result = await _service.CrearAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.AlojamientoId }, result);
    }

    // ── Habitaciones ────────────────────────────────
    [HttpGet("{alojamientoId}/habitaciones")]
    public async Task<IActionResult> GetHabitaciones(int alojamientoId)
        => Ok(await _service.GetHabitacionesByAlojamientoAsync(alojamientoId));

    [HttpPost("habitaciones")]
    public async Task<IActionResult> CrearHabitacion([FromBody] CrearHabitacionRequest request)
    {
        var result = await _service.CrearHabitacionAsync(request);
        return Created("", result);
    }

    // ── Disponibilidad (Calendario) ─────────────────
    [HttpGet("habitaciones/{habitacionId}/disponibilidad")]
    public async Task<IActionResult> GetDisponibilidad(int habitacionId, [FromQuery] DateOnly desde, [FromQuery] DateOnly hasta)
        => Ok(await _service.GetDisponibilidadAsync(habitacionId, desde, hasta));

    // ── Tipos de Alojamiento ─────────────────────────
    [HttpGet("tipos")]
    public async Task<IActionResult> GetTipos()
        => Ok(await _service.GetTiposAlojamientoAsync());

    [HttpPost("tipos")]
    public async Task<IActionResult> CrearTipo([FromBody] CrearTipoAlojamientoRequest request)
    {
        var result = await _service.CrearTipoAlojamientoAsync(request);
        return Created("", result);
    }
}
