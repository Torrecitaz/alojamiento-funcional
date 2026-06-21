import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:intl/intl.dart';
import 'package:dio/dio.dart';
import '../api/api_client.dart';
import '../models/reserva.dart';
import '../providers/auth_provider.dart';
import '../theme/app_theme.dart';
import 'checkout_screen.dart';
import 'factura_screen.dart';

class MisReservasScreen extends StatefulWidget {
  const MisReservasScreen({super.key});

  @override
  State<MisReservasScreen> createState() => _MisReservasScreenState();
}

class _MisReservasScreenState extends State<MisReservasScreen> {
  final ApiClient _apiClient = ApiClient();
  List<Reserva> _reservas = [];
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _fetchReservas();
  }

  Future<void> _fetchReservas() async {
    final authProvider = Provider.of<AuthProvider>(context, listen: false);
    final clienteId = authProvider.user?['clienteId'];

    if (clienteId == null) {
      setState(() {
        _isLoading = false;
      });
      return;
    }

    try {
      final response = await _apiClient.dio.get('/reservas-alojaexpress/cliente/$clienteId');
      final List<dynamic> list = response.data['datos'] ?? response.data ?? [];
      
      if (mounted) {
        setState(() {
          _reservas = list.map((json) => Reserva.fromJson(json)).toList();
          _isLoading = false;
        });
      }
    } catch (e) {
      print("Error cargando reservas: $e");
      if (mounted) {
        setState(() {
          _isLoading = false;
        });
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Error al cargar tus reservas'),
            backgroundColor: AppTheme.danger,
          ),
        );
      }
    }
  }

  void _cancelarReserva(int reservaId) async {
    // Confirmar antes de cancelar
    final confirm = await showDialog<bool>(
      context: context,
      builder: (context) {
        return AlertDialog(
          title: const Text('Cancelar Reservación'),
          content: const Text('¿Estás seguro de que deseas cancelar esta reservación? Esta acción no se puede deshacer.'),
          actions: [
            TextButton(
              onPressed: () => Navigator.of(context).pop(false),
              child: const Text('Volver', style: TextStyle(color: AppTheme.textSecondary)),
            ),
            TextButton(
              onPressed: () => Navigator.of(context).pop(true),
              child: const Text('Sí, Cancelar', style: TextStyle(color: AppTheme.danger)),
            ),
          ],
        );
      },
    );

    if (confirm != true) return;

    setState(() {
      _isLoading = true;
    });

    try {
      await _apiClient.dio.patch('/reservas-alojaexpress/$reservaId/cancelar');
      
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Reservación cancelada exitosamente.'),
            backgroundColor: AppTheme.success,
          ),
        );
        _fetchReservas();
      }
    } catch (e) {
      print("Error cancelando reserva: $e");
      String errorMsg = "No se pudo cancelar la reservación";
      if (e is DioException) {
        final data = e.response?.data;
        errorMsg = data?['mensaje'] ?? data?['message'] ?? errorMsg;
      }
      if (mounted) {
        setState(() {
          _isLoading = false;
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

  Color _getStatusColor(String status) {
    switch (status.toLowerCase()) {
      case 'confirmada':
        return AppTheme.success;
      case 'pendiente':
        return AppTheme.warning;
      case 'cancelada':
      default:
        return AppTheme.danger;
    }
  }

  @override
  Widget build(BuildContext context) {
    final df = DateFormat('dd/MM/yyyy');

    return Scaffold(
      appBar: AppBar(
        title: const Text('Mis Reservas'),
        leading: IconButton(
          icon: const Icon(Icons.arrow_back_ios_new, size: 20),
          onPressed: () => Navigator.of(context).pop(),
        ),
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : _reservas.isEmpty
              ? const Center(
                  child: Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      Icon(Icons.bookmark_border_outlined, size: 64, color: AppTheme.textMuted),
                      SizedBox(height: 16),
                      Text(
                        'Aún no tienes reservaciones',
                        style: TextStyle(
                          fontSize: 16,
                          fontWeight: FontWeight.bold,
                          color: AppTheme.textSecondary,
                        ),
                      ),
                    ],
                  ),
                )
              : ListView.builder(
                  padding: const EdgeInsets.all(16),
                  itemCount: _reservas.length,
                  itemBuilder: (context, index) {
                    final res = _reservas[index];
                    final DateTime checkIn = DateTime.parse(res.fechaCheckIn);
                    final DateTime checkOut = DateTime.parse(res.fechaCheckOut);
                    final int noches = checkOut.difference(checkIn).inDays;

                    return Container(
                      margin: const EdgeInsets.only(bottom: 16),
                      decoration: BoxDecoration(
                        color: Colors.white,
                        borderRadius: BorderRadius.circular(AppTheme.radiusLg),
                        boxShadow: AppTheme.shadowMd,
                        border: Border.all(color: AppTheme.border),
                      ),
                      child: Padding(
                        padding: const EdgeInsets.all(16.0),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Row(
                              mainAxisAlignment: MainAxisAlignment.spaceBetween,
                              children: [
                                Expanded(
                                  child: Text(
                                    res.nombrePropiedad ?? 'Alojamiento',
                                    style: const TextStyle(
                                      fontWeight: FontWeight.bold,
                                      fontSize: 16,
                                      color: AppTheme.primary,
                                    ),
                                  ),
                                ),
                                Container(
                                  padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                                  decoration: BoxDecoration(
                                    color: _getStatusColor(res.estado).withOpacity(0.1),
                                    borderRadius: BorderRadius.circular(AppTheme.radiusSm),
                                  ),
                                  child: Text(
                                    res.estado.toUpperCase(),
                                    style: TextStyle(
                                      color: _getStatusColor(res.estado),
                                      fontWeight: FontWeight.bold,
                                      fontSize: 10,
                                    ),
                                  ),
                                ),
                              ],
                            ),
                            const SizedBox(height: 12),
                            if (res.nombreHabitacion != null) ...[
                              Text(
                                'Habitación: ${res.nombreHabitacion}',
                                style: const TextStyle(fontSize: 13, color: AppTheme.textSecondary),
                              ),
                              const SizedBox(height: 6),
                            ],
                            Text(
                              'Fechas: ${df.format(checkIn)} al ${df.format(checkOut)} ($noches noches)',
                              style: const TextStyle(fontSize: 13, color: AppTheme.textSecondary),
                            ),
                            const SizedBox(height: 6),
                            Text(
                              'Código: ${res.codigoReserva}',
                              style: const TextStyle(fontSize: 13, color: AppTheme.textSecondary),
                            ),
                            const SizedBox(height: 12),
                            Row(
                              mainAxisAlignment: MainAxisAlignment.spaceBetween,
                              children: [
                                Text(
                                  'Total: \$${res.total.toStringAsFixed(2)} USD',
                                  style: const TextStyle(
                                    fontWeight: FontWeight.bold,
                                    color: AppTheme.primary,
                                  ),
                                ),
                                Row(
                                  children: [
                                    if (res.estado.toLowerCase() == 'pendiente') ...[
                                      TextButton(
                                        onPressed: () => _cancelarReserva(res.reservaId),
                                        child: const Text('Cancelar', style: TextStyle(color: AppTheme.danger, fontSize: 13)),
                                      ),
                                      const SizedBox(width: 8),
                                      ElevatedButton(
                                        onPressed: () {
                                          Navigator.of(context).push(
                                            MaterialPageRoute(
                                              builder: (context) => CheckoutScreen(codigoReserva: res.codigoReserva),
                                            ),
                                          );
                                        },
                                        style: ElevatedButton.styleFrom(
                                          backgroundColor: AppTheme.primary,
                                          padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                                          minimumSize: Size.zero,
                                        ),
                                        child: const Text('Pagar', style: TextStyle(fontSize: 13)),
                                      ),
                                    ],
                                    if (res.estado.toLowerCase() == 'confirmada') ...[
                                      ElevatedButton(
                                        onPressed: () {
                                          Navigator.of(context).push(
                                            MaterialPageRoute(
                                              builder: (context) => FacturaScreen(codigoReserva: res.codigoReserva),
                                            ),
                                          );
                                        },
                                        style: ElevatedButton.styleFrom(
                                          backgroundColor: AppTheme.accent,
                                          foregroundColor: Colors.white,
                                          padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                                          minimumSize: Size.zero,
                                        ),
                                        child: const Text('Ver Recibo', style: TextStyle(fontSize: 13, fontWeight: FontWeight.bold)),
                                      ),
                                    ],
                                  ],
                                ),
                              ],
                            ),
                          ],
                        ),
                      ),
                    );
                  },
                ),
    );
  }
}
