using Usuarios.Business.DTOs.Auth;
using Usuarios.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Usuarios.API.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUsuariosService _usuariosService;

    public AuthController(IAuthService authService, IUsuariosService usuariosService)
    {
        _authService = authService;
        _usuariosService = usuariosService;
    }

    /// <summary>
    /// Stub: Login sin JWT funcional. Se implementará en fases posteriores.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (result == null) return Unauthorized(new { mensaje = "Credenciales inválidas" });
        return Ok(new { datos = result });
    }

    [HttpGet("usuarios")]
    public async Task<IActionResult> GetUsuarios()
    {
        var list = await _usuariosService.GetAllAsync();
        var mapped = list.Select(u => new
        {
            usuarioId = u.UsuarioId,
            nombreCompleto = u.NombreCompleto,
            email = u.Email,
            rolNombre = u.Rol,
            estado = u.Estado,
            fechaCreacion = u.FechaCreacion
        });
        return Ok(new { datos = mapped });
    }

    [HttpGet("roles")]
    public IActionResult GetRoles()
    {
        var roles = new[]
        {
            new { rolId = 1, rolNombre = "Cliente" },
            new { rolId = 2, rolNombre = "Colaborador" },
            new { rolId = 3, rolNombre = "Administrador" }
        };
        return Ok(new { datos = roles });
    }

    [HttpPatch("usuarios/{id}/rol")]
    public async Task<IActionResult> UpdateRol(int id, [FromBody] UpdateRolRequest request)
    {
        try
        {
            string rolName = request.RolId switch
            {
                1 => "Cliente",
                2 => "Colaborador",
                3 => "Administrador",
                _ => throw new ArgumentException("RolId inválido")
            };

            await _usuariosService.UpdateRolAsync(id, rolName);
            return Ok(new { mensaje = "Rol actualizado exitosamente" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    [HttpPatch("usuarios/{id}/estado")]
    public async Task<IActionResult> UpdateEstado(int id, [FromBody] UpdateEstadoRequest request)
    {
        try
        {
            await _usuariosService.UpdateEstadoAsync(id, request.Activo);
            return Ok(new { mensaje = "Estado actualizado exitosamente" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }
}

public record UpdateRolRequest(int RolId);
public record UpdateEstadoRequest(bool Activo);
