enum RoleGroup {
  ciudadano,
  gestorUbicacion,
  gestorEvidencias,
  analistaTecnico,
  supervisorOperaciones,
  administrador,
  desconocido,
}

class AppUser {
  const AppUser({
    required this.id,
    required this.email,
    required this.fullName,
    required this.username,
    required this.roleId,
    required this.role,
    required this.isActive,
  });

  final int id;
  final String email;
  final String fullName;
  final String username;
  final int roleId;
  final String role;
  final bool isActive;

  RoleGroup get roleGroup {
    final normalizedRole = role
        .trim()
        .toLowerCase()
        .replaceAll(' ', '')
        .replaceAll('_', '')
        .replaceAll('á', 'a')
        .replaceAll('é', 'e')
        .replaceAll('í', 'i')
        .replaceAll('ó', 'o')
        .replaceAll('ú', 'u');

    switch (normalizedRole) {
      case 'admin':
      case 'administrador':
        return RoleGroup.administrador;

      case 'supervisor':
      case 'supervisoroperaciones':
        return RoleGroup.supervisorOperaciones;

      case 'analista':
      case 'analistatecnico':
      case 'tecnico':
        return RoleGroup.analistaTecnico;

      case 'gestorubicacion':
        return RoleGroup.gestorUbicacion;

      case 'gestorevidencias':
        return RoleGroup.gestorEvidencias;

      case 'ciudadano':
        return RoleGroup.ciudadano;

      default:
        return RoleGroup.desconocido;
    }
  }

  String get roleLabel {
    switch (roleGroup) {
      case RoleGroup.administrador:
        return 'Administrador';
      case RoleGroup.supervisorOperaciones:
        return 'Supervisor de Operaciones';
      case RoleGroup.analistaTecnico:
        return 'Analista Técnico';
      case RoleGroup.gestorUbicacion:
        return 'Gestor de Ubicación';
      case RoleGroup.gestorEvidencias:
        return 'Gestor de Evidencias';
      case RoleGroup.ciudadano:
        return 'Ciudadano';
      case RoleGroup.desconocido:
        return role.isEmpty ? 'Sin rol' : role;
    }
  }

  bool get isCitizen => roleGroup == RoleGroup.ciudadano;

  bool get isAdmin => roleGroup == RoleGroup.administrador;

  bool get isOperationsSupervisor =>
      roleGroup == RoleGroup.supervisorOperaciones;

  bool get isTechnician => roleGroup == RoleGroup.analistaTecnico;

  bool get isLocationManager => roleGroup == RoleGroup.gestorUbicacion;

  bool get isEvidenceManager => roleGroup == RoleGroup.gestorEvidencias;

  bool get isManager => isAdmin || isOperationsSupervisor;

  bool get canReadAudit => isManager;

  bool get canReadIncidentWork => isManager || isTechnician;

  bool get canTriage => isManager || isTechnician;

  bool get canReadAssets => isManager || isTechnician || isLocationManager;

  bool get canReadJurisdictions =>
      isManager || isTechnician || isLocationManager;

  bool get canReadClaims => isManager || isCitizen;

  bool get canReadReports => isManager || isTechnician;

  factory AppUser.fromJson(Map<String, dynamic> json) {
    return AppUser(
      id: _toInt(json['id']),
      email: json['email'] as String? ?? '',
      fullName: json['nombreCompleto'] as String? ?? '',
      username: json['nombreUsuario'] as String? ?? '',
      roleId: _toInt(json['rolId']),
      role: json['rolNombre'] as String? ?? '',
      isActive: json['activo'] as bool? ?? false,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'email': email,
      'nombreCompleto': fullName,
      'nombreUsuario': username,
      'rolId': roleId,
      'rolNombre': role,
      'activo': isActive,
    };
  }

  static int _toInt(dynamic value) {
    if (value is int) {
      return value;
    }

    return int.tryParse(value?.toString() ?? '') ?? 0;
  }
}
