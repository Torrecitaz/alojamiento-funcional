using Alojamientos.Business.DTOs;
using Alojamientos.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Alojamientos.API.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
public class CalendarioController : ControllerBase
{
    private readonly ICalendarioService _service;

    public CalendarioController(ICalendarioService service)
    {
        _service = service;
    }

    [HttpGet("habitacion/{habitacionId}")]
    public async Task<IActionResult> GetDisponibilidad(int habitacionId, [FromQuery] int mes, [FromQuery] int anio)
    {
        var result = await _service.GetDisponibilidadMensualAsync(habitacionId, mes, anio);
        return Ok(result);
    }

    [HttpPost("bloquear")]
    public async Task<IActionResult> BloquearFechas([FromBody] BloquearFechasRequest request)
    {
        var result = await _service.BloquearFechasAsync(request);
        return Ok(result);
    }
}
