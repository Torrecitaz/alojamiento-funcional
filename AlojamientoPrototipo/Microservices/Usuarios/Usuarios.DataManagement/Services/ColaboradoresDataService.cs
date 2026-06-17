using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Usuarios.DataAccess.Repositories.Interfaces;
using Usuarios.DataManagement.Interfaces;
using Usuarios.DataManagement.Mappers;
using Usuarios.DataManagement.Models;

namespace Usuarios.DataManagement.Services;

public class ColaboradoresDataService : IColaboradoresDataService
{
    private readonly IColaboradoresRepository _repo;

    public ColaboradoresDataService(IColaboradoresRepository repo) => _repo = repo;

    public async Task<IEnumerable<ColaboradorDataModel>> GetAllAsync()
    {
        var entities = await _repo.GetAllAsync();
        return entities.Select(ColaboradoresMapper.ToDataModel);
    }

    public async Task<ColaboradorDataModel?> GetByIdAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        return entity != null ? ColaboradoresMapper.ToDataModel(entity) : null;
    }

    public async Task<ColaboradorDataModel?> GetByUsuarioIdAsync(int usuarioId)
    {
        var entity = await _repo.GetByUsuarioIdAsync(usuarioId);
        return entity != null ? ColaboradoresMapper.ToDataModel(entity) : null;
    }

    public async Task<ColaboradorDataModel> CreateAsync(ColaboradorDataModel model)
    {
        var entity = new DataAccess.Entities.ColaboradorEntity
        {
            UsuarioId = model.UsuarioId,
            NombreEmpresa = model.NombreEmpresa,
            Telefono = model.Telefono
        };
        var created = await _repo.AddAsync(entity);
        return ColaboradoresMapper.ToDataModel(created);
    }

    public async Task UpdateAsync(ColaboradorDataModel model)
    {
        var entity = await _repo.GetByIdAsync(model.ColaboradorId);
        if (entity == null) throw new KeyNotFoundException($"Colaborador {model.ColaboradorId} no encontrado");
        ColaboradoresMapper.UpdateEntity(entity, model);
        await _repo.UpdateAsync(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null) throw new KeyNotFoundException($"Colaborador {id} no encontrado");
        await _repo.DeleteAsync(entity);
    }
}
