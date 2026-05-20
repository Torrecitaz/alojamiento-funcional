using Alojamientos.Business.DTOs.Fotos;
using Alojamientos.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Alojamientos.API.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
public class FotosController : ControllerBase
{
    private readonly IFotosService _service;

    public FotosController(IFotosService service) => _service = service;

    [HttpGet("alojamiento/{alojamientoId}")]
    public async Task<IActionResult> GetByAlojamientoId(int alojamientoId)
        => Ok(await _service.GetByAlojamientoIdAsync(alojamientoId));

    [HttpPost]
    public async Task<IActionResult> Agregar([FromBody] AgregarFotoRequest request)
    {
        var result = await _service.AgregarAsync(request);
        return Created("", result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _service.EliminarAsync(id);
        return NoContent();
    }
}
