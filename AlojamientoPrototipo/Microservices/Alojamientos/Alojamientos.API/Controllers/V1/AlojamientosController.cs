using Alojamientos.Business.DTOs.Alojamientos;
using Alojamientos.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Alojamientos.API.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
public class AlojamientosController : ControllerBase
{
    private readonly IAlojamientosService _service;

    public AlojamientosController(IAlojamientosService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearAlojamientoRequest request)
    {
        var result = await _service.CrearAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.AlojamientoId }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarAlojamientoRequest request)
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
