import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../shared/widgets/state_views.dart';
import '../../auth/domain/app_user.dart';
import '../../auth/presentation/auth_controller.dart';
import '../../claims/presentation/claims_page.dart';
import '../../incidents/presentation/assets_page.dart';
import '../../incidents/presentation/incidents_providers.dart';
import '../../incidents/presentation/widgets/incident_list_section.dart';
import '../../profile/presentation/profile_page.dart';
import '../../reports/presentation/dashboard_page.dart';

class _HomeTab {
  const _HomeTab({
    required this.icon,
    required this.label,
    required this.title,
    required this.body,
  });

  final IconData icon;
  final String label;
  final String title;
  final Widget body;
}

class HomePage extends ConsumerStatefulWidget {
  const HomePage({super.key});

  @override
  ConsumerState<HomePage> createState() => _HomePageState();
}

class _HomePageState extends ConsumerState<HomePage> {
  int _index = 0;

  static const _HomeTab _perfil = _HomeTab(
    icon: Icons.person_outline,
    label: 'Perfil',
    title: 'Mi perfil',
    body: ProfilePage(),
  );

  @override
  Widget build(BuildContext context) {
    final user = ref.watch(authControllerProvider).user;

    if (user == null) {
      return const Scaffold(body: Center(child: CircularProgressIndicator()));
    }

    final tabs = _tabsFor(user);
    final safeIndex = _index.clamp(0, tabs.length - 1);
    final showReportFab = user.isCitizen && safeIndex == 0;

    return Scaffold(
      appBar: AppBar(title: Text(tabs[safeIndex].title)),
      body: IndexedStack(
        index: safeIndex,
        children: tabs.map((tab) => tab.body).toList(),
      ),
      floatingActionButton: showReportFab
          ? FloatingActionButton.extended(
              onPressed: () => context.push('/report'),
              icon: const Icon(Icons.add_a_photo_outlined),
              label: const Text('Reportar'),
            )
          : null,
      bottomNavigationBar: tabs.length < 2
          ? null
          : NavigationBar(
              selectedIndex: safeIndex,
              onDestinationSelected: (value) => setState(() => _index = value),
              destinations: tabs
                  .map(
                    (tab) => NavigationDestination(
                      icon: Icon(tab.icon),
                      label: tab.label,
                    ),
                  )
                  .toList(),
            ),
    );
  }

  List<_HomeTab> _tabsFor(AppUser user) {
    switch (user.roleGroup) {
      case RoleGroup.ciudadano:
        return [
          _HomeTab(
            icon: Icons.list_alt_outlined,
            label: 'Incidencias',
            title: 'Mis incidencias',
            body: IncidentListSection(
              provider: myIncidentsProvider,
              emptyTitle: 'Aún no has reportado incidencias',
              emptyMessage:
                  'Usa el botón "Reportar" para crear tu primer reporte.',
            ),
          ),
          _HomeTab(
            icon: Icons.support_agent_outlined,
            label: 'Reclamaciones',
            title: 'Mis reclamaciones',
            body: ClaimsPage(showAppBar: false),
          ),
          _perfil,
        ];

      case RoleGroup.analistaTecnico:
        return [
          _HomeTab(
            icon: Icons.assignment_outlined,
            label: 'Triage',
            title: 'Cola de análisis',
            body: IncidentListSection(
              provider: triageQueueProvider,
              emptyTitle: 'No hay incidencias nuevas',
              emptyMessage:
                  'Las incidencias registradas aparecerán aquí para su análisis.',
              routePrefix: '/triage',
            ),
          ),
          _HomeTab(
            icon: Icons.lightbulb_outline,
            label: 'Activos',
            title: 'Activos urbanos',
            body: AssetsPage(showAppBar: false),
          ),
          _HomeTab(
            icon: Icons.insights_outlined,
            label: 'Indicadores',
            title: 'Indicadores',
            body: DashboardPage(),
          ),
          _perfil,
        ];

      case RoleGroup.gestorUbicacion:
        return [
          _HomeTab(
            icon: Icons.map_outlined,
            label: 'Incidencias',
            title: 'Incidencias',
            body: IncidentListSection(
              provider: allIncidentsProvider,
              emptyTitle: 'Sin incidencias',
              emptyMessage: 'Aquí aparecerán las incidencias registradas.',
            ),
          ),
          _HomeTab(
            icon: Icons.lightbulb_outline,
            label: 'Activos',
            title: 'Activos urbanos',
            body: AssetsPage(showAppBar: false),
          ),
          _perfil,
        ];

      case RoleGroup.gestorEvidencias:
        return [
          _HomeTab(
            icon: Icons.photo_library_outlined,
            label: 'Evidencias',
            title: 'Bandeja de evidencias',
            body: IncidentListSection(
              provider: allIncidentsProvider,
              emptyTitle: 'Sin incidencias por revisar',
              emptyMessage:
                  'Abre una incidencia para consultar o agregar evidencia.',
            ),
          ),
          _perfil,
        ];

      case RoleGroup.supervisorOperaciones:
      case RoleGroup.administrador:
        return [
          _HomeTab(
            icon: Icons.dashboard_outlined,
            label: 'Panel',
            title: 'Panel de gestión',
            body: IncidentListSection(
              provider: allIncidentsProvider,
              emptyTitle: 'Sin incidencias',
            ),
          ),
          _HomeTab(
            icon: Icons.assignment_outlined,
            label: 'Triage',
            title: 'Cola de análisis',
            body: IncidentListSection(
              provider: triageQueueProvider,
              emptyTitle: 'No hay incidencias nuevas',
              emptyMessage:
                  'Las incidencias registradas aparecerán aquí para su análisis.',
              routePrefix: '/triage',
            ),
          ),
          _HomeTab(
            icon: Icons.support_agent_outlined,
            label: 'Reclamaciones',
            title: 'Reclamaciones',
            body: ClaimsPage(showAppBar: false),
          ),
          _HomeTab(
            icon: Icons.insights_outlined,
            label: 'Indicadores',
            title: 'Indicadores',
            body: DashboardPage(),
          ),
          _perfil,
        ];

      case RoleGroup.desconocido:
        return [
          _HomeTab(
            icon: Icons.help_outline,
            label: 'Inicio',
            title: 'Rol no reconocido',
            body: EmptyState(
              title: 'Tu rol no está configurado',
              message:
                  'El rol "${user.role}" no tiene una experiencia asignada en '
                  'la app. Contacta al administrador.',
              icon: Icons.help_outline,
            ),
          ),
          _perfil,
        ];
    }
  }
}
