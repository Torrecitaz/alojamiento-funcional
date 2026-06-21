import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:dio/dio.dart';
import '../api/api_client.dart';
import '../models/reserva.dart';
import '../models/factura.dart';
import '../theme/app_theme.dart';
import 'home_screen.dart';

class FacturaScreen extends StatefulWidget {
  final String codigoReserva;
  const FacturaScreen({super.key, required this.codigoReserva});

  @override
  State<FacturaScreen> createState() => _FacturaScreenState();
}

class _FacturaScreenState extends State<FacturaScreen> {
  final ApiClient _apiClient = ApiClient();
  Reserva? _reserva;
  Factura? _factura;
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _fetchFacturaData();
  }

  Future<void> _fetchFacturaData() async {
    try {
      // 1. Obtener la reserva por código
      final resResponse = await _apiClient.dio.get('/reservas-alojaexpress/${widget.codigoReserva}');
      final reservaData = Reserva.fromJson(resResponse.data['datos']);
      
      if (mounted) {
        setState(() {
          _reserva = reservaData;
        });
      }

      // 2. Obtener la factura por reservaId
      try {
        final factResponse = await _apiClient.dio.get('/facturas-alojaexpress/reserva/${reservaData.reservaId}');
        final data = factResponse.data;
        if (data != null && (data['success'] == true || data['datos'] != null)) {
          final factData = Factura.fromJson(data['datos']);
          if (mounted) {
            setState(() {
              _factura = factData;
            });
          }
        }
      } catch (e) {
        print("Factura no encontrada o error en la petición: $e");
      }

      if (mounted) {
        setState(() {
          _isLoading = false;
        });
      }
    } catch (e) {
      print("Error cargando factura completa: $e");
      if (mounted) {
        setState(() {
          _isLoading = false;
        });
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Error al cargar la información de facturación'),
            backgroundColor: AppTheme.danger,
          ),
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_isLoading) {
      return const Scaffold(
        body: Center(child: CircularProgressIndicator()),
      );
    }

    if (_reserva == null) {
      return const Scaffold(
        body: Center(child: Text('Reserva no encontrada')),
      );
    }

    final df = DateFormat('dd/MM/yyyy');
    final DateTime checkIn = DateTime.parse(_reserva!.fechaCheckIn);
    final DateTime checkOut = DateTime.parse(_reserva!.fechaCheckOut);
    final int noches = checkOut.difference(checkIn).inDays;

    // Workaround para bug del monto en 0 de la factura
    final double totalFinal = (_factura != null && _factura!.monto > 0)
        ? _factura!.monto
        : _reserva!.total;

    final double subtotal = totalFinal / 1.15;
    final double iva = totalFinal - subtotal;

    return Scaffold(
      appBar: AppBar(
        title: const Text('Recibo de Reserva'),
        leading: IconButton(
          icon: const Icon(Icons.arrow_back_ios_new, size: 20),
          onPressed: () => Navigator.of(context).pushAndRemoveUntil(
            MaterialPageRoute(builder: (context) => const HomeScreen()),
            (route) => false,
          ),
        ),
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            // Diseño de la factura / recibo oficial
            Container(
              padding: const EdgeInsets.all(24),
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(AppTheme.radiusLg),
                border: const Border(
                  top: BorderSide(color: AppTheme.primary, width: 6),
                ),
                boxShadow: AppTheme.shadowMd,
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // Encabezado
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      const Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            'AlojaExpress',
                            style: TextStyle(
                              fontSize: 20,
                              fontWeight: FontWeight.bold,
                              color: AppTheme.primary,
                            ),
                          ),
                          Text(
                            'Factura de Hospedaje',
                            style: TextStyle(fontSize: 10, color: AppTheme.textSecondary),
                          ),
                        ],
                      ),
                      Column(
                        crossAxisAlignment: CrossAxisAlignment.end,
                        children: [
                          Text(
                            'RECIBO #${_reserva!.codigoReserva}',
                            style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 13),
                          ),
                          Text(
                            'Emisión: ${df.format(DateTime.now())}',
                            style: const TextStyle(fontSize: 10, color: AppTheme.textSecondary),
                          ),
                        ],
                      ),
                    ],
                  ),
                  const SizedBox(height: 20),
                  const Divider(),
                  const SizedBox(height: 12),

                  // Datos del Cliente
                  const Text(
                    'DATOS DEL HUESPED',
                    style: TextStyle(fontSize: 11, fontWeight: FontWeight.bold, color: AppTheme.textSecondary),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    _reserva!.nombreCliente ?? 'N/D',
                    style: const TextStyle(fontSize: 14, fontWeight: FontWeight.w500),
                  ),
                  const SizedBox(height: 20),

                  // Datos de la Propiedad
                  const Text(
                    'DETALLE DEL ALOJAMIENTO',
                    style: TextStyle(fontSize: 11, fontWeight: FontWeight.bold, color: AppTheme.textSecondary),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    _reserva!.nombrePropiedad ?? 'Alojamiento',
                    style: const TextStyle(fontSize: 14, fontWeight: FontWeight.w500, color: AppTheme.primary),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    'Estadía: ${df.format(checkIn)} al ${df.format(checkOut)} ($noches noches)',
                    style: const TextStyle(fontSize: 12, color: AppTheme.textSecondary),
                  ),
                  Text(
                    'Huéspedes: ${_reserva!.numAdultos} adultos, ${_reserva!.numNinos} niños',
                    style: const TextStyle(fontSize: 12, color: AppTheme.textSecondary),
                  ),
                  const SizedBox(height: 24),

                  // Tabla de desglose
                  Container(
                    color: AppTheme.background,
                    padding: const EdgeInsets.all(12),
                    child: const Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Text('Descripción', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 12)),
                        Text('Total', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 12)),
                      ],
                    ),
                  ),
                  Padding(
                    padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 16),
                    child: Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Expanded(
                          child: Text(
                            'Estadía en ${_reserva!.nombrePropiedad}',
                            style: const TextStyle(fontSize: 13),
                          ),
                        ),
                        Text('\$${subtotal.toStringAsFixed(2)}'),
                      ],
                    ),
                  ),
                  Padding(
                    padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
                    child: Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        const Text('Impuestos (IVA 15%)', style: TextStyle(fontSize: 13)),
                        Text('\$${iva.toStringAsFixed(2)}'),
                      ],
                    ),
                  ),
                  const SizedBox(height: 12),
                  const Divider(),
                  Padding(
                    padding: const EdgeInsets.all(12.0),
                    child: Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        const Text(
                          'TOTAL PAGADO:',
                          style: TextStyle(fontWeight: FontWeight.bold, fontSize: 14),
                        ),
                        Text(
                          '\$${totalFinal.toStringAsFixed(2)} USD',
                          style: const TextStyle(
                            fontWeight: FontWeight.bold,
                            color: AppTheme.success,
                            fontSize: 16,
                          ),
                        ),
                      ],
                    ),
                  ),

                  // Historial de transacciones
                  if (_factura != null) ...[
                    const SizedBox(height: 24),
                    const Text(
                      'HISTORIAL DE TRANSACCIONES',
                      style: TextStyle(fontSize: 11, fontWeight: FontWeight.bold, color: AppTheme.textSecondary),
                    ),
                    const SizedBox(height: 8),
                    Row(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        const Icon(Icons.check_circle_outline, color: AppTheme.success, size: 16),
                        const SizedBox(width: 6),
                        Expanded(
                          child: Text(
                            'Transacción autorizada con éxito.\nReferencia: FAC-${_factura!.facturaId}\nMétodo: ${_factura!.metodoPagoTipo ?? _factura!.metodoPago ?? 'Tarjeta'}\nMonto: \$${totalFinal.toStringAsFixed(2)} USD',
                            style: const TextStyle(fontSize: 11, color: AppTheme.textSecondary, height: 1.4),
                          ),
                        ),
                      ],
                    ),
                  ],
                ],
              ),
            ),
            const SizedBox(height: 32),

            // Botón Volver al inicio
            ElevatedButton(
              onPressed: () => Navigator.of(context).pushAndRemoveUntil(
                MaterialPageRoute(builder: (context) => const HomeScreen()),
                (route) => false,
              ),
              style: ElevatedButton.styleFrom(
                minimumSize: const Size(double.infinity, 54),
              ),
              child: const Text('Volver a la Página de Inicio'),
            ),
          ],
        ),
      ),
    );
  }
}
