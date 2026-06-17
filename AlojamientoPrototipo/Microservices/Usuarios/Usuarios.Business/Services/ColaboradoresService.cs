using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Usuarios.Business.DTOs.Colaboradores;
using Usuarios.Business.Interfaces;
using Usuarios.Business.Mappers;
using Usuarios.DataManagement.Interfaces;
using Usuarios.DataManagement.Models;

namespace Usuarios.Business.Services;

public class ColaboradoresService : IColaboradoresService
{
    private readonly IColaboradoresDataService _dataService;
    private readonly IUsuariosDataService _usuariosDataService;

    public ColaboradoresService(IColaboradoresDataService dataService, IUsuariosDataService usuariosDataService)
    {
        _dataService = dataService;
        _usuariosDataService = usuariosDataService;
    }

    public async Task<IEnumerable<ColaboradorResponse>> GetAllAsync()
    {
        var models = await _dataService.GetAllAsync();
        return models.Select(ColaboradoresBusinessMapper.ToResponse);
    }

    public async Task<ColaboradorResponse?> GetByIdAsync(int id)
    {
        var model = await _dataService.GetByIdAsync(id);
        return model != null ? ColaboradoresBusinessMapper.ToResponse(model) : null;
    }

    public async Task<ColaboradorResponse?> GetByUsuarioIdAsync(int usuarioId)
    {
        var model = await _dataService.GetByUsuarioIdAsync(usuarioId);
        return model != null ? ColaboradoresBusinessMapper.ToResponse(model) : null;
    }

    public async Task<ColaboradorResponse> CreateAsync(CrearColaboradorRequest request)
    {
        // Verificar si el usuario existe
        var user = await _usuariosDataService.GetByIdAsync(request.UsuarioId);
        if (user == null)
        {
            throw new KeyNotFoundException($"Usuario {request.UsuarioId} no encontrado");
        }

        // Crear colaborador
        var model = new ColaboradorDataModel
        {
            UsuarioId = request.UsuarioId,
            NombreEmpresa = request.NombreEmpresa,
            Telefono = request.Telefono
        };

        var created = await _dataService.CreateAsync(model);
        // Recargar con los datos del usuario para el response
        var fullModel = await _dataService.GetByIdAsync(created.ColaboradorId);
        return ColaboradoresBusinessMapper.ToResponse(fullModel ?? created);
    }

    public async Task DeleteAsync(int id)
    {
        await _dataService.DeleteAsync(id);
    }
}
