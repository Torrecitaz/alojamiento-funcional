namespace Alojamientos.Business.Exceptions;

public class AlojamientoNotFoundException : Exception
{
    public AlojamientoNotFoundException(int id) : base($"No se encontró el alojamiento con ID {id}") { }
}

public class HabitacionNotFoundException : Exception
{
    public HabitacionNotFoundException(int id) : base($"No se encontró la habitación con ID {id}") { }
}

public class FotoNotFoundException : Exception
{
    public FotoNotFoundException(int id) : base($"No se encontró la foto con ID {id}") { }
}

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}
