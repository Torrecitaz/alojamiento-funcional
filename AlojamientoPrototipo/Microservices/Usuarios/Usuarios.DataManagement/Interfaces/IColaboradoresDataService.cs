using System.Collections.Generic;
using System.Threading.Tasks;
using Usuarios.DataManagement.Models;

namespace Usuarios.DataManagement.Interfaces;

public interface IColaboradoresDataService
{
    Task<IEnumerable<ColaboradorDataModel>> GetAllAsync();
    Task<ColaboradorDataModel?> GetByIdAsync(int id);
    Task<ColaboradorDataModel?> GetByUsuarioIdAsync(int usuarioId);
    Task<ColaboradorDataModel> CreateAsync(ColaboradorDataModel model);
    Task UpdateAsync(ColaboradorDataModel model);
    Task DeleteAsync(int id);
}
