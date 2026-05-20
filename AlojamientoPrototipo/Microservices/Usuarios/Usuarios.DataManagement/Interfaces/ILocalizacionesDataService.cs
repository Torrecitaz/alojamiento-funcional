using Usuarios.DataManagement.Models;

namespace Usuarios.DataManagement.Interfaces;

public interface ILocalizacionesDataService
{
    Task<IEnumerable<LocalizacionDataModel>> GetAllAsync();
    Task<LocalizacionDataModel?> GetByIdAsync(int id);
    Task<LocalizacionDataModel> CreateAsync(LocalizacionDataModel model);
    Task DeleteAsync(int id);
}
