using Usuarios.Business.DTOs.Clientes;
using Usuarios.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Usuarios.API.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly IClientesService _service;

    public ClientesController(IClientesService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string? nombre = null)
        => Ok(await _service.GetAllAsync(page, size, nombre));

    [HttpGet("{clienteId}")]
    public async Task<IActionResult> GetById(int clienteId)
    {
        var result = await _service.GetByIdAsync(clienteId);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("cedula/{cedula}")]
    public async Task<IActionResult> GetByCedula(string cedula)
    {
        var result = await _service.GetByCedulaAsync(cedula);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("registrar")]
    public async Task<IActionResult> Registrar([FromBody] RegistrarClienteRequest request)
    {
        await _service.RegistrarClienteAsync(request);
        return Created("", new { mensaje = "Cliente registrado exitosamente" });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarClienteRequest request)
    {
        await _service.ActualizarClienteAsync(id, request);
        return Ok(new { mensaje = "Cliente actualizado exitosamente" });
    }

    [HttpPatch("{id}/estado")]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoRequest request)
    {
        await _service.CambiarEstadoAsync(id, request);
        return Ok(new { mensaje = "Estado del cliente actualizado" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _service.EliminarClienteAsync(id);
        return NoContent();
    }
}
