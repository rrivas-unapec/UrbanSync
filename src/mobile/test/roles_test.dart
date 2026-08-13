import 'package:flutter_test/flutter_test.dart';
import 'package:urbansync/features/auth/domain/app_user.dart';

AppUser _user(String role) => AppUser(
  id: 1,
  email: 'x@y.com',
  fullName: 'Usuario',
  username: 'usuario',
  roleId: 1,
  role: role,
  isActive: true,
);

void main() {
  group('RoleGroup', () {
    test('cada rol del backend cae en su propio grupo', () {
      expect(_user('Ciudadano').roleGroup, RoleGroup.ciudadano);
      expect(_user('GestorUbicacion').roleGroup, RoleGroup.gestorUbicacion);
      expect(_user('GestorEvidencias').roleGroup, RoleGroup.gestorEvidencias);
      expect(_user('AnalistaTecnico').roleGroup, RoleGroup.analistaTecnico);
      expect(
        _user('SupervisorOperaciones').roleGroup,
        RoleGroup.supervisorOperaciones,
      );
      expect(_user('Administrador').roleGroup, RoleGroup.administrador);
    });

    test('los gestores ya no caen en ciudadano', () {
      expect(_user('GestorUbicacion').isCitizen, isFalse);
      expect(_user('GestorEvidencias').isCitizen, isFalse);
    });

    test('tolera acentos, espacios y mayúsculas', () {
      expect(_user('analista técnico').roleGroup, RoleGroup.analistaTecnico);
      expect(_user('  ADMINISTRADOR ').roleGroup, RoleGroup.administrador);
      expect(_user('gestor_ubicacion').roleGroup, RoleGroup.gestorUbicacion);
    });

    test('un rol desconocido no se confunde con ciudadano', () {
      final user = _user('RolNuevoDelBackend');

      expect(user.roleGroup, RoleGroup.desconocido);
      expect(user.isCitizen, isFalse);
      expect(user.isManager, isFalse);
      expect(user.roleLabel, 'RolNuevoDelBackend');
    });
  });

  group('Permisos derivados de los roles de cada controlador', () {
    test('auditoría solo para administración y supervisión', () {
      expect(_user('Administrador').canReadAudit, isTrue);
      expect(_user('SupervisorOperaciones').canReadAudit, isTrue);
      expect(_user('AnalistaTecnico').canReadAudit, isFalse);
      expect(_user('GestorUbicacion').canReadAudit, isFalse);
      expect(_user('Ciudadano').canReadAudit, isFalse);
    });

    test('análisis, trabajos y reportes excluyen al ciudadano', () {
      expect(_user('AnalistaTecnico').canReadIncidentWork, isTrue);
      expect(_user('SupervisorOperaciones').canReadIncidentWork, isTrue);
      expect(_user('Ciudadano').canReadIncidentWork, isFalse);
      expect(_user('GestorEvidencias').canReadIncidentWork, isFalse);
    });

    test('activos y jurisdicciones incluyen al gestor de ubicación', () {
      expect(_user('GestorUbicacion').canReadAssets, isTrue);
      expect(_user('GestorUbicacion').canReadJurisdictions, isTrue);
      expect(_user('Ciudadano').canReadAssets, isFalse);
      expect(_user('GestorEvidencias').canReadAssets, isFalse);
    });

    test('reclamaciones: ciudadano y gestión, no los analistas', () {
      expect(_user('Ciudadano').canReadClaims, isTrue);
      expect(_user('Administrador').canReadClaims, isTrue);
      expect(_user('SupervisorOperaciones').canReadClaims, isTrue);
      expect(_user('AnalistaTecnico').canReadClaims, isFalse);
      expect(_user('GestorUbicacion').canReadClaims, isFalse);
    });

    test('triage para analistas y gestión', () {
      expect(_user('AnalistaTecnico').canTriage, isTrue);
      expect(_user('Administrador').canTriage, isTrue);
      expect(_user('Ciudadano').canTriage, isFalse);
      expect(_user('GestorEvidencias').canTriage, isFalse);
    });
  });

  group('Etiquetas de rol', () {
    test('cada rol muestra un nombre legible', () {
      expect(_user('GestorUbicacion').roleLabel, 'Gestor de Ubicación');
      expect(_user('GestorEvidencias').roleLabel, 'Gestor de Evidencias');
      expect(
        _user('SupervisorOperaciones').roleLabel,
        'Supervisor de Operaciones',
      );
      expect(_user('AnalistaTecnico').roleLabel, 'Analista Técnico');
    });

    test('sin rol asignado se indica explícitamente', () {
      expect(_user('').roleLabel, 'Sin rol');
    });
  });
}
