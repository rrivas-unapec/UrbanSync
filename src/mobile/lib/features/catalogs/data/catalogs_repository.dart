import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/network/api_exception.dart';
import '../../../core/network/dio_client.dart';
import '../domain/catalogs.dart';

final catalogsRepositoryProvider = Provider<CatalogsRepository>(
  (ref) => CatalogsRepository(ref.read(dioProvider)),
);

class CatalogsRepository {
  const CatalogsRepository(this._dio);
  final Dio _dio;

  Future<List<T>> _list<T>(
    String path,
    T Function(Map<String, dynamic>) parse,
  ) async {
    try {
      final response = await _dio.get<List<dynamic>>(path);
      return response.data!
          .map((e) => parse(e as Map<String, dynamic>))
          .toList();
    } on DioException catch (error) {
      throw ApiException.fromDio(error);
    }
  }

  Future<T> _single<T>(
    String path,
    T Function(Map<String, dynamic>) parse,
  ) async {
    try {
      final response = await _dio.get<Map<String, dynamic>>(path);
      return parse(response.data!);
    } on DioException catch (error) {
      throw ApiException.fromDio(error);
    }
  }

  Future<List<JurisdictionItem>> jurisdictions() =>
      _list('/api/jurisdictions', JurisdictionItem.fromJson);

  Future<JurisdictionItem> jurisdiction(int id) =>
      _single('/api/jurisdictions/$id', JurisdictionItem.fromJson);

  Future<JurisdictionItem> createJurisdiction({
    required String nombre,
    required String nivel,
    int? jurisdiccionPadreId,
  }) async {
    try {
      final response = await _dio.post<Map<String, dynamic>>(
        '/api/jurisdictions',
        data: {
          'name': nombre,
          'level': nivel,
          'parentJurisdictionId': ?jurisdiccionPadreId,
        },
      );
      return JurisdictionItem.fromJson(response.data!);
    } on DioException catch (error) {
      throw ApiException.fromDio(error);
    }
  }

  Future<List<Department>> departments() =>
      _list('/api/departments', Department.fromJson);

  Future<Department> department(int id) =>
      _single('/api/departments/$id', Department.fromJson);

  Future<Department> createDepartment({
    required String nombre,
    int? jurisdiccionId,
  }) async {
    try {
      final response = await _dio.post<Map<String, dynamic>>(
        '/api/departments',
        data: {'name': nombre, 'jurisdictionId': ?jurisdiccionId},
      );
      return Department.fromJson(response.data!);
    } on DioException catch (error) {
      throw ApiException.fromDio(error);
    }
  }

  Future<List<InstitutionItem>> institutions() =>
      _list('/api/institutions', InstitutionItem.fromJson);

  Future<InstitutionItem> institution(int id) =>
      _single('/api/institutions/$id', InstitutionItem.fromJson);

  Future<InstitutionItem> createInstitution({
    required String nombre,
    required String tipoInstitucion,
    String? contactoEmail,
    String? contactoTelefono,
  }) async {
    try {
      final response = await _dio.post<Map<String, dynamic>>(
        '/api/institutions',
        data: {
          'name': nombre,
          'institutionType': tipoInstitucion,
          'contactEmail': ?contactoEmail,
          'contactPhone': ?contactoTelefono,
        },
      );
      return InstitutionItem.fromJson(response.data!);
    } on DioException catch (error) {
      throw ApiException.fromDio(error);
    }
  }

  Future<List<LocationItem>> locations() =>
      _list('/api/locations', LocationItem.fromJson);

  Future<LocationItem> location(int id) =>
      _single('/api/locations/$id', LocationItem.fromJson);

  Future<LocationItem> createLocation({
    required String direccion,
    required int jurisdiccionId,
    String? referencia,
    double? latitud,
    double? longitud,
  }) async {
    try {
      final response = await _dio.post<Map<String, dynamic>>(
        '/api/locations',
        data: {
          'address': direccion,
          'jurisdictionId': jurisdiccionId,
          'reference': ?referencia,
          'latitude': ?latitud,
          'longitude': ?longitud,
        },
      );
      return LocationItem.fromJson(response.data!);
    } on DioException catch (error) {
      throw ApiException.fromDio(error);
    }
  }
}
