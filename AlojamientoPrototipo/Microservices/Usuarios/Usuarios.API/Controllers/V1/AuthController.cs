using Usuarios.Business.DTOs.Auth;
using Usuarios.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Usuarios.API.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    /// <summary>
    /// Stub: Login sin JWT funcional. Se implementará en fases posteriores.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (result == null) return Unauthorized(new { mensaje = "Credenciales inválidas" });
        return Ok(result);
    }
}
