import 'package:flutter_test/flutter_test.dart';
import 'package:urbansync/features/catalogs/domain/catalogs.dart';
import 'package:urbansync/features/claims/domain/claim.dart';
import 'package:urbansync/features/incidents/domain/incident_work.dart';

void main() {
  group('Catálogos', () {
    test('JurisdictionItem mapea el contrato en inglés', () {
      final item = JurisdictionItem.fromJson({
        'id': 3,
        'name': 'Distrito Nacional',
        'level': 'Provincia',
        'parentJurisdictionId': 1,
        'parentJurisdictionName': 'República Dominicana',
        'isActive': true,
      });

      expect(item.nombre, 'Distrito Nacional');
      expect(item.nivel, 'Provincia');
      expect(item.jurisdiccionPadreNombre, 'República Dominicana');
      expect(item.activo, isTrue);
    });

    test('JurisdictionItem sin padre deja los campos nulos', () {
      final item = JurisdictionItem.fromJson({
        'id': 1,
        'name': 'Raíz',
        'level': 'Pais',
        'parentJurisdictionId': null,
        'parentJurisdictionName': null,
        'isActive': false,
      });

      expect(item.jurisdiccionPadreId, isNull);
      expect(item.activo, isFalse);
    });

    test('Department mapea nombre y jurisdicción', () {
      final item = Department.fromJson({
        'id': 5,
        'name': 'Obras Públicas',
        'jurisdictionId': 3,
        'jurisdictionName': 'Distrito Nacional',
        'isActive': true,
      });

      expect(item.nombre, 'Obras Públicas');
      expect(item.jurisdiccionNombre, 'Distrito Nacional');
    });

    test('InstitutionItem mapea contactos opcionales', () {
      final item = InstitutionItem.fromJson({
        'id': 2,
        'name': 'Ayuntamiento',
        'institutionType': 'Municipal',
        'contactEmail': 'info@ayto.do',
        'contactPhone': null,
        'isActive': true,
      });

      expect(item.tipoInstitucion, 'Municipal');
      expect(item.contactoEmail, 'info@ayto.do');
      expect(item.contactoTelefono, isNull);
    });

    test('LocationItem mapea coordenadas y jurisdicción', () {
      final item = LocationItem.fromJson({
        'id': 9,
        'address': 'Av. Independencia 100',
        'reference': 'Frente al parque',
        'latitude': 18.47,
        'longitude': -69.91,
        'jurisdictionId': 3,
        'jurisdictionName': 'Distrito Nacional',
        'createdAt': '2026-08-08T09:00:00',
      });

      expect(item.direccion, 'Av. Independencia 100');
      expect(item.latitud, 18.47);
      expect(item.jurisdiccionNombre, 'Distrito Nacional');
    });
  });

  group('Trabajo sobre la incidencia', () {
    test('IncidentEvidence mapea la respuesta y detecta imagen', () {
      final evidence = IncidentEvidence.fromJson({
        'id': 4,
        'incidentId': 7,
        'evidenceType': 'Foto',
        'filePath': '/uploads/foto.JPG',
        'description': 'Antes de la reparación',
        'uploadedByUserId': 2,
        'uploadedByUserName': 'Ana',
        'uploadedAt': '2026-08-08T11:00:00',
      });

      expect(evidence.incidenciaId, 7);
      expect(evidence.usuarioSube, 'Ana');
      expect(evidence.esImagen, isTrue);
    });

    test('IncidentEvidence no marca como imagen un pdf', () {
      final evidence = IncidentEvidence.fromJson({
        'id': 5,
        'incidentId': 7,
        'evidenceType': 'Documento',
        'filePath': '/uploads/acta.pdf',
        'description': null,
        'uploadedByUserId': 2,
        'uploadedByUserName': 'Ana',
        'uploadedAt': '2026-08-08T11:00:00',
      });

      expect(evidence.esImagen, isFalse);
      expect(evidence.descripcion, isNull);
    });

    test('TechnicalAnalysis mapea diagnóstico y acciones', () {
      final analysis = TechnicalAnalysis.fromJson({
        'id': 1,
        'incidentId': 7,
        'technicalUserId': 3,
        'technicalUserName': 'Luis',
        'diagnosis': 'Transformador dañado',
        'recommendedActions': 'Reemplazar unidad',
        'analysisDate': '2026-08-08T12:00:00',
      });

      expect(analysis.diagnostico, 'Transformador dañado');
      expect(analysis.accionesRecomendadas, 'Reemplazar unidad');
      expect(analysis.usuarioTecnico, 'Luis');
    });

    test('IncidentJob mapea fechas opcionales', () {
      final job = IncidentJob.fromJson({
        'id': 8,
        'incidentId': 7,
        'assignedUserId': 3,
        'assignedUserName': 'Luis',
        'jobDescription': 'Cambio de luminaria',
        'status': 'EnProgreso',
        'startDate': '2026-08-08T13:00:00',
        'endDate': null,
        'result': null,
      });

      expect(job.estado, 'EnProgreso');
      expect(job.fechaInicio, isNotNull);
      expect(job.fechaFin, isNull);
      expect(IncidentJob.estados, contains('Finalizado'));
    });

    test('IncidentReport mapea trabajo opcional', () {
      final report = IncidentReport.fromJson({
        'id': 2,
        'incidentId': 7,
        'jobId': 8,
        'generatedByUserId': 3,
        'generatedByUserName': 'Luis',
        'content': 'Trabajo completado',
        'filePath': null,
        'generatedAt': '2026-08-08T15:00:00',
      });

      expect(report.trabajoId, 8);
      expect(report.contenido, 'Trabajo completado');
      expect(report.rutaArchivo, isNull);
    });

    test('IncidentReport sin trabajo asociado', () {
      final report = IncidentReport.fromJson({
        'id': 3,
        'incidentId': 7,
        'jobId': null,
        'generatedByUserId': 3,
        'generatedByUserName': 'Luis',
        'content': null,
        'filePath': '/uploads/reporte.pdf',
        'generatedAt': '2026-08-08T15:00:00',
      });

      expect(report.trabajoId, isNull);
      expect(report.rutaArchivo, '/uploads/reporte.pdf');
    });
  });

  group('Reclamaciones', () {
    test('Claim mapea la respuesta completa', () {
      final claim = Claim.fromJson({
        'id': 12,
        'citizenUserId': 4,
        'citizenUserName': 'Pedro',
        'locationId': 9,
        'locationAddress': 'Av. Independencia 100',
        'category': 'Reclamacion',
        'title': 'Basura acumulada',
        'description': 'Lleva tres días sin recoger.',
        'status': 'Abierta',
        'createdAt': '2026-08-08T08:00:00',
      });

      expect(claim.ciudadano, 'Pedro');
      expect(claim.ubicacionDireccion, 'Av. Independencia 100');
      expect(claim.categoria, 'Reclamacion');
      expect(claim.estado, 'Abierta');
    });
  });
}
