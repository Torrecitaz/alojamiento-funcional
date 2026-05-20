namespace Facturacion.Business.Exceptions;

public class FacturaNotFoundException : Exception
{
    public FacturaNotFoundException(int id) : base($"No se encontró la factura con ID {id}") { }
}
