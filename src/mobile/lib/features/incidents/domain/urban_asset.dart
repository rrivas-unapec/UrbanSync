class UrbanAsset {
  const UrbanAsset({
    required this.id,
    required this.codigo,
    required this.nombre,
    required this.tipo,
    required this.estado,
    required this.jurisdiccionId,
    required this.jurisdiccionNombre,
    this.fechaInstalacion,
    this.activo = true,
  });

  final int id;
  final String codigo;
  final String nombre;
  final String tipo;
  final String estado;
  final int jurisdiccionId;
  final String jurisdiccionNombre;
  final DateTime? fechaInstalacion;
  final bool activo;

  String get etiqueta => codigo.isEmpty ? nombre : '$codigo · $nombre';

  factory UrbanAsset.fromJson(Map<String, dynamic> json) => UrbanAsset(
    id: (json['id'] as num).toInt(),
    codigo: json['code'] as String? ?? '',
    nombre: json['name'] as String? ?? '',
    tipo: json['type'] as String? ?? '',
    estado: json['status'] as String? ?? '',
    jurisdiccionId: (json['jurisdictionId'] as num?)?.toInt() ?? 0,
    jurisdiccionNombre: json['jurisdictionName'] as String? ?? '',
    fechaInstalacion: json['installationDate'] == null
        ? null
        : DateTime.parse(json['installationDate'] as String),
    activo: json['isActive'] as bool? ?? true,
  );
}

class AssetHistoryEntry {
  const AssetHistoryEntry({
    required this.incidenciaId,
    required this.codigoCaso,
    required this.tipoIncidencia,
    required this.descripcion,
    required this.estado,
    required this.fechaReporte,
  });

  final int incidenciaId;
  final String codigoCaso;
  final String tipoIncidencia;
  final String descripcion;
  final String estado;
  final DateTime fechaReporte;

  factory AssetHistoryEntry.fromJson(Map<String, dynamic> json) =>
      AssetHistoryEntry(
        incidenciaId: (json['incidentId'] as num).toInt(),
        codigoCaso: json['caseCode'] as String? ?? '',
        tipoIncidencia: json['incidentType'] as String? ?? '',
        descripcion: json['description'] as String? ?? '',
        estado: json['status'] as String? ?? '',
        fechaReporte: DateTime.parse(json['reportDate'] as String),
      );
}
