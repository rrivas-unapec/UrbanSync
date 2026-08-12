import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/network/api_exception.dart';
import '../../../core/network/dio_client.dart';
import '../domain/incident_work.dart';

final incidentWorkRepositoryProvider = Provider<IncidentWorkRepository>(
  (ref) => IncidentWorkRepository(ref.read(dioProvider)),
);

class IncidentWorkRepository {
  const IncidentWorkRepository(this._dio);
  final Dio _dio;

  Future<List<T>> _listByIncident<T>(
    String resource,
    int incidentId,
    T Function(Map<String, dynamic>) parse,
  ) async {
    try {
      final response = await _dio.get<List<dynamic>>(
        '/api/$resource/by-incident/$incidentId',
      );
      return response.data!
          .map((e) => parse(e as Map<String, dynamic>))
          .toList();
    } on DioException catch (error) {
      throw ApiException.fromDio(error);
    }
  }

  Future<List<IncidentEvidence>> evidences(int incidentId) =>
      _listByIncident('evidences', incidentId, IncidentEvidence.fromJson);

  Future<IncidentEvidence> createEvidence({
    required int incidenciaId,
    required String tipoEvidencia,
    required String rutaArchivo,
    required int usuarioSubeId,
    String? descripcion,
  }) async {
    try {
      final response = await _dio.post<Map<String, dynamic>>(
        '/api/evidences',
        data: {
          'incidentId': incidenciaId,
          'evidenceType': tipoEvidencia,
          'filePath': rutaArchivo,
          'description': ?descripcion,
          'uploadedByUserId': usuarioSubeId,
        },
      );
      return IncidentEvidence.fromJson(response.data!);
    } on DioException catch (error) {
      throw ApiException.fromDio(error);
    }
  }

  Future<TechnicalAnalysis?> technicalAnalysis(int incidentId) async {
    try {
      final response = await _dio.get<Map<String, dynamic>>(
        '/api/technical-analyses/by-incident/$incidentId',
      );
      return TechnicalAnalysis.fromJson(response.data!);
    } on DioException catch (error) {
      if (error.response?.statusCode == 404) return null;
      throw ApiException.fromDio(error);
    }
  }

  Future<TechnicalAnalysis> createTechnicalAnalysis({
    required int incidenciaId,
    required int usuarioTecnicoId,
    required String diagnostico,
    String? accionesRecomendadas,
  }) async {
    try {
      final response = await _dio.post<Map<String, dynamic>>(
        '/api/technical-analyses',
        data: {
          'incidentId': incidenciaId,
          'technicalUserId': usuarioTecnicoId,
          'diagnosis': diagnostico,
          'recommendedActions': ?accionesRecomendadas,
        },
      );
      return TechnicalAnalysis.fromJson(response.data!);
    } on DioException catch (error) {
      throw ApiException.fromDio(error);
    }
  }

  Future<List<IncidentJob>> jobs(int incidentId) =>
      _listByIncident('jobs', incidentId, IncidentJob.fromJson);

  Future<IncidentJob> createJob({
    required int incidenciaId,
    required int usuarioAsignadoId,
    required String descripcionTrabajo,
    String estado = 'Pendiente',
    DateTime? fechaInicio,
    DateTime? fechaFin,
    String? resultado,
  }) async {
    try {
      final response = await _dio.post<Map<String, dynamic>>(
        '/api/jobs',
        data: {
          'incidentId': incidenciaId,
          'assignedUserId': usuarioAsignadoId,
          'jobDescription': descripcionTrabajo,
          'status': estado,
          'startDate': ?fechaInicio?.toUtc().toIso8601String(),
          'endDate': ?fechaFin?.toUtc().toIso8601String(),
          'result': ?resultado,
        },
      );
      return IncidentJob.fromJson(response.data!);
    } on DioException catch (error) {
      throw ApiException.fromDio(error);
    }
  }

  Future<IncidentJob> updateJob(
    int jobId, {
    required String estado,
    DateTime? fechaInicio,
    DateTime? fechaFin,
    String? resultado,
  }) async {
    try {
      final response = await _dio.put<Map<String, dynamic>>(
        '/api/jobs/$jobId',
        data: {
          'status': estado,
          'startDate': ?fechaInicio?.toUtc().toIso8601String(),
          'endDate': ?fechaFin?.toUtc().toIso8601String(),
          'result': ?resultado,
        },
      );
      return IncidentJob.fromJson(response.data!);
    } on DioException catch (error) {
      throw ApiException.fromDio(error);
    }
  }

  Future<List<IncidentReport>> reports(int incidentId) =>
      _listByIncident('reports', incidentId, IncidentReport.fromJson);

  Future<IncidentReport> createReport({
    required int incidenciaId,
    required int generadoPorId,
    int? trabajoId,
    String? contenido,
    String? rutaArchivo,
  }) async {
    try {
      final response = await _dio.post<Map<String, dynamic>>(
        '/api/reports',
        data: {
          'incidentId': incidenciaId,
          'jobId': ?trabajoId,
          'generatedByUserId': generadoPorId,
          'content': ?contenido,
          'filePath': ?rutaArchivo,
        },
      );
      return IncidentReport.fromJson(response.data!);
    } on DioException catch (error) {
      throw ApiException.fromDio(error);
    }
  }
}
