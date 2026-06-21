class Alojamiento {
  final int alojamientoId;
  final String nombre;
  final String? descripcion;
  final String? direccion;
  final String? ciudad;
  final int? ciudadId;
  final int estrellas;
  final double calificacionPromedio;
  final bool admiteMascotas;
  final String? imagenUrl;

  Alojamiento({
    required this.alojamientoId,
    required this.nombre,
    this.descripcion,
    this.direccion,
    this.ciudad,
    this.ciudadId,
    required this.estrellas,
    required this.calificacionPromedio,
    required this.admiteMascotas,
    this.imagenUrl,
  });

  factory Alojamiento.fromJson(Map<String, dynamic> json) {
    return Alojamiento(
      alojamientoId: json['alojamientoId'] ?? 0,
      nombre: json['nombre'] ?? '',
      descripcion: json['descripcion'],
      direccion: json['direccion'],
      ciudad: json['ciudad'],
      ciudadId: json['ciudadId'] != null ? int.tryParse(json['ciudadId'].toString()) : null,
      estrellas: json['estrellas'] ?? 0,
      calificacionPromedio: json['calificacionPromedio'] != null
          ? double.parse(json['calificacionPromedio'].toString())
          : 0.0,
      admiteMascotas: json['admiteMascotas'] ?? false,
      imagenUrl: json['imagenUrl'],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'alojamientoId': alojamientoId,
      'nombre': nombre,
      'descripcion': descripcion,
      'direccion': direccion,
      'ciudad': ciudad,
      'ciudadId': ciudadId,
      'estrellas': estrellas,
      'calificacionPromedio': calificacionPromedio,
      'admiteMascotas': admiteMascotas,
      'imagenUrl': imagenUrl,
    };
  }
}
