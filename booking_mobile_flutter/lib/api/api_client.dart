import 'package:dio/dio.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:uuid/uuid.dart';

class ApiClient {
  final Dio dio = Dio(BaseOptions(
    baseUrl: 'https://api-gateway-y75a.onrender.com/api/v2',
    connectTimeout: const Duration(seconds: 45),
    receiveTimeout: const Duration(seconds: 45),
  ));

  final FlutterSecureStorage _storage = const FlutterSecureStorage();
  final Uuid _uuid = const Uuid();

  // Instancia única (Singleton)
  static final ApiClient _instance = ApiClient._internal();
  factory ApiClient() => _instance;

  ApiClient._internal() {
    dio.interceptors.add(InterceptorsWrapper(
      onRequest: (options, handler) async {
        // 1. Inyectar JWT Token desde almacenamiento seguro
        String? token = await _storage.read(key: 'alojaexpress_token');
        if (token != null && token.isNotEmpty) {
          options.headers['Authorization'] = 'Bearer $token';
        }

        // 2. Inyectar llave de Idempotencia en peticiones POST
        if (options.method.toUpperCase() == 'POST') {
          final hasIdempotency = options.headers.containsKey('X-Idempotency-Key') ||
              options.headers.containsKey('Idempotency-Key');
          if (!hasIdempotency) {
            String key = _uuid.v4();
            options.headers['X-Idempotency-Key'] = key;
            options.headers['Idempotency-Key'] = key;
          }
        }

        return handler.next(options);
      },
      onError: (DioException error, handler) async {
        // 3. Manejo centralizado de expiración de sesión (401)
        if (error.response?.statusCode == 401) {
          await _storage.delete(key: 'alojaexpress_token');
          await _storage.delete(key: 'alojaexpress_user');
          // Aquí se puede propagar la redirección o limpiar el estado
        }
        return handler.next(error);
      },
    ));
  }
}
