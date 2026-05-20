using Usuarios.Business.DTOs.Usuarios;
using Usuarios.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Usuarios.API.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuariosService _service;

    public UsuariosController(IUsuariosService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }
}
