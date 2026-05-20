using Alojamientos.Business.DTOs.Habitaciones;
using Alojamientos.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Alojamientos.API.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
public class HabitacionesController : ControllerBase
{
    private readonly IHabitacionesService _service;

    public HabitacionesController(IHabitacionesService service) => _service = service;

    [HttpGet("alojamiento/{alojamientoId}")]
    public async Task<IActionResult> GetByAlojamientoId(int alojamientoId)
        => Ok(await _service.GetByAlojamientoIdAsync(alojamientoId));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearHabitacionRequest request)
    {
        var result = await _service.CrearAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.HabitacionId }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarHabitacionRequest request)
    {
        await _service.ActualizarAsync(id, request);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _service.EliminarAsync(id);
        return NoContent();
    }
}
