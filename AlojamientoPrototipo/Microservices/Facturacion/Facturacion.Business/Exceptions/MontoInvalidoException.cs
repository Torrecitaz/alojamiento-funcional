namespace Facturacion.Business.Exceptions;

public class MontoInvalidoException : Exception
{
    public MontoInvalidoException(string mensaje) : base(mensaje) { }
}
