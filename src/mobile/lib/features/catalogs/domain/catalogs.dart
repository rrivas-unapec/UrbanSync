class JurisdictionItem {
  const JurisdictionItem({
    required this.id,
    required this.nombre,
    required this.nivel,
    this.jurisdiccionPadreId,
    this.jurisdiccionPadreNombre,
    this.activo = true,
  });

  final int id;
  final String nombre;
  final String nivel;
  final int? jurisdiccionPadreId;
  final String? jurisdiccionPadreNombre;
  final bool activo;

  factory JurisdictionItem.fromJson(Map<String, dynamic> json) =>
      JurisdictionItem(
        id: (json['id'] as num).toInt(),
        nombre: json['name'] as String? ?? '',
        nivel: json['level'] as String? ?? '',
        jurisdiccionPadreId: (json['parentJurisdictionId'] as num?)?.toInt(),
        jurisdiccionPadreNombre: json['parentJurisdictionName'] as String?,
        activo: json['isActive'] as bool? ?? true,
      );
}

class Department {
  const Department({
    required this.id,
    required this.nombre,
    this.jurisdiccionId,
    this.jurisdiccionNombre,
    this.activo = true,
  });

  final int id;
  final String nombre;
  final int? jurisdiccionId;
  final String? jurisdiccionNombre;
  final bool activo;

  factory Department.fromJson(Map<String, dynamic> json) => Department(
    id: (json['id'] as num).toInt(),
    nombre: json['name'] as String? ?? '',
    jurisdiccionId: (json['jurisdictionId'] as num?)?.toInt(),
    jurisdiccionNombre: json['jurisdictionName'] as String?,
    activo: json['isActive'] as bool? ?? true,
  );
}

class InstitutionItem {
  const InstitutionItem({
    required this.id,
    required this.nombre,
    required this.tipoInstitucion,
    this.contactoEmail,
    this.contactoTelefono,
    this.activo = true,
  });

  final int id;
  final String nombre;
  final String tipoInstitucion;
  final String? contactoEmail;
  final String? contactoTelefono;
  final bool activo;

  factory InstitutionItem.fromJson(Map<String, dynamic> json) =>
      InstitutionItem(
        id: (json['id'] as num).toInt(),
        nombre: json['name'] as String? ?? '',
        tipoInstitucion: json['institutionType'] as String? ?? '',
        contactoEmail: json['contactEmail'] as String?,
        contactoTelefono: json['contactPhone'] as String?,
        activo: json['isActive'] as bool? ?? true,
      );
}

class LocationItem {
  const LocationItem({
    required this.id,
    required this.direccion,
    required this.jurisdiccionId,
    required this.jurisdiccionNombre,
    required this.fechaCreacion,
    this.referencia,
    this.latitud,
    this.longitud,
  });

  final int id;
  final String direccion;
  final int jurisdiccionId;
  final String jurisdiccionNombre;
  final DateTime fechaCreacion;
  final String? referencia;
  final double? latitud;
  final double? longitud;

  factory LocationItem.fromJson(Map<String, dynamic> json) => LocationItem(
    id: (json['id'] as num).toInt(),
    direccion: json['address'] as String? ?? '',
    jurisdiccionId: (json['jurisdictionId'] as num?)?.toInt() ?? 0,
    jurisdiccionNombre: json['jurisdictionName'] as String? ?? '',
    fechaCreacion: DateTime.parse(json['createdAt'] as String),
    referencia: json['reference'] as String?,
    latitud: (json['latitude'] as num?)?.toDouble(),
    longitud: (json['longitude'] as num?)?.toDouble(),
  );
}
