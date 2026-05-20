namespace Usuarios.Business.Exceptions;

public class UsuarioNotFoundException : Exception
{
    public UsuarioNotFoundException(int id) : base($"No se encontró el usuario con ID {id}") { }
    public UsuarioNotFoundException(string email) : base($"No se encontró el usuario con email {email}") { }
}

public class ClienteNotFoundException : Exception
{
    public ClienteNotFoundException(int id) : base($"No se encontró el cliente con ID {id}") { }
    public ClienteNotFoundException(string cedula) : base($"No se encontró el cliente con cédula {cedula}") { }
}

public class EmailAlreadyExistsException : Exception
{
    public EmailAlreadyExistsException(string email) : base($"El email {email} ya está registrado") { }
}

public class AuthException : Exception
{
    public AuthException(string message) : base(message) { }
}

public class LocalizacionNotFoundException : Exception
{
    public LocalizacionNotFoundException(int id) : base($"No se encontró la localización con ID {id}") { }
}

public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}
