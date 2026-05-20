using Facturacion.DataManagement.Models;

namespace Facturacion.DataManagement.Interfaces;

public interface IAuditoriaDataService
{
    Task RegistrarAccionAsync(AuditoriaGeneralDataModel model);
}
