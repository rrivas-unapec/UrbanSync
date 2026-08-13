import 'package:flutter_test/flutter_test.dart';
import 'package:urbansync/features/incidents/domain/incident.dart';
import 'package:urbansync/features/reports/domain/report_summary.dart';

Incident _incident({
  required int id,
  String estado = 'Registrada',
  String prioridad = 'Media',
  String tipo = 'Alumbrado',
  String jurisdiccion = 'Distrito Nacional',
}) => Incident(
  id: id,
  codigoCaso: 'INC-$id',
  estado: estado,
  prioridad: prioridad,
  descripcion: 'x',
  tipoIncidenciaId: 1,
  tipoIncidencia: tipo,
  jurisdiccionId: 1,
  jurisdiccion: jurisdiccion,
  direccion: 'Calle 1',
  usuarioReporta: 'Ana',
  fechaReporte: DateTime.utc(2026, 8, 8),
);

void main() {
  group('ReportSummary.fromIncidents', () {
    test('cuenta el total y agrupa por cada dimensión', () {
      final summary = ReportSummary.fromIncidents([
        _incident(id: 1, estado: 'Registrada', prioridad: 'Alta'),
        _incident(id: 2, estado: 'Registrada', prioridad: 'Media'),
        _incident(id: 3, estado: 'Cerrada', prioridad: 'Alta'),
      ]);

      expect(summary.total, 3);

      expect(summary.porEstado.first.clave, 'Registrada');
      expect(summary.porEstado.first.total, 2);

      expect(summary.porPrioridad.first.clave, 'Alta');
      expect(summary.porPrioridad.first.total, 2);
    });

    test('ordena de mayor a menor', () {
      final summary = ReportSummary.fromIncidents([
        _incident(id: 1, tipo: 'Bacheo'),
        _incident(id: 2, tipo: 'Alumbrado'),
        _incident(id: 3, tipo: 'Alumbrado'),
        _incident(id: 4, tipo: 'Alumbrado'),
      ]);

      expect(summary.porTipo.map((e) => e.clave).toList(), [
        'Alumbrado',
        'Bacheo',
      ]);
      expect(summary.porTipo.first.total, 3);
    });

    test('ignora los valores vacíos en vez de contarlos', () {
      final summary = ReportSummary.fromIncidents([
        _incident(id: 1, jurisdiccion: 'Distrito Nacional'),
        _incident(id: 2, jurisdiccion: ''),
      ]);

      expect(summary.total, 2);
      expect(summary.porJurisdiccion, hasLength(1));
      expect(summary.porJurisdiccion.single.clave, 'Distrito Nacional');
    });

    test('sin incidencias devuelve un resumen vacío, no un error', () {
      final summary = ReportSummary.fromIncidents(const []);

      expect(summary.total, 0);
      expect(summary.porEstado, isEmpty);
      expect(summary.porTipo, isEmpty);
      expect(summary.porPrioridad, isEmpty);
      expect(summary.porJurisdiccion, isEmpty);
    });
  });
}
