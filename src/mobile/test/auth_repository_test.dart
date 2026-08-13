import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';
import 'package:urbansync/core/network/api_exception.dart';
import 'package:urbansync/features/auth/data/auth_repository.dart';
import 'package:urbansync/features/auth/domain/app_user.dart';

class _MockDio extends Mock implements Dio {}

void main() {
  late _MockDio dio;
  late AuthRepository repository;

  setUp(() {
    dio = _MockDio();
    repository = AuthRepository(dio);
  });

  test(
    'login devuelve token, expiración y usuario',
    () async {
      when(
        () => dio.post<Map<String, dynamic>>(
          '/api/auth/login',
          data: any(named: 'data'),
        ),
      ).thenAnswer(
        (_) async => Response<Map<String, dynamic>>(
          requestOptions: RequestOptions(
            path: '/api/auth/login',
          ),
          data: {
            'token': 'header.payload.signature',
            'expiresAtUtc':
                '2026-08-05T18:00:00Z',
            'user': {
              'id': 1,
              'nombreUsuario':
                  'ciudadano@urbansync.com',
              'nombreCompleto':
                  'Ciudadano UrbanSync',
              'email':
                  'ciudadano@urbansync.com',
              'rolId': 6,
              'rolNombre': 'Ciudadano',
              'activo': true,
            },
          },
        ),
      );

      final result = await repository.login(
        'ciudadano@urbansync.com',
        'Clave1*',
      );

      expect(
        result.token,
        'header.payload.signature',
      );

      expect(
        result.expiresAtUtc,
        DateTime.parse(
          '2026-08-05T18:00:00Z',
        ),
      );

      expect(result.user.id, 1);
      expect(result.user.role, 'Ciudadano');
      expect(
        result.user.roleGroup,
        RoleGroup.ciudadano,
      );
    },
  );

  test(
    'login mapea error HTTP a ApiException',
    () async {
      when(
        () => dio.post<Map<String, dynamic>>(
          '/api/auth/login',
          data: any(named: 'data'),
        ),
      ).thenThrow(
        DioException(
          requestOptions: RequestOptions(
            path: '/api/auth/login',
          ),
          type: DioExceptionType.badResponse,
          response: Response(
            requestOptions: RequestOptions(
              path: '/api/auth/login',
            ),
            statusCode: 401,
          ),
        ),
      );

      expect(
        () => repository.login(
          'x@x.com',
          'bad',
        ),
        throwsA(isA<ApiException>()),
      );
    },
  );
}