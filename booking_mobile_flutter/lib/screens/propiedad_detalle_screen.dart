import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:intl/intl.dart';
import 'package:dio/dio.dart';
import '../api/api_client.dart';
import '../models/alojamiento.dart';
import '../models/habitacion.dart';
import '../providers/auth_provider.dart';
import '../theme/app_theme.dart';
import 'checkout_screen.dart';

class PropiedadDetalleScreen extends StatefulWidget {
  final int alojamientoId;
  const PropiedadDetalleScreen({super.key, required this.alojamientoId});

  @override
  State<PropiedadDetalleScreen> createState() => _PropiedadDetalleScreenState();
}

class _PropiedadDetalleScreenState extends State<PropiedadDetalleScreen> {
  final ApiClient _apiClient = ApiClient();
  Alojamiento? _propiedad;
  List<Habitacion> _habitaciones = [];
  bool _isLoading = true;

  // Formulario
  DateTime? _checkInDate;
  DateTime? _checkOutDate;
  int _numAdultos = 1;
  int _numNinos = 0;
  bool _llevaMascotas = false;
  Habitacion? _selectedHabitacion;
  bool _bookingLoading = false;

  @override
  void initState() {
    super.initState();
    _fetchDetails();
  }

  Future<void> _fetchDetails() async {
    try {
      final responses = await Future.wait([
        _apiClient.dio.get('/alojamientos-alojaexpress/${widget.alojamientoId}'),
        _apiClient.dio.get('/habitaciones-alojaexpress/alojamiento/${widget.alojamientoId}'),
      ]);

      if (mounted) {
        setState(() {
          _propiedad = Alojamiento.fromJson(responses[0].data['datos']);
          final List<dynamic> habs = responses[1].data['datos'] ?? [];
          _habitaciones = habs.map((json) => Habitacion.fromJson(json)).toList();
          _isLoading = false;
        });
      }
    } catch (e) {
      print("Error cargando detalles: $e");
      if (mounted) {
        setState(() {
          _isLoading = false;
        });
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Error al cargar la información de la propiedad'),
            backgroundColor: AppTheme.danger,
          ),
        );
      }
    }
  }

  int get _nights {
    if (_checkInDate == null || _checkOutDate == null) return 0;
    return _checkOutDate!.difference(_checkInDate!).inDays;
  }

  double get _totalPrice {
    if (_selectedHabitacion == null || _nights <= 0) return 0.0;
    return _selectedHabitacion!.precioPorNoche * _nights;
  }

  Future<void> _selectDate(BuildContext context, bool isCheckIn) async {
    final DateTime now = DateTime.now();
    final DateTime initial = isCheckIn 
        ? (_checkInDate ?? now) 
        : (_checkOutDate ?? (_checkInDate?.add(const Duration(days: 1)) ?? now.add(const Duration(days: 1))));
    
    final DateTime first = isCheckIn ? now : (_checkInDate?.add(const Duration(days: 1)) ?? now.add(const Duration(days: 1)));
    final DateTime last = now.add(const Duration(days: 365));

    final DateTime? picked = await showDatePicker(
      context: context,
      initialDate: initial,
      firstDate: first,
      lastDate: last,
      builder: (context, child) {
        return Theme(
          data: Theme.of(context).copyWith(
            colorScheme: const ColorScheme.light(
              primary: AppTheme.primary,
              onPrimary: Colors.white,
              onSurface: AppTheme.text,
            ),
          ),
          child: child!,
        );
      },
    );

    if (picked != null) {
      setState(() {
        if (isCheckIn) {
          _checkInDate = picked;
          if (_checkOutDate != null && !_checkOutDate!.isAfter(_checkInDate!)) {
            _checkOutDate = _checkInDate!.add(const Duration(days: 1));
          }
        } else {
          _checkOutDate = picked;
        }
      });
    }
  }

  void _submitBooking() async {
    final authProvider = Provider.of<AuthProvider>(context, listen: false);
    if (!authProvider.isAuthenticated) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Debes iniciar sesión para realizar una reserva'),
          backgroundColor: AppTheme.warning,
        ),
      );
      return;
    }

    if (_selectedHabitacion == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Por favor selecciona una habitación'),
          backgroundColor: AppTheme.danger,
        ),
      );
      return;
    }

    if (_checkInDate == null || _checkOutDate == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Por favor selecciona fechas válidas'),
          backgroundColor: AppTheme.danger,
        ),
      );
      return;
    }

    setState(() {
      _bookingLoading = true;
    });

    try {
      final df = DateFormat('yyyy-MM-dd');
      final payload = {
        'clienteId': authProvider.user?['clienteId'] ?? int.tryParse(authProvider.user?['id'] ?? ''),
        'habitacionId': _selectedHabitacion!.habitacionId,
        'fechaCheckIn': df.format(_checkInDate!),
        'fechaCheckOut': df.format(_checkOutDate!),
        'numAdultos': _numAdultos,
        'numNinos': _numNinos,
        'llevaMascotas': _llevaMascotas,
      };

      final response = await _apiClient.dio.post(
        '/reservas/booking',
        data: payload,
      );

      if (mounted) {
        final nuevaReserva = response.data['datos'];
        final codigoReserva = nuevaReserva['codigoReserva'];

        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('¡Reserva creada! Completa tu pago.'),
            backgroundColor: AppTheme.success,
          ),
        );

        Navigator.of(context).pushReplacement(
          MaterialPageRoute(
            builder: (context) => CheckoutScreen(codigoReserva: codigoReserva),
          ),
        );
      }
    } catch (e) {
      print("Error creando reserva: $e");
      String errorMsg = "Error al crear la reserva";
      if (e is DioException) {
        final data = e.response?.data;
        errorMsg = data?['mensaje'] ?? data?['message'] ?? errorMsg;
      }
      if (mounted) {
        setState(() {
          _bookingLoading = false;
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

    if (_propiedad == null) {
      return Scaffold(
        appBar: AppBar(),
        body: const Center(child: Text('Propiedad no encontrada')),
      );
    }

    final df = DateFormat('dd/MM/yyyy');

    return Scaffold(
      body: SingleChildScrollView(
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            // Imagen de portada con botón volver
            Stack(
              children: [
                _propiedad!.imagenUrl != null
                    ? Image.network(
                        _propiedad!.imagenUrl!,
                        height: 280,
                        width: double.infinity,
                        fit: BoxFit.cover,
                      )
                    : Container(
                        height: 280,
                        color: AppTheme.border,
                        child: const Icon(Icons.home_work_outlined, size: 64, color: AppTheme.textMuted),
                      ),
                Positioned(
                  top: MediaQuery.of(context).padding.top + 10,
                  left: 16,
                  child: CircleAvatar(
                    backgroundColor: Colors.white.withOpacity(0.9),
                    child: IconButton(
                      icon: const Icon(Icons.arrow_back_ios_new, color: AppTheme.text, size: 18),
                      onPressed: () => Navigator.of(context).pop(),
                    ),
                  ),
                ),
              ],
            ),

            Padding(
              padding: const EdgeInsets.all(20.0),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // Título e info básica
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      Expanded(
                        child: Text(
                          _propiedad!.nombre,
                          style: const TextStyle(
                            fontFamily: 'Playfair Display',
                            fontSize: 24,
                            fontWeight: FontWeight.bold,
                            color: AppTheme.primary,
                          ),
                        ),
                      ),
                      Container(
                        padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                        decoration: BoxDecoration(
                          color: AppTheme.primary.withOpacity(0.1),
                          borderRadius: BorderRadius.circular(AppTheme.radiusSm),
                        ),
                        child: Text(
                          '★ ${_propiedad!.calificacionPromedio.toStringAsFixed(1)}',
                          style: const TextStyle(
                            fontWeight: FontWeight.bold,
                            color: AppTheme.primary,
                          ),
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 8),
                  Row(
                    children: [
                      const Icon(Icons.location_on_outlined, size: 16, color: AppTheme.textSecondary),
                      const SizedBox(width: 4),
                      Text(
                        _propiedad!.direccion ?? _propiedad!.ciudad ?? 'N/D',
                        style: const TextStyle(color: AppTheme.textSecondary),
                      ),
                    ],
                  ),
                  const SizedBox(height: 16),
                  const Divider(),
                  const SizedBox(height: 8),

                  // Descripción
                  const Text(
                    'Acerca de este alojamiento',
                    style: TextStyle(
                      fontSize: 16,
                      fontWeight: FontWeight.bold,
                      color: AppTheme.primary,
                    ),
                  ),
                  const SizedBox(height: 8),
                  Text(
                    _propiedad!.descripcion ?? 'Este exclusivo alojamiento ofrece todas las comodidades para una estadía placentera y relajante.',
                    style: const TextStyle(color: AppTheme.textSecondary, height: 1.5),
                  ),
                  const SizedBox(height: 24),

                  // Selección de Habitaciones
                  const Text(
                    'Habitaciones Disponibles',
                    style: TextStyle(
                      fontSize: 16,
                      fontWeight: FontWeight.bold,
                      color: AppTheme.primary,
                    ),
                  ),
                  const SizedBox(height: 12),
                  _habitaciones.isEmpty
                      ? const Text(
                          'No hay habitaciones registradas o disponibles para este alojamiento.',
                          style: TextStyle(color: AppTheme.textMuted),
                        )
                      : ListView.builder(
                          shrinkWrap: true,
                          physics: const NeverScrollableScrollPhysics(),
                          itemCount: _habitaciones.length,
                          itemBuilder: (context, index) {
                            final hab = _habitaciones[index];
                            final isSelected = _selectedHabitacion?.habitacionId == hab.habitacionId;
                            return Container(
                              margin: const EdgeInsets.only(bottom: 12),
                              decoration: BoxDecoration(
                                color: isSelected ? AppTheme.primary.withOpacity(0.02) : Colors.white,
                                borderRadius: BorderRadius.circular(AppTheme.radiusMd),
                                border: Border.all(
                                  color: isSelected ? AppTheme.primary : AppTheme.border,
                                  width: isSelected ? 1.5 : 1,
                                ),
                              ),
                              child: ListTile(
                                leading: Icon(
                                  Icons.bed_outlined,
                                  color: isSelected ? AppTheme.primary : AppTheme.textSecondary,
                                ),
                                title: Text(
                                  hab.nombre,
                                  style: const TextStyle(fontWeight: FontWeight.bold),
                                ),
                                subtitle: Text(
                                  'Adultos: ${hab.capacidadAdultos} | Niños: ${hab.capacidadNinos}\n${hab.tipoHabitacion ?? 'Estándar'}',
                                  style: const TextStyle(fontSize: 12),
                                ),
                                trailing: Column(
                                  mainAxisAlignment: MainAxisAlignment.center,
                                  crossAxisAlignment: CrossAxisAlignment.end,
                                  children: [
                                    Text(
                                      '\$${hab.precioPorNoche.toStringAsFixed(2)}',
                                      style: const TextStyle(
                                        fontWeight: FontWeight.bold,
                                        color: AppTheme.primary,
                                      ),
                                    ),
                                    const Text('/ noche', style: TextStyle(fontSize: 10, color: AppTheme.textMuted)),
                                  ],
                                ),
                                onTap: () {
                                  setState(() {
                                    _selectedHabitacion = hab;
                                  });
                                },
                              ),
                            );
                          },
                        ),
                  const SizedBox(height: 24),
                  const Divider(),
                  const SizedBox(height: 16),

                  // Formulario de fechas y huéspedes
                  const Text(
                    'Detalles de tu estadía',
                    style: TextStyle(
                      fontSize: 16,
                      fontWeight: FontWeight.bold,
                      color: AppTheme.primary,
                    ),
                  ),
                  const SizedBox(height: 16),
                  Row(
                    children: [
                      Expanded(
                        child: GestureDetector(
                          onTap: () => _selectDate(context, true),
                          child: Container(
                            padding: const EdgeInsets.all(16),
                            decoration: BoxDecoration(
                              color: Colors.white,
                              borderRadius: BorderRadius.circular(AppTheme.radiusMd),
                              border: Border.all(color: AppTheme.border),
                            ),
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                const Text('CHECK-IN', style: TextStyle(fontSize: 10, color: AppTheme.textMuted)),
                                const SizedBox(height: 4),
                                Text(
                                  _checkInDate != null ? df.format(_checkInDate!) : 'Seleccionar',
                                  style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 13),
                                ),
                              ],
                            ),
                          ),
                        ),
                      ),
                      const SizedBox(width: 12),
                      Expanded(
                        child: GestureDetector(
                          onTap: () => _selectDate(context, false),
                          child: Container(
                            padding: const EdgeInsets.all(16),
                            decoration: BoxDecoration(
                              color: Colors.white,
                              borderRadius: BorderRadius.circular(AppTheme.radiusMd),
                              border: Border.all(color: AppTheme.border),
                            ),
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                const Text('CHECK-OUT', style: TextStyle(fontSize: 10, color: AppTheme.textMuted)),
                                const SizedBox(height: 4),
                                Text(
                                  _checkOutDate != null ? df.format(_checkOutDate!) : 'Seleccionar',
                                  style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 13),
                                ),
                              ],
                            ),
                          ),
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 16),

                  // Huéspedes y Mascotas
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      Row(
                        children: [
                          const Text('Adultos: '),
                          IconButton(
                            icon: const Icon(Icons.remove_circle_outline, size: 20),
                            onPressed: _numAdultos > 1 ? () => setState(() => _numAdultos--) : null,
                          ),
                          Text('$_numAdultos'),
                          IconButton(
                            icon: const Icon(Icons.add_circle_outline, size: 20),
                            onPressed: () => setState(() => _numAdultos++),
                          ),
                        ],
                      ),
                      Row(
                        children: [
                          const Text('Niños: '),
                          IconButton(
                            icon: const Icon(Icons.remove_circle_outline, size: 20),
                            onPressed: _numNinos > 0 ? () => setState(() => _numNinos--) : null,
                          ),
                          Text('$_numNinos'),
                          IconButton(
                            icon: const Icon(Icons.add_circle_outline, size: 20),
                            onPressed: () => setState(() => _numNinos++),
                          ),
                        ],
                      ),
                    ],
                  ),
                  const SizedBox(height: 8),
                  CheckboxListTile(
                    title: const Text('Llevo mascotas', style: TextStyle(fontSize: 14)),
                    value: _llevaMascotas,
                    onChanged: (val) {
                      setState(() {
                        _llevaMascotas = val ?? false;
                      });
                    },
                    controlAffinity: ListTileControlAffinity.leading,
                    contentPadding: EdgeInsets.zero,
                    activeColor: AppTheme.primary,
                  ),
                  const SizedBox(height: 24),

                  // Resumen de precios
                  if (_selectedHabitacion != null && _nights > 0) ...[
                    Container(
                      padding: const EdgeInsets.all(16),
                      decoration: BoxDecoration(
                        color: AppTheme.background,
                        borderRadius: BorderRadius.circular(AppTheme.radiusMd),
                      ),
                      child: Column(
                        children: [
                          Row(
                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                            children: [
                              Text('\$${_selectedHabitacion!.precioPorNoche.toStringAsFixed(2)} x $_nights noches'),
                              Text('\$${_totalPrice.toStringAsFixed(2)}'),
                            ],
                          ),
                          const Divider(height: 20),
                          Row(
                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                            children: [
                              const Text('Total Estimado (sin IVA):', style: TextStyle(fontWeight: FontWeight.bold)),
                              Text(
                                '\$${_totalPrice.toStringAsFixed(2)} USD',
                                style: const TextStyle(
                                  fontWeight: FontWeight.bold,
                                  color: AppTheme.primary,
                                  fontSize: 16,
                                ),
                              ),
                            ],
                          ),
                        ],
                      ),
                    ),
                    const SizedBox(height: 24),
                  ],

                  // Botón Reservar
                  ElevatedButton(
                    onPressed: _bookingLoading ? null : _submitBooking,
                    style: ElevatedButton.styleFrom(
                      minimumSize: const Size(double.infinity, 54),
                    ),
                    child: _bookingLoading
                        ? const CircularProgressIndicator(color: Colors.white)
                        : const Text('Reservar Ahora'),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}
