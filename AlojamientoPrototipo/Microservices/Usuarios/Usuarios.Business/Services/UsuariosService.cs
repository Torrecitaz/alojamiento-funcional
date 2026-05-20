using Usuarios.Business.DTOs.Usuarios;
using Usuarios.Business.Interfaces;
using Usuarios.Business.Mappers;
using Usuarios.DataManagement.Interfaces;

namespace Usuarios.Business.Services;

public class UsuariosService : IUsuariosService
{
    private readonly IUsuariosDataService _dataService;

    public UsuariosService(IUsuariosDataService dataService) => _dataService = dataService;

    public async Task<IEnumerable<UsuarioResponse>> GetAllAsync()
    {
        var models = await _dataService.GetAllAsync();
        return models.Select(UsuariosBusinessMapper.ToResponse);
    }

    public async Task<UsuarioResponse?> GetByIdAsync(int id)
    {
        var model = await _dataService.GetByIdAsync(id);
        return model != null ? UsuariosBusinessMapper.ToResponse(model) : null;
    }
}
