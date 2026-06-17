using System.Collections.Generic;
using System.Threading.Tasks;
using Usuarios.Business.DTOs.Colaboradores;

namespace Usuarios.Business.Interfaces;

public interface IColaboradoresService
{
    Task<IEnumerable<ColaboradorResponse>> GetAllAsync();
    Task<ColaboradorResponse?> GetByIdAsync(int id);
    Task<ColaboradorResponse?> GetByUsuarioIdAsync(int usuarioId);
    Task<ColaboradorResponse> CreateAsync(CrearColaboradorRequest request);
    Task DeleteAsync(int id);
}
