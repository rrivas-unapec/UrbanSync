import 'package:flutter_test/flutter_test.dart';
import 'package:urbansync/features/incidents/domain/catalog.dart';
import 'package:urbansync/features/incidents/domain/incident.dart';
import 'package:urbansync/features/incidents/domain/urban_asset.dart';

void main() {
  group('IncidentType.fromJson', () {
    test('mapea los nombres en inglés del API', () {
      final type = IncidentType.fromJson({
        'id': 3,
        'name': 'Alumbrado público',
        'description': 'Fallas de iluminación',
        'institutionId': 2,
        'institutionName': 'Ayuntamiento',
        'isActive': true,
      });

      expect(type.id, 3);
      expect(type.nombre, 'Alumbrado público');
      expect(type.descripcion, 'Fallas de iluminación');
      expect(type.institucionId, 2);
      expect(type.institucionNombre, 'Ayuntamiento');
      expect(type.activo, isTrue);
    });

    test('un tipo inactivo se marca como tal', () {
      final type = IncidentType.fromJson({
        'id': 4,
        'name': 'Obsoleto',
        'institutionId': 1,
        'institutionName': 'X',
        'isActive': false,
      });

      expect(type.activo, isFalse);
    });

    test('tolera descripción ausente', () {
      final type = IncidentType.fromJson({
        'id': 5,
        'name': 'Sin descripcion',
        'institutionId': 1,
        'institutionName': 'X',
        'isActive': true,
      });

      expect(type.descripcion, isNull);
    });
  });

  group('UrbanAsset.fromJson', () {
    test('mapea el activo completo', () {
      final asset = UrbanAsset.fromJson({
        'id': 11,
        'code': 'LUM-004',
        'name': 'Luminaria Parque Central',
        'type': 'Luminaria',
        'status': 'Operativo',
        'jurisdictionId': 7,
        'jurisdictionName': 'Distrito Nacional',
        'installationDate': '2024-03-15T00:00:00',
        'isActive': true,
      });

      expect(asset.id, 11);
      expect(asset.etiqueta, 'LUM-004 · Luminaria Parque Central');
      expect(asset.jurisdiccionId, 7);
      expect(asset.jurisdiccionNombre, 'Distrito Nacional');
      expect(asset.fechaInstalacion, isNotNull);
    });

    test('sin código la etiqueta cae al nombre', () {
      final asset = UrbanAsset.fromJson({
        'id': 12,
        'code': '',
        'name': 'Banco de plaza',
        'type': 'Mobiliario',
        'status': 'Operativo',
        'jurisdictionId': 1,
        'jurisdictionName': 'DN',
        'installationDate': null,
        'isActive': true,
      });

      expect(asset.etiqueta, 'Banco de plaza');
      expect(asset.fechaInstalacion, isNull);
    });
  });

  group('AssetHistoryEntry.fromJson', () {
    test('mapea el historial del activo', () {
      final entry = AssetHistoryEntry.fromJson({
        'incidentId': 42,
        'caseCode': 'INC-20260808-AB12CD34',
        'incidentType': 'Alumbrado público',
        'description': 'Luminaria apagada',
        'status': 'Cerrada',
        'reportDate': '2026-08-08T10:15:00',
      });

      expect(entry.incidenciaId, 42);
      expect(entry.codigoCaso, 'INC-20260808-AB12CD34');
      expect(entry.estado, 'Cerrada');
    });
  });

  group('Incident.fromJson', () {
    test('lee el activo asociado cuando viene', () {
      final incident = Incident.fromJson({
        'id': 1,
        'codigoCaso': 'INC-1',
        'estado': 'Registrada',
        'prioridad': 'Media',
        'descripcion': 'x',
        'tipoIncidenciaId': 1,
        'tipoIncidencia': 'Alumbrado',
        'activoId': 11,
        'jurisdiccionId': 7,
        'jurisdiccion': 'DN',
        'direccion': 'Calle 1',
        'usuarioReporta': 'Ana',
        'fechaReporte': '2026-08-08T10:00:00Z',
      });

      expect(incident.activoId, 11);
    });

    test('activoId es nulo cuando la incidencia no tiene activo', () {
      final incident = Incident.fromJson({
        'id': 2,
        'codigoCaso': 'INC-2',
        'estado': 'Registrada',
        'prioridad': 'Media',
        'descripcion': 'x',
        'tipoIncidenciaId': 1,
        'tipoIncidencia': 'Alumbrado',
        'jurisdiccionId': 7,
        'jurisdiccion': 'DN',
        'direccion': 'Calle 1',
        'usuarioReporta': 'Ana',
        'fechaReporte': '2026-08-08T10:00:00Z',
      });

      expect(incident.activoId, isNull);
    });
  });
}
