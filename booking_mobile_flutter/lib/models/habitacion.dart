class Habitacion {
  final int habitacionId;
  final int alojamientoId;
  final String nombre;
  final String? descripcion;
  final double precioPorNoche;
  final int capacidadAdultos;
  final int capacidadNinos;
  final String? tipoHabitacion;
  final bool activa;

  Habitacion({
    required this.habitacionId,
    required this.alojamientoId,
    required this.nombre,
    this.descripcion,
    required this.precioPorNoche,
    required this.capacidadAdultos,
    required this.capacidadNinos,
    this.tipoHabitacion,
    required this.activa,
  });

  factory Habitacion.fromJson(Map<String, dynamic> json) {
    return Habitacion(
      habitacionId: json['habitacionId'] ?? 0,
      alojamientoId: json['alojamientoId'] ?? 0,
      nombre: json['nombre'] ?? '',
      descripcion: json['descripcion'],
      precioPorNoche: json['precioPorNoche'] != null
          ? double.parse(json['precioPorNoche'].toString())
          : 0.0,
      capacidadAdultos: json['capacidadAdultos'] ?? 0,
      capacidadNinos: json['capacidadNinos'] ?? 0,
      tipoHabitacion: json['tipoHabitacion'],
      activa: json['activa'] ?? json['activo'] ?? true,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'habitacionId': habitacionId,
      'alojamientoId': alojamientoId,
      'nombre': nombre,
      'descripcion': descripcion,
      'precioPorNoche': precioPorNoche,
      'capacidadAdultos': capacidadAdultos,
      'capacidadNinos': capacidadNinos,
      'tipoHabitacion': tipoHabitacion,
      'activa': activa,
    };
  }
}
