using Usuarios.Business.DTOs;
using Usuarios.Business.Exceptions;
using Usuarios.Business.Interfaces;
using Usuarios.DataManagement.Interfaces;
using Usuarios.DataManagement.Models;

namespace Usuarios.Business.Services;

public class LocalizacionesService : ILocalizacionesService
{
    private readonly ILocalizacionesDataService _dataService;
    private readonly IUnitOfWork _unitOfWork;

    public LocalizacionesService(ILocalizacionesDataService dataService, IUnitOfWork unitOfWork)
    {
        _dataService = dataService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<LocalizacionResponse>> GetAllAsync()
    {
        var data = await _dataService.GetAllAsync();
        return data.Select(d => new LocalizacionResponse
        {
            LocalizacionId = d.LocalizacionId,
            Descripcion = d.Descripcion,
            PoligonoWkt = d.PoligonoWkt
        });
    }

    public async Task<LocalizacionResponse> GetByIdAsync(int id)
    {
        var data = await _dataService.GetByIdAsync(id);
        if (data == null) throw new LocalizacionNotFoundException(id);

        return new LocalizacionResponse
        {
            LocalizacionId = data.LocalizacionId,
            Descripcion = data.Descripcion,
            PoligonoWkt = data.PoligonoWkt
        };
    }

    public async Task<LocalizacionResponse> CrearAsync(CrearLocalizacionRequest request)
    {
        var model = new LocalizacionDataModel
        {
            Descripcion = request.Descripcion,
            PoligonoWkt = request.PoligonoWkt
        };

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var created = await _dataService.CreateAsync(model);
            await _unitOfWork.CommitTransactionAsync();

            return new LocalizacionResponse
            {
                LocalizacionId = created.LocalizacionId,
                Descripcion = created.Descripcion,
                PoligonoWkt = created.PoligonoWkt
            };
        }
        catch (ArgumentException ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw new BusinessRuleException(ex.Message);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task EliminarAsync(int id)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _dataService.DeleteAsync(id);
            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
