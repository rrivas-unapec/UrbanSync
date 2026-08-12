import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:image_picker/image_picker.dart';
import 'package:latlong2/latlong.dart';

import '../../../app/theme.dart';
import '../../../core/network/api_exception.dart';
import '../../../shared/utils/formatters.dart';
import '../../../shared/widgets/app_card.dart';
import '../../../shared/widgets/buttons.dart';
import '../../../shared/widgets/state_views.dart';
import '../../../shared/widgets/status_chip.dart';
import '../../audit/presentation/audit_providers.dart';
import '../../audit/presentation/incident_audit_section.dart';
import '../../auth/domain/app_user.dart';
import '../../auth/presentation/auth_controller.dart';
import '../data/incident_work_repository.dart';
import '../data/incidents_repository.dart';
import '../domain/incident.dart';
import 'incident_work_providers.dart';
import 'incidents_providers.dart';
import 'widgets/incident_work_sections.dart';

class _DetailTab {
  const _DetailTab(this.label, this.icon, this.body);

  final String label;
  final IconData icon;
  final Widget body;
}

class IncidentDetailPage extends ConsumerStatefulWidget {
  const IncidentDetailPage({super.key, required this.incidentId});

  final int incidentId;

  @override
  ConsumerState<IncidentDetailPage> createState() => _IncidentDetailPageState();
}

class _IncidentDetailPageState extends ConsumerState<IncidentDetailPage> {
  bool _busy = false;

  Future<void> _updateStatus(String estado) async {
    setState(() => _busy = true);
    try {
      await ref
          .read(incidentsRepositoryProvider)
          .updateStatus(widget.incidentId, estado);
      _refresh();
      _toast('Estado actualizado a $estado.', AppColors.secondary);
    } on ApiException catch (error) {
      _toast(error.message, AppColors.destructive);
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  Future<void> _addEvidence() async {
    final tipo = await showModalBottomSheet<String>(
      context: context,
      builder: (context) => SafeArea(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            for (final option in const ['Antes', 'Despues', 'Documento'])
              ListTile(
                leading: const Icon(Icons.label_outline),
                title: Text('Evidencia: $option'),
                onTap: () => Navigator.pop(context, option),
              ),
          ],
        ),
      ),
    );
    if (tipo == null) return;

    final userId = ref.read(authControllerProvider).user?.id;
    if (userId == null) return;

    final picked = await ImagePicker().pickImage(
      source: ImageSource.camera,
      imageQuality: 70,
      maxWidth: 1600,
    );
    if (picked == null || !mounted) return;

    setState(() => _busy = true);
    try {
      await ref
          .read(incidentWorkRepositoryProvider)
          .createEvidence(
            incidenciaId: widget.incidentId,
            tipoEvidencia: tipo,
            rutaArchivo: picked.path,
            usuarioSubeId: userId,
          );
      _refresh();
      _toast('Evidencia registrada.', AppColors.secondary);
    } on ApiException catch (error) {
      _toast(error.message, AppColors.destructive);
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  void _refresh() {
    ref.invalidate(incidentDetailProvider(widget.incidentId));
    ref.invalidate(incidentAuditProvider(widget.incidentId));
    ref.invalidate(incidentEvidencesProvider(widget.incidentId));
    ref.invalidate(incidentAnalysisProvider(widget.incidentId));
    ref.invalidate(incidentJobsProvider(widget.incidentId));
    ref.invalidate(incidentReportsProvider(widget.incidentId));
  }

  void _toast(String message, Color color) {
    if (!mounted) return;
    ScaffoldMessenger.of(
      context,
    ).showSnackBar(SnackBar(content: Text(message), backgroundColor: color));
  }

  @override
  Widget build(BuildContext context) {
    final async = ref.watch(incidentDetailProvider(widget.incidentId));
    final user = ref.watch(authControllerProvider).user;

    final body = async.when(
      loading: () => const LoadingView(),
      error: (error, _) => ErrorView(
        message: error is ApiException
            ? error.message
            : 'No se pudo cargar la incidencia.',
        onRetry: _refresh,
      ),
      data: (incident) => _content(incident, user),
    );

    final tabs = _tabsFor(user, body);

    if (tabs.length == 1) {
      return Scaffold(
        appBar: AppBar(title: const Text('Detalle de incidencia')),
        body: body,
        bottomNavigationBar: user == null
            ? null
            : _actionBar(async.value, user),
      );
    }

    return DefaultTabController(
      length: tabs.length,
      child: Scaffold(
        appBar: AppBar(
          title: const Text('Detalle de incidencia'),
          bottom: TabBar(
            isScrollable: true,
            tabs: [
              for (final tab in tabs)
                Tab(text: tab.label, icon: Icon(tab.icon, size: 20)),
            ],
          ),
        ),
        body: TabBarView(children: [for (final tab in tabs) tab.body]),
        bottomNavigationBar: user == null
            ? null
            : _actionBar(async.value, user),
      ),
    );
  }

  /// Las pestañas siguen los roles de lectura del API: evidencias las ve
  /// cualquiera, análisis/trabajos/reportes solo gestión y técnicos, y la
  /// auditoría solo gestión.
  List<_DetailTab> _tabsFor(AppUser? user, Widget info) {
    final id = widget.incidentId;

    final tabs = <_DetailTab>[
      _DetailTab('Info', Icons.info_outline, info),
      _DetailTab(
        'Evidencias',
        Icons.photo_library_outlined,
        IncidentEvidencesSection(incidentId: id),
      ),
    ];

    if (user == null) return tabs;

    if (user.isManager || user.isTechnician) {
      tabs.addAll([
        _DetailTab(
          'Análisis',
          Icons.science_outlined,
          IncidentAnalysisSection(incidentId: id),
        ),
        _DetailTab(
          'Trabajos',
          Icons.build_outlined,
          IncidentJobsSection(incidentId: id),
        ),
        _DetailTab(
          'Reportes',
          Icons.description_outlined,
          IncidentReportsSection(incidentId: id),
        ),
      ]);
    }

    if (user.isManager) {
      tabs.add(
        _DetailTab(
          'Auditoría',
          Icons.history,
          IncidentAuditSection(incidentId: id),
        ),
      );
    }

    return tabs;
  }

  Widget _content(Incident incident, AppUser? user) {
    return RefreshIndicator(
      onRefresh: () async {
        _refresh();
        await ref.read(incidentDetailProvider(widget.incidentId).future);
      },
      child: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          Row(
            children: [
              Expanded(
                child: Text(
                  incident.codigoCaso,
                  style: Theme.of(context).textTheme.titleLarge,
                ),
              ),
              StatusChip(label: incident.estado),
            ],
          ),
          const SizedBox(height: 8),
          Wrap(
            spacing: 8,
            runSpacing: 8,
            children: [
              StatusChip(label: incident.prioridad, kind: ChipKind.prioridad),
              _tag(Icons.category_outlined, incident.tipoIncidencia),
              _tag(Icons.account_balance_outlined, incident.jurisdiccion),
            ],
          ),
          const SizedBox(height: 16),
          AppCard(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  'Descripción',
                  style: Theme.of(context).textTheme.titleMedium,
                ),
                const SizedBox(height: 6),
                Text(incident.descripcion),
                const Divider(height: 24),
                _infoRow(Icons.place_outlined, 'Dirección', incident.direccion),
                if (incident.referencia != null &&
                    incident.referencia!.isNotEmpty)
                  _infoRow(
                    Icons.signpost_outlined,
                    'Referencia',
                    incident.referencia!,
                  ),
                if (incident.institucionAsignada != null)
                  _infoRow(
                    Icons.business_outlined,
                    'Institución derivada',
                    incident.institucionAsignada!,
                  ),
                _infoRow(
                  Icons.person_outline,
                  'Reportado por',
                  incident.usuarioReporta,
                ),
              ],
            ),
          ),
          const SizedBox(height: 16),
          if (incident.latitud != null && incident.longitud != null) ...[
            _miniMap(LatLng(incident.latitud!, incident.longitud!)),
            const SizedBox(height: 16),
          ],
          _timelineCard(incident),
          const SizedBox(height: 80),
        ],
      ),
    );
  }

  Widget _timelineCard(Incident incident) {
    return AppCard(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            'Línea de tiempo',
            style: Theme.of(context).textTheme.titleMedium,
          ),
          const SizedBox(height: 12),
          _timelineRow(
            'Reportada',
            formatDateTime(incident.fechaReporte),
            true,
          ),
          _timelineRow(
            'Asignada',
            incident.fechaAsignacion == null
                ? 'Pendiente'
                : formatDateTime(incident.fechaAsignacion!),
            incident.fechaAsignacion != null,
          ),
          _timelineRow(
            'Cerrada',
            incident.fechaCierre == null
                ? 'Pendiente'
                : formatDateTime(incident.fechaCierre!),
            incident.fechaCierre != null,
            isLast: true,
          ),
        ],
      ),
    );
  }

  Widget _timelineRow(
    String label,
    String value,
    bool done, {
    bool isLast = false,
  }) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Column(
          children: [
            Icon(
              done ? Icons.check_circle : Icons.radio_button_unchecked,
              size: 18,
              color: done ? AppColors.secondary : AppColors.mutedForeground,
            ),
            if (!isLast)
              Container(width: 2, height: 24, color: AppColors.muted),
          ],
        ),
        const SizedBox(width: 12),
        Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(label, style: const TextStyle(fontWeight: FontWeight.w600)),
            Text(
              value,
              style: const TextStyle(
                color: AppColors.mutedForeground,
                fontSize: 12,
              ),
            ),
            const SizedBox(height: 8),
          ],
        ),
      ],
    );
  }

  Widget _miniMap(LatLng point) {
    return ClipRRect(
      borderRadius: BorderRadius.circular(12),
      child: SizedBox(
        height: 160,
        child: FlutterMap(
          options: MapOptions(
            initialCenter: point,
            initialZoom: 15,
            interactionOptions: const InteractionOptions(
              flags: InteractiveFlag.none,
            ),
          ),
          children: [
            TileLayer(
              urlTemplate: 'https://tile.openstreetmap.org/{z}/{x}/{y}.png',
              userAgentPackageName: 'com.urbansync.urbansync',
            ),
            MarkerLayer(
              markers: [
                Marker(
                  point: point,
                  width: 40,
                  height: 40,
                  child: const Icon(
                    Icons.location_on,
                    color: AppColors.destructive,
                    size: 40,
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  Widget? _actionBar(Incident? incident, AppUser user) {
    if (incident == null) return null;

    final actions = <Widget>[];

    if (user.isManager &&
        (incident.estado == 'Registrada' || incident.estado == 'EnAnalisis')) {
      actions.add(
        Expanded(
          child: PrimaryButton(
            label: 'Analizar',
            icon: Icons.fact_check_outlined,
            onPressed: () => context.push('/triage/${incident.id}'),
          ),
        ),
      );
    }

    if (user.isTechnician) {
      if (incident.estado == 'Asignada') {
        actions.add(
          Expanded(
            child: PrimaryButton(
              label: 'Iniciar',
              loading: _busy,
              icon: Icons.play_arrow,
              onPressed: () => _updateStatus('EnProceso'),
            ),
          ),
        );
      } else if (incident.estado == 'EnProceso') {
        actions.add(
          Expanded(
            child: PrimaryButton(
              label: 'Completar',
              loading: _busy,
              icon: Icons.check,
              onPressed: () => _updateStatus('Cerrada'),
            ),
          ),
        );
      }
    }

    if (user.isTechnician || user.isManager) {
      actions.add(
        Expanded(
          child: SecondaryButton(
            label: 'Evidencia',
            icon: Icons.add_a_photo_outlined,
            onPressed: _busy ? null : _addEvidence,
          ),
        ),
      );
    }

    if (actions.isEmpty) return null;

    return SafeArea(
      child: Padding(
        padding: const EdgeInsets.all(12),
        child: Row(
          children: [
            for (var i = 0; i < actions.length; i++) ...[
              if (i > 0) const SizedBox(width: 12),
              actions[i],
            ],
          ],
        ),
      ),
    );
  }

  Widget _tag(IconData icon, String label) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
      decoration: BoxDecoration(
        color: AppColors.muted,
        borderRadius: BorderRadius.circular(999),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 14, color: AppColors.mutedForeground),
          const SizedBox(width: 4),
          Text(label, style: const TextStyle(fontSize: 12)),
        ],
      ),
    );
  }

  Widget _infoRow(IconData icon, String label, String value) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 6),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Icon(icon, size: 18, color: AppColors.mutedForeground),
          const SizedBox(width: 10),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  label,
                  style: const TextStyle(
                    color: AppColors.mutedForeground,
                    fontSize: 12,
                  ),
                ),
                Text(value),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
