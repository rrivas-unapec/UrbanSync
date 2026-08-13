import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/network/api_exception.dart';
import '../../../core/network/dio_client.dart';
import '../domain/catalog.dart';
import '../domain/incident.dart';
import '../domain/urban_asset.dart';

final incidentsRepositoryProvider = Provider<IncidentsRepository>(
  (ref) => IncidentsRepository(ref.read(dioProvider)),
);

class IncidentsRepository {
  const IncidentsRepository(this._dio);
  final Dio _dio;

  Future<List<Incident>> list({String? status, bool mine = false}) async {
    try {
      final response = await _dio.get<List<dynamic>>(
        '/api/incidents',
        queryParameters: {
          if (status != null) 'status': status,
          if (mine) 'mine': true,
        },
      );
      return response.data!
          .map((e) => Incident.fromJson(e as Map<String, dynamic>))
          .toList();
    } on DioException catch (error) {
      throw ApiException.fromDio(error);
    }
  }

  Future<Incident> getById(int id) async {
    try {
      final response = await _dio.get<Map<String, dynamic>>(
        '/api/incidents/$id',
      );
      return Incident.fromJson(response.data!);
    } on DioException catch (error) {
      throw ApiException.fromDio(error);
    }
  }

  Future<Incident> create({
    required int tipoIncidenciaId,
    required String descripcion,
    required String prioridad,
    required double lat,
    required double lng,
    required String direccion,
    String? referencia,
    int? jurisdiccionId,
    int? activoId,
  }) async {
    try {
      final response = await _dio.post<Map<String, dynamic>>(
        '/api/incidents',
        data: {
          'tipoIncidenciaId': tipoIncidenciaId,
          'activoId': ?activoId,
          'descripcion': descripcion,
          'prioridad': prioridad,
          'ubicacion': {
            'lat': lat,
            'lng': lng,
            'direccion': direccion,
            'referencia': referencia,
            'jurisdiccionId': jurisdiccionId,
          },
        },
      );
      return Incident.fromJson(response.data!);
    } on DioException catch (error) {
      throw ApiException.fromDio(error);
    }
  }

  Future<Incident> triage(
    int id, {
    int? tipoIncidenciaId,
    String? prioridad,
    String? accion,
    int? jurisdiccionId,
  }) async {
    try {
      final response = await _dio.patch<Map<String, dynamic>>(
        '/api/incidents/$id/triage',
        data: {
          'tipoIncidenciaId': tipoIncidenciaId,
          'prioridad': prioridad,
          'accion': accion,
          'jurisdiccionId': jurisdiccionId,
        },
      );
      return Incident.fromJson(response.data!);
    } on DioException catch (error) {
      throw ApiException.fromDio(error);
    }
  }

  Future<Incident> updateStatus(int id, String estado) async {
    try {
      final response = await _dio.patch<Map<String, dynamic>>(
        '/api/incidents/$id/status',
        data: {'estado': estado},
      );
      return Incident.fromJson(response.data!);
    } on DioException catch (error) {
      throw ApiException.fromDio(error);
    }
  }

  Future<List<IncidentType>> incidentTypes() async {
    try {
      final response = await _dio.get<List<dynamic>>('/api/incident-types');
      return response.data!
          .map((e) => IncidentType.fromJson(e as Map<String, dynamic>))
          .where((type) => type.activo)
          .toList();
    } on DioException catch (error) {
      throw ApiException.fromDio(error);
    }
  }

  Future<List<UrbanAsset>> assets() async {
    try {
      final response = await _dio.get<List<dynamic>>('/api/assets');
      return response.data!
          .map((e) => UrbanAsset.fromJson(e as Map<String, dynamic>))
          .where((asset) => asset.activo)
          .toList();
    } on DioException catch (error) {
      throw ApiException.fromDio(error);
    }
  }

  Future<List<AssetHistoryEntry>> assetHistory(int assetId) async {
    try {
      final response = await _dio.get<List<dynamic>>(
        '/api/assets/$assetId/history',
      );
      return response.data!
          .map((e) => AssetHistoryEntry.fromJson(e as Map<String, dynamic>))
          .toList();
    } on DioException catch (error) {
      throw ApiException.fromDio(error);
    }
  }

  Future<List<Jurisdiction>> jurisdictions() async {
    try {
      final response = await _dio.get<List<dynamic>>('/api/jurisdictions');
      return response.data!
          .map((e) => Jurisdiction.fromJson(e as Map<String, dynamic>))
          .toList();
    } on DioException catch (error) {
      throw ApiException.fromDio(error);
    }
  }
}
