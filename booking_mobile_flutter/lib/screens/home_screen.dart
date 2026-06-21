import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../api/api_client.dart';
import '../models/alojamiento.dart';
import '../providers/auth_provider.dart';
import '../theme/app_theme.dart';
import 'login_screen.dart';
import 'propiedad_detalle_screen.dart';
import 'mis_reservas_screen.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  final ApiClient _apiClient = ApiClient();
  List<Alojamiento> _alojamientos = [];
  List<dynamic> _ciudades = [];
  bool _isLoading = true;
  String _searchQuery = '';
  String? _selectedCiudadId;
  bool _onlyPetFriendly = false;

  @override
  void initState() {
    super.initState();
    _fetchCiudades();
    _fetchAlojamientos();
  }

  Future<void> _fetchCiudades() async {
    try {
      final response = await _apiClient.dio.get('/alojamientos-alojaexpress/ciudades');
      if (mounted) {
        setState(() {
          _ciudades = response.data['datos'] ?? [];
        });
      }
    } catch (e) {
      print("Error cargando ciudades: $e");
    }
  }

  Future<void> _fetchAlojamientos() async {
    if (!mounted) return;
    setState(() {
      _isLoading = true;
    });

    try {
      final queryParams = <String, dynamic>{};
      if (_selectedCiudadId != null && _selectedCiudadId!.isNotEmpty) {
        queryParams['CiudadId'] = _selectedCiudadId;
      }
      if (_onlyPetFriendly) {
        queryParams['AdmiteMascotas'] = 'true';
      }

      final response = await _apiClient.dio.get(
        '/alojamientos-alojaexpress/buscar',
        queryParameters: queryParams,
      );

      final List<dynamic> items = response.data['datos']?['items'] ?? [];
      if (mounted) {
        setState(() {
          _alojamientos = items.map((json) => Alojamiento.fromJson(json)).toList();
          _isLoading = false;
        });
      }
    } catch (e) {
      print("Error cargando alojamientos: $e");
      if (mounted) {
        setState(() {
          _isLoading = false;
        });
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Error al cargar las propiedades'),
            backgroundColor: AppTheme.danger,
          ),
        );
      }
    }
  }

  List<Alojamiento> get _filteredAlojamientos {
    if (_searchQuery.isEmpty) return _alojamientos;
    return _alojamientos
        .where((p) => p.nombre.toLowerCase().contains(_searchQuery.toLowerCase()))
        .toList();
  }

  Widget _buildStars(int count) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: List.generate(5, (index) {
        return Icon(
          Icons.star,
          size: 14,
          color: index < count ? AppTheme.accent : AppTheme.textMuted.withOpacity(0.3),
        );
      }),
    );
  }

  @override
  Widget build(BuildContext context) {
    final authProvider = Provider.of<AuthProvider>(context);
    final user = authProvider.user;

    return Scaffold(
      appBar: AppBar(
        title: const Text('AlojaExpress'),
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            onPressed: () {
              _fetchCiudades();
              _fetchAlojamientos();
            },
          ),
        ],
      ),
      drawer: Drawer(
        child: Column(
          children: [
            UserAccountsDrawerHeader(
              decoration: const BoxDecoration(
                color: AppTheme.primary,
              ),
              currentAccountPicture: const CircleAvatar(
                backgroundColor: AppTheme.accent,
                child: Text(
                  '✦',
                  style: TextStyle(fontSize: 28, color: Colors.white),
                ),
              ),
              accountName: Text(
                user?['nombreCompleto'] ?? 'Invitado',
                style: const TextStyle(fontWeight: FontWeight.bold),
              ),
              accountEmail: Text(user?['email'] ?? ''),
            ),
            ListTile(
              leading: const Icon(Icons.explore_outlined, color: AppTheme.primary),
              title: const Text('Explorar Alojamientos'),
              onTap: () {
                Navigator.of(context).pop();
              },
            ),
            ListTile(
              leading: const Icon(Icons.book_online_outlined, color: AppTheme.primary),
              title: const Text('Mis Reservas'),
              onTap: () {
                Navigator.of(context).pop();
                Navigator.of(context).push(
                  MaterialPageRoute(builder: (context) => const MisReservasScreen()),
                );
              },
            ),
            const Spacer(),
            const Divider(),
            ListTile(
              leading: const Icon(Icons.logout, color: AppTheme.danger),
              title: const Text('Cerrar Sesión'),
              onTap: () async {
                await authProvider.logout();
                if (context.mounted) {
                  Navigator.of(context).pushReplacement(
                    MaterialPageRoute(builder: (context) => const LoginScreen()),
                  );
                }
              },
            ),
            const SizedBox(height: 20),
          ],
        ),
      ),
      body: Column(
        children: [
          // Buscador y filtros
          Padding(
            padding: const EdgeInsets.all(16.0),
            child: Column(
              children: [
                TextField(
                  onChanged: (val) {
                    setState(() {
                      _searchQuery = val;
                    });
                  },
                  decoration: InputDecoration(
                    hintText: 'Buscar por nombre...',
                    prefixIcon: const Icon(Icons.search, color: AppTheme.textSecondary),
                    filled: true,
                    fillColor: Colors.white,
                    border: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(AppTheme.radiusMd),
                      borderSide: const BorderSide(color: AppTheme.border),
                    ),
                  ),
                ),
                const SizedBox(height: 12),
                Row(
                  children: [
                    Expanded(
                      child: Container(
                        padding: const EdgeInsets.symmetric(horizontal: 12),
                        decoration: BoxDecoration(
                          color: Colors.white,
                          borderRadius: BorderRadius.circular(AppTheme.radiusMd),
                          border: Border.all(color: AppTheme.border),
                        ),
                        child: DropdownButtonHideUnderline(
                          child: DropdownButton<String>(
                            value: _selectedCiudadId,
                            hint: const Text('Cualquier Ciudad'),
                            isExpanded: true,
                            items: [
                              const DropdownMenuItem<String>(
                                value: '',
                                child: Text('Cualquier Ciudad'),
                              ),
                              ..._ciudades.map((c) {
                                return DropdownMenuItem<String>(
                                  value: c['ciudadId'].toString(),
                                  child: Text(c['nombre'] ?? ''),
                                );
                              }),
                            ],
                            onChanged: (val) {
                              setState(() {
                                _selectedCiudadId = val == '' ? null : val;
                              });
                              _fetchAlojamientos();
                            },
                          ),
                        ),
                      ),
                    ),
                    const SizedBox(width: 12),
                    FilterChip(
                      label: const Text('Pet Friendly'),
                      selected: _onlyPetFriendly,
                      onSelected: (val) {
                        setState(() {
                          _onlyPetFriendly = val;
                        });
                        _fetchAlojamientos();
                      },
                      selectedColor: AppTheme.primary.withOpacity(0.1),
                      checkmarkColor: AppTheme.primary,
                    ),
                  ],
                ),
              ],
            ),
          ),

          // Grilla de alojamientos
          Expanded(
            child: RefreshIndicator(
              onRefresh: _fetchAlojamientos,
              child: _isLoading
                  ? GridView.builder(
                      padding: const EdgeInsets.all(16),
                      gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                        crossAxisCount: 2,
                        childAspectRatio: 0.75,
                        crossAxisSpacing: 16,
                        mainAxisSpacing: 16,
                      ),
                      itemCount: 6,
                      itemBuilder: (context, index) {
                        return Container(
                          decoration: BoxDecoration(
                            color: Colors.white,
                            borderRadius: BorderRadius.circular(AppTheme.radiusLg),
                          ),
                          child: const Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Expanded(
                                child: Center(
                                  child: CircularProgressIndicator(),
                                ),
                              ),
                            ],
                          ),
                        );
                      },
                    )
                  : _filteredAlojamientos.isEmpty
                      ? const Center(
                          child: Column(
                            mainAxisAlignment: MainAxisAlignment.center,
                            children: [
                              Icon(Icons.search_off, size: 64, color: AppTheme.textMuted),
                              SizedBox(height: 16),
                              Text(
                                'No se encontraron propiedades',
                                style: TextStyle(
                                  fontSize: 16,
                                  fontWeight: FontWeight.bold,
                                  color: AppTheme.textSecondary,
                                ),
                              ),
                            ],
                          ),
                        )
                      : GridView.builder(
                          padding: const EdgeInsets.all(16),
                          gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                            crossAxisCount: 2,
                            childAspectRatio: 0.73,
                            crossAxisSpacing: 16,
                            mainAxisSpacing: 16,
                          ),
                          itemCount: _filteredAlojamientos.length,
                          itemBuilder: (context, index) {
                            final prop = _filteredAlojamientos[index];
                            return GestureDetector(
                              onTap: () {
                                Navigator.of(context).push(
                                  MaterialPageRoute(
                                    builder: (context) => PropiedadDetalleScreen(
                                      alojamientoId: prop.alojamientoId,
                                    ),
                                  ),
                                );
                              },
                              child: Container(
                                decoration: BoxDecoration(
                                  color: Colors.white,
                                  borderRadius: BorderRadius.circular(AppTheme.radiusLg),
                                  boxShadow: AppTheme.shadowMd,
                                ),
                                child: Column(
                                  crossAxisAlignment: CrossAxisAlignment.stretch,
                                  children: [
                                    // Imagen de propiedad
                                    Expanded(
                                      child: ClipRRect(
                                        borderRadius: const BorderRadius.vertical(
                                          top: Radius.circular(AppTheme.radiusLg),
                                        ),
                                        child: Stack(
                                          fit: StackFit.expand,
                                          children: [
                                            prop.imagenUrl != null
                                                ? Image.network(
                                                    prop.imagenUrl!,
                                                    fit: BoxFit.cover,
                                                    errorBuilder: (context, error, stackTrace) {
                                                      return Container(
                                                        color: AppTheme.border,
                                                        child: const Icon(
                                                          Icons.home_work_outlined,
                                                          color: AppTheme.textMuted,
                                                        ),
                                                      );
                                                    },
                                                  )
                                                : Container(
                                                    color: AppTheme.border,
                                                    child: const Icon(
                                                      Icons.home_work_outlined,
                                                      color: AppTheme.textMuted,
                                                    ),
                                                  ),
                                            if (prop.admiteMascotas)
                                              Positioned(
                                                top: 8,
                                                left: 8,
                                                child: Container(
                                                  padding: const EdgeInsets.symmetric(
                                                    horizontal: 8,
                                                    vertical: 4,
                                                  ),
                                                  decoration: BoxDecoration(
                                                    color: Colors.white.withOpacity(0.9),
                                                    borderRadius: BorderRadius.circular(
                                                      AppTheme.radiusSm,
                                                    ),
                                                  ),
                                                  child: const Row(
                                                    children: [
                                                      Icon(
                                                        Icons.pets,
                                                        size: 10,
                                                        color: AppTheme.primary,
                                                      ),
                                                      SizedBox(width: 4),
                                                      Text(
                                                        'Pet Friendly',
                                                        style: TextStyle(
                                                          fontSize: 8,
                                                          fontWeight: FontWeight.bold,
                                                          color: AppTheme.primary,
                                                        ),
                                                      ),
                                                    ],
                                                  ),
                                                ),
                                              ),
                                          ],
                                        ),
                                      ),
                                    ),

                                    // Información de propiedad
                                    Padding(
                                      padding: const EdgeInsets.all(12.0),
                                      child: Column(
                                        crossAxisAlignment: CrossAxisAlignment.start,
                                        children: [
                                          Text(
                                            prop.nombre,
                                            maxLines: 1,
                                            overflow: TextOverflow.ellipsis,
                                            style: const TextStyle(
                                              fontWeight: FontWeight.bold,
                                              fontSize: 14,
                                              color: AppTheme.text,
                                            ),
                                          ),
                                          const SizedBox(height: 4),
                                          Row(
                                            children: [
                                              const Icon(
                                                Icons.location_on_outlined,
                                                size: 12,
                                                color: AppTheme.textSecondary,
                                              ),
                                              const SizedBox(width: 2),
                                              Expanded(
                                                child: Text(
                                                  prop.ciudad ?? 'N/D',
                                                  maxLines: 1,
                                                  overflow: TextOverflow.ellipsis,
                                                  style: const TextStyle(
                                                    fontSize: 11,
                                                    color: AppTheme.textSecondary,
                                                  ),
                                                ),
                                              ),
                                            ],
                                          ),
                                          const SizedBox(height: 8),
                                          Row(
                                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                            children: [
                                              _buildStars(prop.estrellas),
                                              Text(
                                                '★ ${prop.calificacionPromedio.toStringAsFixed(1)}',
                                                style: const TextStyle(
                                                  fontWeight: FontWeight.bold,
                                                  fontSize: 12,
                                                  color: AppTheme.primary,
                                                ),
                                              ),
                                            ],
                                          ),
                                        ],
                                      ),
                                    ),
                                  ],
                                ),
                              ),
                            );
                          },
                        ),
            ),
          ),
        ],
      ),
    );
  }
}
