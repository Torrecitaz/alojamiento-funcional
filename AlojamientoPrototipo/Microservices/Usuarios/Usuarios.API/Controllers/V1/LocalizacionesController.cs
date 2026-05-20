using Microsoft.AspNetCore.Mvc;
using Usuarios.Business.DTOs;
using Usuarios.Business.Interfaces;

namespace Usuarios.API.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
public class LocalizacionesController : ControllerBase
{
    private readonly ILocalizacionesService _service;

    public LocalizacionesController(ILocalizacionesService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        return Ok(await _service.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearLocalizacionRequest request)
    {
        var result = await _service.CrearAsync(request);
        return Created($"/api/v1/localizaciones/{result.LocalizacionId}", result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.EliminarAsync(id);
        return NoContent();
    }
}
