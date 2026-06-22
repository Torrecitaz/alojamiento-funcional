import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:jwt_decoder/jwt_decoder.dart';
import 'package:dio/dio.dart';
import '../api/api_client.dart';

class AuthProvider extends ChangeNotifier {
  final FlutterSecureStorage _storage = const FlutterSecureStorage();
  final ApiClient _apiClient = ApiClient();

  String? _token;
  Map<String, dynamic>? _user;
  bool _isAuthenticated = false;
  bool _isLoading = false;

  String? get token => _token;
  Map<String, dynamic>? get user => _user;
  bool get isAuthenticated => _isAuthenticated;
  bool get isLoading => _isLoading;

  Future<void> tryAutoLogin() async {
    _token = await _storage.read(key: 'alojaexpress_token');
    if (_token != null) {
      if (JwtDecoder.isExpired(_token!)) {
        await logout();
        return;
      }
      String? userStr = await _storage.read(key: 'alojaexpress_user');
      if (userStr != null) {
        _user = jsonDecode(userStr);
        _isAuthenticated = true;
      } else {
        await logout();
      }
    }
    notifyListeners();
  }

  Future<String?> login(String email, String password) async {
    _isLoading = true;
    notifyListeners();
    try {
      final response = await _apiClient.dio.post('/auth-alojaexpress/login', data: {
        'email': email,
        'password': password,
      });

      if (response.statusCode == 200 || response.statusCode == 201) {
        final rawData = response.data;
        final Map<String, dynamic> data = (rawData != null && rawData is Map && rawData.containsKey('datos'))
            ? Map<String, dynamic>.from(rawData['datos'])
            : Map<String, dynamic>.from(rawData);

        final token = data['token'] as String;

        Map<String, dynamic> decoded = JwtDecoder.decode(token);
        String userId = decoded['sub'] ?? '';

        int? clienteId = data['clienteId'] != null ? int.tryParse(data['clienteId'].toString()) : null;
        int? colaboradorId = data['colaboradorId'] != null ? int.tryParse(data['colaboradorId'].toString()) : null;
        String emailRes = data['email'] ?? email;
        String nombreCompleto = data['nombreCompleto'] ?? '';
        List<dynamic> roles = data['roles'] ?? [];

        // Fallback: si clienteId es null, buscar el clienteId por email
        if (clienteId == null && emailRes.isNotEmpty) {
          try {
            final clientResponse = await _apiClient.dio.get('/clientes-alojaexpress', queryParameters: {
              'page': 1,
              'pageSize': 200,
            });
            final listData = clientResponse.data;
            final clients = listData['datos'] ?? listData['items'] ?? [];
            final matchingClient = clients.firstWhere(
              (c) => c['email']?.toString().toLowerCase() == emailRes.toLowerCase(),
              orElse: () => null,
            );
            if (matchingClient != null) {
              clienteId = int.tryParse(matchingClient['clienteId'].toString());
            }
          } catch (e) {
            print("Error resolviendo clienteId por email en móvil: $e");
          }
        }

        _user = {
          'id': userId,
          'clienteId': clienteId,
          'colaboradorId': colaboradorId,
          'nombreCompleto': nombreCompleto,
          'email': emailRes,
          'roles': roles,
        };

        _token = token;
        _isAuthenticated = true;

        await _storage.write(key: 'alojaexpress_token', value: token);
        await _storage.write(key: 'alojaexpress_user', value: jsonEncode(_user));

        _isLoading = false;
        notifyListeners();
        return null; // Éxito, sin mensaje de error
      }
    } catch (e) {
      print("Error en login móvil: $e");
      String errorMsg = "Error al iniciar sesión.";
      if (e is DioException) {
        final data = e.response?.data;
        errorMsg = data?['mensaje'] ?? data?['message'] ?? errorMsg;
      }
      _isLoading = false;
      notifyListeners();
      return errorMsg;
    }
    _isLoading = false;
    notifyListeners();
    return "Error al iniciar sesión.";
  }

  Future<String?> register({
    required String nombreCompleto,
    required String email,
    required String password,
    required String telefono,
  }) async {
    _isLoading = true;
    notifyListeners();
    try {
      final registerResponse = await _apiClient.dio.post('/clientes-alojaexpress/registrar', data: {
        'nombreCompleto': nombreCompleto,
        'email': email,
        'password': password,
        'telefono': telefono.isEmpty ? null : telefono,
      });

      if (registerResponse.statusCode == 200 || registerResponse.statusCode == 201) {
        _isLoading = false;
        notifyListeners();
        return null; // Éxito
      }
    } catch (e) {
      print("Error en registro móvil: $e");
      String errorMsg = "Error al registrar la cuenta.";
      if (e is DioException) {
        final data = e.response?.data;
        final errors = data?['errores'] ?? data?['errors'];
        if (errors != null && errors is List) {
          errorMsg = (errors as List).join('\n');
        } else {
          errorMsg = data?['mensaje'] ?? data?['message'] ?? errorMsg;
        }
      }
      _isLoading = false;
      notifyListeners();
      return errorMsg;
    }
    _isLoading = false;
    notifyListeners();
    return "Error al registrar la cuenta.";
  }

  Future<void> logout() async {
    _token = null;
    _user = null;
    _isAuthenticated = false;
    await _storage.delete(key: 'alojaexpress_token');
    await _storage.delete(key: 'alojaexpress_user');
    notifyListeners();
  }
}
