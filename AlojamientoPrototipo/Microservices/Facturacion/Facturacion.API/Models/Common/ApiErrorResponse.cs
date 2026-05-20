namespace Facturacion.API.Models.Common;

public class ApiErrorResponse
{
    public bool Success { get; set; } = false;
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }

    public ApiErrorResponse(string message, string? details = null)
    {
        Message = message;
        Details = details;
    }
}
