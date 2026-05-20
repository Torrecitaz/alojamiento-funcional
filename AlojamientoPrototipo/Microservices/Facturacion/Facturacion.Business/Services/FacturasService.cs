using Facturacion.Business.DTOs;
using Facturacion.Business.Exceptions;
using Facturacion.Business.Interfaces;
using Facturacion.Business.Mappers;
using Facturacion.DataManagement.Interfaces;
using Facturacion.DataManagement.Models;
using MassTransit;
using Shared.Kernel.Events;

namespace Facturacion.Business.Services;

public class FacturasService : IFacturasService
{
    private readonly IFacturasDataService _facturasDataService;
    private readonly IAuditoriaDataService _auditoriaDataService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;

    public FacturasService(
        IFacturasDataService facturasDataService,
        IAuditoriaDataService auditoriaDataService,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint)
    {
        _facturasDataService = facturasDataService;
        _auditoriaDataService = auditoriaDataService;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<FacturaResponse> GetByIdAsync(int id)
    {
        var factura = await _facturasDataService.GetByIdAsync(id);
        if (factura == null) throw new FacturaNotFoundException(id);
        return FacturacionBusinessMapper.ToResponse(factura);
    }

    public async Task<IEnumerable<FacturaResponse>> GetByReservaIdAsync(int reservaId)
    {
        var facturas = await _facturasDataService.GetByReservaIdAsync(reservaId);
        return facturas.Select(FacturacionBusinessMapper.ToResponse);
    }

    public async Task<IEnumerable<FacturaResumenResponse>> GetResumenByReservaIdAsync(int reservaId)
    {
        var facturas = await _facturasDataService.GetByReservaIdAsync(reservaId);
        return facturas.Select(FacturacionBusinessMapper.ToResumenResponse);
    }

    public async Task<FacturaResponse> CrearAsync(CrearFacturaRequest request)
    {
        // Validación de negocio adicional
        decimal totalCalculado = request.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);
        if (request.Monto != totalCalculado)
        {
            throw new MontoInvalidoException($"El monto total ({request.Monto}) no coincide con la suma de los detalles ({totalCalculado}).");
        }

        var model = new FacturaDataModel
        {
            ReservaId = request.ReservaId,
            MetodoPagoId = request.MetodoPagoId,
            Monto = request.Monto,
            FechaPago = request.FechaPago,
            Estado = request.FechaPago.HasValue ? "Pagado" : "Pendiente",
            Detalles = request.Detalles.Select(d => new DetalleFacturaDataModel
            {
                Descripcion = d.Descripcion,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario
            }).ToList()
        };

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var created = await _facturasDataService.CreateAsync(model);

            // Registrar en Auditoría
            await _auditoriaDataService.RegistrarAccionAsync(new AuditoriaGeneralDataModel
            {
                NombreTabla = "Facturas",
                Operacion = "INSERT",
                RegistroId = created.FacturaId.ToString(),
                DatosNuevos = $"ReservaId: {created.ReservaId}, Monto: {created.Monto}",
                UsuarioAccion = "Sistema_Facturacion",
                Origen = "FacturasService.CrearAsync"
            });

            await _unitOfWork.CommitTransactionAsync();
            return await GetByIdAsync(created.FacturaId);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task AprobarFacturaAsync(int id)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // Obtener la factura para conocer el ReservaId y Monto
            var factura = await _facturasDataService.GetByIdAsync(id)
                ?? throw new FacturaNotFoundException(id);

            await _facturasDataService.UpdateStatusAsync(id, "Pagado");

            await _auditoriaDataService.RegistrarAccionAsync(new AuditoriaGeneralDataModel
            {
                NombreTabla = "Facturas",
                Operacion = "UPDATE",
                RegistroId = id.ToString(),
                DatosNuevos = "Estado -> Pagado",
                UsuarioAccion = "Sistema_Facturacion",
                Origen = "FacturasService.AprobarFacturaAsync"
            });

            await _unitOfWork.CommitTransactionAsync();

            // ── Publicar evento asíncrono a RabbitMQ (CloudAMQP) ──
            await _publishEndpoint.Publish(new FacturaPagadaEvent
            {
                ReservaId = factura.ReservaId,
                FacturaId = factura.FacturaId,
                MontoPagado = factura.Monto,
                FechaPago = DateTime.UtcNow
            });
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task RechazarFacturaAsync(int id)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _facturasDataService.UpdateStatusAsync(id, "Rechazado");

            await _auditoriaDataService.RegistrarAccionAsync(new AuditoriaGeneralDataModel
            {
                NombreTabla = "Facturas",
                Operacion = "UPDATE",
                RegistroId = id.ToString(),
                DatosNuevos = "Estado -> Rechazado",
                UsuarioAccion = "Sistema_Facturacion",
                Origen = "FacturasService.RechazarFacturaAsync"
            });

            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
