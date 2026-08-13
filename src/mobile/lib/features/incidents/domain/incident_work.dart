class IncidentEvidence {
  const IncidentEvidence({
    required this.id,
    required this.incidenciaId,
    required this.tipoEvidencia,
    required this.rutaArchivo,
    required this.usuarioSubeId,
    required this.usuarioSube,
    required this.fechaSubida,
    this.descripcion,
  });

  final int id;
  final int incidenciaId;
  final String tipoEvidencia;
  final String rutaArchivo;
  final int usuarioSubeId;
  final String usuarioSube;
  final DateTime fechaSubida;
  final String? descripcion;

  bool get esImagen {
    final path = rutaArchivo.toLowerCase();
    return path.endsWith('.jpg') ||
        path.endsWith('.jpeg') ||
        path.endsWith('.png') ||
        path.endsWith('.webp') ||
        path.endsWith('.gif');
  }

  factory IncidentEvidence.fromJson(Map<String, dynamic> json) =>
      IncidentEvidence(
        id: (json['id'] as num).toInt(),
        incidenciaId: (json['incidentId'] as num?)?.toInt() ?? 0,
        tipoEvidencia: json['evidenceType'] as String? ?? '',
        rutaArchivo: json['filePath'] as String? ?? '',
        usuarioSubeId: (json['uploadedByUserId'] as num?)?.toInt() ?? 0,
        usuarioSube: json['uploadedByUserName'] as String? ?? '',
        fechaSubida: DateTime.parse(json['uploadedAt'] as String),
        descripcion: json['description'] as String?,
      );
}

class TechnicalAnalysis {
  const TechnicalAnalysis({
    required this.id,
    required this.incidenciaId,
    required this.usuarioTecnicoId,
    required this.usuarioTecnico,
    required this.diagnostico,
    required this.fechaAnalisis,
    this.accionesRecomendadas,
  });

  final int id;
  final int incidenciaId;
  final int usuarioTecnicoId;
  final String usuarioTecnico;
  final String diagnostico;
  final DateTime fechaAnalisis;
  final String? accionesRecomendadas;

  factory TechnicalAnalysis.fromJson(Map<String, dynamic> json) =>
      TechnicalAnalysis(
        id: (json['id'] as num).toInt(),
        incidenciaId: (json['incidentId'] as num?)?.toInt() ?? 0,
        usuarioTecnicoId: (json['technicalUserId'] as num?)?.toInt() ?? 0,
        usuarioTecnico: json['technicalUserName'] as String? ?? '',
        diagnostico: json['diagnosis'] as String? ?? '',
        fechaAnalisis: DateTime.parse(json['analysisDate'] as String),
        accionesRecomendadas: json['recommendedActions'] as String?,
      );
}

class IncidentJob {
  const IncidentJob({
    required this.id,
    required this.incidenciaId,
    required this.usuarioAsignadoId,
    required this.usuarioAsignado,
    required this.descripcionTrabajo,
    required this.estado,
    this.fechaInicio,
    this.fechaFin,
    this.resultado,
  });

  static const estados = ['Pendiente', 'EnProgreso', 'Finalizado'];

  final int id;
  final int incidenciaId;
  final int usuarioAsignadoId;
  final String usuarioAsignado;
  final String descripcionTrabajo;
  final String estado;
  final DateTime? fechaInicio;
  final DateTime? fechaFin;
  final String? resultado;

  factory IncidentJob.fromJson(Map<String, dynamic> json) => IncidentJob(
    id: (json['id'] as num).toInt(),
    incidenciaId: (json['incidentId'] as num?)?.toInt() ?? 0,
    usuarioAsignadoId: (json['assignedUserId'] as num?)?.toInt() ?? 0,
    usuarioAsignado: json['assignedUserName'] as String? ?? '',
    descripcionTrabajo: json['jobDescription'] as String? ?? '',
    estado: json['status'] as String? ?? '',
    fechaInicio: json['startDate'] == null
        ? null
        : DateTime.parse(json['startDate'] as String),
    fechaFin: json['endDate'] == null
        ? null
        : DateTime.parse(json['endDate'] as String),
    resultado: json['result'] as String?,
  );
}

class IncidentReport {
  const IncidentReport({
    required this.id,
    required this.incidenciaId,
    required this.generadoPorId,
    required this.generadoPor,
    required this.fechaGeneracion,
    this.trabajoId,
    this.contenido,
    this.rutaArchivo,
  });

  final int id;
  final int incidenciaId;
  final int generadoPorId;
  final String generadoPor;
  final DateTime fechaGeneracion;
  final int? trabajoId;
  final String? contenido;
  final String? rutaArchivo;

  factory IncidentReport.fromJson(Map<String, dynamic> json) => IncidentReport(
    id: (json['id'] as num).toInt(),
    incidenciaId: (json['incidentId'] as num?)?.toInt() ?? 0,
    generadoPorId: (json['generatedByUserId'] as num?)?.toInt() ?? 0,
    generadoPor: json['generatedByUserName'] as String? ?? '',
    fechaGeneracion: DateTime.parse(json['generatedAt'] as String),
    trabajoId: (json['jobId'] as num?)?.toInt(),
    contenido: json['content'] as String?,
    rutaArchivo: json['filePath'] as String?,
  );
}
