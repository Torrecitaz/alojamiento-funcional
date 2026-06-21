class Factura {
  final int facturaId;
  final int reservaId;
  final double monto;
  final String? fechaPago;
  final String? fechaCreacion;
  final String? metodoPago;
  final String? metodoPagoTipo;

  Factura({
    required this.facturaId,
    required this.reservaId,
    required this.monto,
    this.fechaPago,
    this.fechaCreacion,
    this.metodoPago,
    this.metodoPagoTipo,
  });

  factory Factura.fromJson(Map<String, dynamic> json) {
    return Factura(
      facturaId: json['facturaId'] ?? 0,
      reservaId: json['reservaId'] ?? 0,
      monto: json['monto'] != null ? double.parse(json['monto'].toString()) : 0.0,
      fechaPago: json['fechaPago'],
      fechaCreacion: json['fechaCreacion'],
      metodoPago: json['metodoPago'],
      metodoPagoTipo: json['metodoPagoTipo'],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'facturaId': facturaId,
      'reservaId': reservaId,
      'monto': monto,
      'fechaPago': fechaPago,
      'fechaCreacion': fechaCreacion,
      'metodoPago': metodoPago,
      'metodoPagoTipo': metodoPagoTipo,
    };
  }
}
