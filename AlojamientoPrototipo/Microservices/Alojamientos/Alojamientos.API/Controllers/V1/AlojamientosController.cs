using Alojamientos.Business.DTOs.Alojamientos;
using Alojamientos.Business.Interfaces;
using Alojamientos.DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Alojamientos.API.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
public class AlojamientosController : ControllerBase
{
    private readonly IAlojamientosService _service;
    private readonly ITiposAlojamientoRepository _tiposRepository;

    public AlojamientosController(IAlojamientosService service, ITiposAlojamientoRepository tiposRepository)
    {
        _service = service;
        _tiposRepository = tiposRepository;
    }

    [HttpGet("tipos")]
    public async Task<IActionResult> GetTipos()
        => Ok(await _tiposRepository.GetAllAsync());

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpGet("buscar")]
    public async Task<IActionResult> Buscar(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? ciudad = null,
        [FromQuery] string? tipo = null,
        [FromQuery] int? estrellas = null,
        [FromQuery] bool? admiteMascotas = null,
        [FromQuery] bool? tienePiscina = null)
    {
        var (items, totalRecords) = await _service.GetPagedAsync(page, pageSize, search, ciudad, tipo, estrellas, admiteMascotas, tienePiscina);
        return Ok(new AlojamientoPagedResponse(items, totalRecords));
    }

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
