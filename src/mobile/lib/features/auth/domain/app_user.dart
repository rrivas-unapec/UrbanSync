enum RoleGroup {
  citizen,
  technician,
  manager,
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
        .replaceAll('_', '');

    switch (normalizedRole) {
      case 'admin':
      case 'administrador':
      case 'supervisor':
      case 'supervisoroperaciones':
      case 'gestor':
        return RoleGroup.manager;

      case 'tecnico':
      case 'técnico':
      case 'analista':
      case 'analistatecnico':
      case 'analistatécnico':
        return RoleGroup.technician;

      case 'ciudadano':
      default:
        return RoleGroup.citizen;
    }
  }

  bool get isManager => roleGroup == RoleGroup.manager;

  bool get isTechnician => roleGroup == RoleGroup.technician;

  bool get isCitizen => roleGroup == RoleGroup.citizen;

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