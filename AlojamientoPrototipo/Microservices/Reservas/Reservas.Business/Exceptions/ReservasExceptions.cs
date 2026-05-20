namespace Reservas.Business.Exceptions;

public class ReservaNotFoundException : Exception
{
    public ReservaNotFoundException(int id) : base($"No se encontró la reserva con ID {id}") { }
}

public class FechasInvalidasException : Exception
{
    public FechasInvalidasException(string mensaje) : base(mensaje) { }
}

public class DescuentoInvalidoException : Exception
{
    public DescuentoInvalidoException(string codigo) : base($"El código de descuento '{codigo}' no es válido o ha expirado.") { }
}

public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}
