import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../features/auth/presentation/auth_controller.dart';
import '../env/env.dart';
import '../storage/token_storage.dart';

final dioProvider = Provider<Dio>((ref) {
  final tokenStorage = ref.read(tokenStorageProvider);

  final dio = Dio(
    BaseOptions(
      baseUrl: AppEnv.baseUrl,
      connectTimeout: const Duration(seconds: 15),
      receiveTimeout: const Duration(seconds: 20),
      sendTimeout: const Duration(seconds: 20),
      headers: const {
        'Accept': 'application/json',
        'Content-Type': 'application/json',
      },
    ),
  );

  dio.interceptors.add(
    InterceptorsWrapper(
      onRequest: (options, handler) async {
        final isAuthenticationRequest =
            options.path.contains('/api/auth/login') ||
            options.path.contains('/api/auth/register');

        if (!isAuthenticationRequest) {
          final session = await tokenStorage.readSession();

          if (session != null && session.isExpired) {
            await ref
                .read(authControllerProvider.notifier)
                .markSessionExpired();

            return handler.reject(
              DioException(
                requestOptions: options,
                type: DioExceptionType.cancel,
                message: 'La sesión ha expirado.',
              ),
            );
          }

          final token = session?.token;

          if (token != null && token.isNotEmpty) {
            options.headers['Authorization'] =
                'Bearer $token';
          }
        }

        handler.next(options);
      },
      onResponse: (response, handler) async {
        if (response.statusCode == 401) {
          final path = response.requestOptions.path;

          final isAuthenticationRequest =
              path.contains('/api/auth/login') ||
              path.contains('/api/auth/register');

          if (!isAuthenticationRequest) {
            await ref
                .read(authControllerProvider.notifier)
                .markSessionExpired();
          }
        }

        handler.next(response);
      },
      onError: (error, handler) async {
        final path = error.requestOptions.path;

        final isAuthenticationRequest =
            path.contains('/api/auth/login') ||
            path.contains('/api/auth/register');

        if (error.response?.statusCode == 401 &&
            !isAuthenticationRequest) {
          await ref
              .read(authControllerProvider.notifier)
              .markSessionExpired();
        }

        handler.next(error);
      },
    ),
  );

  return dio;
});