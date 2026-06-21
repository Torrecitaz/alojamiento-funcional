class Reserva {
  final int reservaId;
  final String codigoReserva;
  final int clienteId;
  final String? nombreCliente;
  final int alojamientoId;
  final String? nombrePropiedad;
  final int? habitacionId;
  final String? nombreHabitacion;
  final String fechaCheckIn;
  final String fechaCheckOut;
  final int numAdultos;
  final int numNinos;
  final bool llevaMascotas;
  final double total;
  final String estado;

  Reserva({
    required this.reservaId,
    required this.codigoReserva,
    required this.clienteId,
    this.nombreCliente,
    required this.alojamientoId,
    this.nombrePropiedad,
    this.habitacionId,
    this.nombreHabitacion,
    required this.fechaCheckIn,
    required this.fechaCheckOut,
    required this.numAdultos,
    required this.numNinos,
    required this.llevaMascotas,
    required this.total,
    required this.estado,
  });

  factory Reserva.fromJson(Map<String, dynamic> json) {
    return Reserva(
      reservaId: json['reservaId'] ?? 0,
      codigoReserva: json['codigoReserva'] ?? '',
      clienteId: json['clienteId'] ?? 0,
      nombreCliente: json['nombreCliente'],
      alojamientoId: json['alojamientoId'] ?? 0,
      nombrePropiedad: json['nombrePropiedad'] ?? json['nombreAlojamiento'],
      habitacionId: json['habitacionId'],
      nombreHabitacion: json['nombreHabitacion'],
      fechaCheckIn: json['fechaCheckIn'] ?? '',
      fechaCheckOut: json['fechaCheckOut'] ?? '',
      numAdultos: json['numAdultos'] ?? 1,
      numNinos: json['numNinos'] ?? 0,
      llevaMascotas: json['llevaMascotas'] ?? false,
      total: json['total'] != null
          ? double.parse(json['total'].toString())
          : (json['montoTotal'] != null
              ? double.parse(json['montoTotal'].toString())
              : 0.0),
      estado: json['estado'] ?? 'Pendiente',
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'reservaId': reservaId,
      'codigoReserva': codigoReserva,
      'clienteId': clienteId,
      'nombreCliente': nombreCliente,
      'alojamientoId': alojamientoId,
      'nombrePropiedad': nombrePropiedad,
      'habitacionId': habitacionId,
      'nombreHabitacion': nombreHabitacion,
      'fechaCheckIn': fechaCheckIn,
      'fechaCheckOut': fechaCheckOut,
      'numAdultos': numAdultos,
      'numNinos': numNinos,
      'llevaMascotas': llevaMascotas,
      'total': total,
      'estado': estado,
    };
  }
}
