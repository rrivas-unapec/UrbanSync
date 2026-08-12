import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/network/api_exception.dart';
import '../../../core/network/dio_client.dart';
import '../domain/claim.dart';

final claimsRepositoryProvider = Provider<ClaimsRepository>(
  (ref) => ClaimsRepository(ref.read(dioProvider)),
);

class ClaimsRepository {
  const ClaimsRepository(this._dio);
  final Dio _dio;

  Future<List<Claim>> all() => _list('/api/claims');

  Future<List<Claim>> byCitizen(int citizenUserId) =>
      _list('/api/claims/my-claims/$citizenUserId');

  Future<List<Claim>> _list(String path) async {
    try {
      final response = await _dio.get<List<dynamic>>(path);
      return response.data!
          .map((e) => Claim.fromJson(e as Map<String, dynamic>))
          .toList();
    } on DioException catch (error) {
      throw ApiException.fromDio(error);
    }
  }

  Future<Claim> getById(int id) async {
    try {
      final response = await _dio.get<Map<String, dynamic>>('/api/claims/$id');
      return Claim.fromJson(response.data!);
    } on DioException catch (error) {
      throw ApiException.fromDio(error);
    }
  }

  Future<Claim> create({
    required int ciudadanoId,
    required int ubicacionId,
    required String categoria,
    required String titulo,
    required String descripcion,
  }) async {
    try {
      final response = await _dio.post<Map<String, dynamic>>(
        '/api/claims',
        data: {
          'citizenUserId': ciudadanoId,
          'locationId': ubicacionId,
          'category': categoria,
          'title': titulo,
          'description': descripcion,
        },
      );
      return Claim.fromJson(response.data!);
    } on DioException catch (error) {
      throw ApiException.fromDio(error);
    }
  }

  Future<Claim> updateStatus(int id, String estado) async {
    try {
      final response = await _dio.put<Map<String, dynamic>>(
        '/api/claims/$id/status',
        data: {'status': estado},
      );
      return Claim.fromJson(response.data!);
    } on DioException catch (error) {
      throw ApiException.fromDio(error);
    }
  }
}
