using Facturacion.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Facturacion.API.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
public class MetodosPagoController : ControllerBase
{
    private readonly IMetodosPagoService _service;

    public MetodosPagoController(IMetodosPagoService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());
}
