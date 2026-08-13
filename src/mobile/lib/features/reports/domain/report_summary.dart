import '../../incidents/domain/incident.dart';

class CountItem {
  const CountItem({required this.clave, required this.total});

  final String clave;
  final int total;

  factory CountItem.fromJson(Map<String, dynamic> json) => CountItem(
    clave: json['clave'] as String? ?? '',
    total: json['total'] as int? ?? 0,
  );
}

class ReportSummary {
  const ReportSummary({
    required this.total,
    required this.porEstado,
    required this.porTipo,
    required this.porPrioridad,
    required this.porJurisdiccion,
  });

  final int total;
  final List<CountItem> porEstado;
  final List<CountItem> porTipo;
  final List<CountItem> porPrioridad;
  final List<CountItem> porJurisdiccion;

  static List<CountItem> _list(dynamic raw) =>
      (raw as List<dynamic>?)
          ?.map((e) => CountItem.fromJson(e as Map<String, dynamic>))
          .toList() ??
      const [];

  factory ReportSummary.fromJson(Map<String, dynamic> json) => ReportSummary(
    total: json['total'] as int? ?? 0,
    porEstado: _list(json['porEstado']),
    porTipo: _list(json['porTipo']),
    porPrioridad: _list(json['porPrioridad']),
    porJurisdiccion: _list(json['porJurisdiccion']),
  );

  static List<CountItem> agrupar(
    List<Incident> incidencias,
    String Function(Incident) clave,
  ) {
    final conteo = <String, int>{};

    for (final incidencia in incidencias) {
      final valor = clave(incidencia).trim();
      if (valor.isEmpty) continue;
      conteo[valor] = (conteo[valor] ?? 0) + 1;
    }

    final items = conteo.entries
        .map((e) => CountItem(clave: e.key, total: e.value))
        .toList();

    items.sort((a, b) => b.total.compareTo(a.total));

    return items;
  }

  factory ReportSummary.fromIncidents(List<Incident> incidencias) =>
      ReportSummary(
        total: incidencias.length,
        porEstado: agrupar(incidencias, (i) => i.estado),
        porTipo: agrupar(incidencias, (i) => i.tipoIncidencia),
        porPrioridad: agrupar(incidencias, (i) => i.prioridad),
        porJurisdiccion: agrupar(incidencias, (i) => i.jurisdiccion),
      );
}
