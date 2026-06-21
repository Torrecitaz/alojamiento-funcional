import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:dio/dio.dart';
import '../api/api_client.dart';
import '../models/reserva.dart';
import '../theme/app_theme.dart';
import 'factura_screen.dart';
import 'mis_reservas_screen.dart';

class CheckoutScreen extends StatefulWidget {
  final String codigoReserva;
  const CheckoutScreen({super.key, required this.codigoReserva});

  @override
  State<CheckoutScreen> createState() => _CheckoutScreenState();
}

class _CheckoutScreenState extends State<CheckoutScreen> {
  final ApiClient _apiClient = ApiClient();
  Reserva? _reserva;
  bool _isLoading = true;
  bool _processing = false;
  String _metodoPago = 'Tarjeta'; // 'Tarjeta' o 'EnSitio'

  @override
  void initState() {
    super.initState();
    _fetchReserva();
  }

  Future<void> _fetchReserva() async {
    try {
      final response = await _apiClient.dio.get('/reservas-alojaexpress/${widget.codigoReserva}');
      if (mounted) {
        final res = Reserva.fromJson(response.data['datos']);
        setState(() {
          _reserva = res;
          _isLoading = false;
        });

        // Si ya está confirmada, mandar directo a la factura
        if (res.estado == 'Confirmada') {
          Navigator.of(context).pushReplacement(
            MaterialPageRoute(
              builder: (context) => FacturaScreen(codigoReserva: widget.codigoReserva),
            ),
          );
        }
      }
    } catch (e) {
      print("Error cargando reserva: $e");
      if (mounted) {
        setState(() {
          _isLoading = false;
        });
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Error al cargar la información de la reserva'),
            backgroundColor: AppTheme.danger,
          ),
        );
        Navigator.of(context).pushReplacement(
          MaterialPageRoute(builder: (context) => const MisReservasScreen()),
        );
      }
    }
  }

  void _processPayment() async {
    setState(() {
      _processing = true;
    });

    try {
      final String metodoPagoId = _metodoPago == 'Tarjeta'
          ? '22222222-2222-2222-2222-222222222222' // Tarjeta de Crédito/Débito
          : '33333333-3333-3333-3333-333333333333'; // Pago en Sitio

      final payload = {
        'idCarrito': widget.codigoReserva,
        'metodoPagoId': metodoPagoId,
        'currency': 'USD',
      };

      await _apiClient.dio.post(
        '/reservas-alojaexpress/checkout',
        data: payload,
      );

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('¡Pago procesado exitosamente!'),
            backgroundColor: AppTheme.success,
          ),
        );
        Navigator.of(context).pushReplacement(
          MaterialPageRoute(
            builder: (context) => FacturaScreen(codigoReserva: widget.codigoReserva),
          ),
        );
      }
    } catch (e) {
      print("Error al procesar pago: $e");
      String errorMsg = "El pago ha fallado";
      if (e is DioException) {
        final data = e.response?.data;
        errorMsg = data?['mensaje'] ?? data?['message'] ?? errorMsg;
      }
      if (mounted) {
        setState(() {
          _processing = false;
        });
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(errorMsg),
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

    return Scaffold(
      appBar: AppBar(
        title: const Text('Checkout'),
        leading: IconButton(
          icon: const Icon(Icons.arrow_back_ios_new, size: 20),
          onPressed: () => Navigator.of(context).pushReplacement(
            MaterialPageRoute(builder: (context) => const MisReservasScreen()),
          ),
        ),
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            // Resumen de la reserva
            Container(
              padding: const EdgeInsets.all(20),
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(AppTheme.radiusLg),
                boxShadow: AppTheme.shadowMd,
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      const Icon(Icons.shield_outlined, color: AppTheme.accent, size: 24),
                      const SizedBox(width: 8),
                      Text(
                        'Código: ${_reserva!.codigoReserva}',
                        style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16),
                      ),
                    ],
                  ),
                  const SizedBox(height: 16),
                  const Divider(),
                  const SizedBox(height: 8),
                  Text(
                    _reserva!.nombrePropiedad ?? 'Alojamiento',
                    style: const TextStyle(
                      fontSize: 18,
                      fontWeight: FontWeight.bold,
                      color: AppTheme.primary,
                    ),
                  ),
                  const SizedBox(height: 8),
                  if (_reserva!.nombreHabitacion != null) ...[
                    Text(
                      'Habitación: ${_reserva!.nombreHabitacion}',
                      style: const TextStyle(color: AppTheme.textSecondary),
                    ),
                    const SizedBox(height: 12),
                  ],
                  Row(
                    children: [
                      const Icon(Icons.calendar_today_outlined, size: 16, color: AppTheme.textSecondary),
                      const SizedBox(width: 6),
                      Text(
                        '${df.format(checkIn)} al ${df.format(checkOut)} ($noches noches)',
                        style: const TextStyle(fontSize: 13, color: AppTheme.textSecondary),
                      ),
                    ],
                  ),
                  const SizedBox(height: 8),
                  Row(
                    children: [
                      const Icon(Icons.people_outline, size: 16, color: AppTheme.textSecondary),
                      const SizedBox(width: 6),
                      Text(
                        '${_reserva!.numAdultos} adultos, ${_reserva!.numNinos} niños',
                        style: const TextStyle(fontSize: 13, color: AppTheme.textSecondary),
                      ),
                    ],
                  ),
                  const SizedBox(height: 8),
                  Row(
                    children: [
                      const Icon(Icons.pets_outlined, size: 16, color: AppTheme.textSecondary),
                      const SizedBox(width: 6),
                      Text(
                        _reserva!.llevaMascotas ? 'Lleva mascotas' : 'No lleva mascotas',
                        style: const TextStyle(fontSize: 13, color: AppTheme.textSecondary),
                      ),
                    ],
                  ),
                ],
              ),
            ),
            const SizedBox(height: 24),

            // Métodos de Pago
            const Text(
              'Método de Pago',
              style: TextStyle(
                fontSize: 16,
                fontWeight: FontWeight.bold,
                color: AppTheme.primary,
              ),
            ),
            const SizedBox(height: 12),
            Container(
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(AppTheme.radiusMd),
                border: Border.all(color: AppTheme.border),
              ),
              child: Column(
                children: [
                  RadioListTile<String>(
                    title: const Text('Tarjeta de Crédito/Débito', style: TextStyle(fontWeight: FontWeight.w500)),
                    secondary: const Icon(Icons.credit_card, color: AppTheme.primary),
                    value: 'Tarjeta',
                    groupValue: _metodoPago,
                    activeColor: AppTheme.primary,
                    onChanged: (val) {
                      setState(() {
                        _metodoPago = val!;
                      });
                    },
                  ),
                  const Divider(height: 1),
                  RadioListTile<String>(
                    title: const Text('Pago en Sitio (Check-in)', style: TextStyle(fontWeight: FontWeight.w500)),
                    secondary: const Icon(Icons.payments_outlined, color: AppTheme.primary),
                    value: 'EnSitio',
                    groupValue: _metodoPago,
                    activeColor: AppTheme.primary,
                    onChanged: (val) {
                      setState(() {
                        _metodoPago = val!;
                      });
                    },
                  ),
                ],
              ),
            ),
            const SizedBox(height: 24),

            // Desglose de Precios
            Container(
              padding: const EdgeInsets.all(20),
              decoration: BoxDecoration(
                color: AppTheme.background,
                borderRadius: BorderRadius.circular(AppTheme.radiusLg),
                border: Border.all(color: AppTheme.border),
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      const Text('Subtotal (Estadía)', style: TextStyle(color: AppTheme.textSecondary)),
                      Text('\$${(_reserva!.total / 1.15).toStringAsFixed(2)} USD'),
                    ],
                  ),
                  const SizedBox(height: 8),
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      const Text('Impuestos (IVA 15%)', style: TextStyle(color: AppTheme.textSecondary)),
                      Text('\$${(_reserva!.total - (_reserva!.total / 1.15)).toStringAsFixed(2)} USD'),
                    ],
                  ),
                  const Divider(height: 24),
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      const Text(
                        'Total a Pagar:',
                        style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16),
                      ),
                      Text(
                        '\$${_reserva!.total.toStringAsFixed(2)} USD',
                        style: const TextStyle(
                          fontWeight: FontWeight.bold,
                          color: AppTheme.primary,
                          fontSize: 20,
                        ),
                      ),
                    ],
                  ),
                ],
              ),
            ),
            const SizedBox(height: 32),

            // Botón Pagar
            ElevatedButton(
              onPressed: _processing ? null : _processPayment,
              style: ElevatedButton.styleFrom(
                minimumSize: const Size(double.infinity, 54),
              ),
              child: _processing
                  ? const CircularProgressIndicator(color: Colors.white)
                  : Text(_metodoPago == 'Tarjeta' ? 'Pagar Reservación' : 'Confirmar Reservación'),
            ),
          ],
        ),
      ),
    );
  }
}
