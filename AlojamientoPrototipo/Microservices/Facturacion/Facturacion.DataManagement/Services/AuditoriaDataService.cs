using Facturacion.DataAccess.Entities;
using Facturacion.DataAccess.Repositories.Interfaces;
using Facturacion.DataManagement.Interfaces;
using Facturacion.DataManagement.Models;

namespace Facturacion.DataManagement.Services;

public class AuditoriaDataService : IAuditoriaDataService
{
    private readonly IAuditoriaRepository _repo;

    public AuditoriaDataService(IAuditoriaRepository repo) => _repo = repo;

    public async Task RegistrarAccionAsync(AuditoriaGeneralDataModel model)
    {
        var entity = new AuditoriaGeneralEntity
        {
            NombreTabla = model.NombreTabla,
            Operacion = model.Operacion,
            RegistroId = model.RegistroId,
            DatosAnteriores = model.DatosAnteriores,
            DatosNuevos = model.DatosNuevos,
            UsuarioAccion = model.UsuarioAccion,
            Origen = model.Origen
        };

        await _repo.AddAsync(entity);
    }
}
