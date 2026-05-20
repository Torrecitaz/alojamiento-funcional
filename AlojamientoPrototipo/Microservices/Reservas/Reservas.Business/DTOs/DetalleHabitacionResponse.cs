namespace Reservas.Business.DTOs;

public record DetalleHabitacionResponse(
    int DetalleId,
    int HabitacionId,
    decimal PrecioPorNoche,
    int NumNoches,
    decimal SubTotalHabitacion
);
