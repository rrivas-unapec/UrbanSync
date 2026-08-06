import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/network/api_exception.dart';
import '../../../core/network/dio_client.dart';
import '../domain/app_user.dart';

final authRepositoryProvider = Provider<AuthRepository>(
  (ref) => AuthRepository(ref.read(dioProvider)),
);

class AuthResult {
  const AuthResult({
    required this.token,
    required this.expiresAtUtc,
    required this.user,
  });

  final String token;
  final DateTime expiresAtUtc;
  final AppUser user;
}

class AuthRepository {
  const AuthRepository(this._dio);

  final Dio _dio;

  Future<AuthResult> login(
    String email,
    String password,
  ) async {
    try {
      final response = await _dio.post<Map<String, dynamic>>(
        '/api/auth/login',
        data: {
          'email': email.trim(),
          'password': password,
        },
      );

      final data = response.data;

      if (data == null) {
        throw const ApiException(
          'La API devolvió una respuesta vacía.',
        );
      }

      final token = data['token'] as String? ?? '';
      final expirationText =
          data['expiresAtUtc'] as String? ?? '';
      final userData =
          data['user'] as Map<String, dynamic>?;

      if (token.isEmpty ||
          expirationText.isEmpty ||
          userData == null) {
        throw const ApiException(
          'La respuesta de autenticación no es válida.',
        );
      }

      final expiresAtUtc = DateTime.tryParse(expirationText);

      if (expiresAtUtc == null) {
        throw const ApiException(
          'La expiración del token no es válida.',
        );
      }

      return AuthResult(
        token: token,
        expiresAtUtc: expiresAtUtc.toUtc(),
        user: AppUser.fromJson(userData),
      );
    } on DioException catch (error) {
      throw ApiException.fromDio(error);
    }
  }

  Future<void> register({
    required String fullName,
    required String email,
    required String password,
  }) async {
    try {
      await _dio.post<Map<String, dynamic>>(
        '/api/auth/register',
        data: {
          'nombreCompleto': fullName.trim(),
          'email': email.trim(),
          'password': password,
        },
      );
    } on DioException catch (error) {
      throw ApiException.fromDio(error);
    }
  }
}