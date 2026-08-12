import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../app/theme.dart';
import '../../../../core/network/api_exception.dart';
import '../../../../shared/utils/formatters.dart';
import '../../../../shared/widgets/app_card.dart';
import '../../../../shared/widgets/state_views.dart';
import '../../../../shared/widgets/status_chip.dart';
import '../../domain/incident_work.dart';
import '../incident_work_providers.dart';

/// Envuelve las secciones del detalle con los tres estados obligatorios y
/// pull-to-refresh, para no repetirlo en cada pestaña.
class _SectionShell<T> extends StatelessWidget {
  const _SectionShell({
    required this.async,
    required this.onRefresh,
    required this.isEmpty,
    required this.emptyTitle,
    required this.emptyMessage,
    required this.emptyIcon,
    required this.errorMessage,
    required this.builder,
  });

  final AsyncValue<T> async;
  final VoidCallback onRefresh;
  final bool Function(T) isEmpty;
  final String emptyTitle;
  final String emptyMessage;
  final IconData emptyIcon;
  final String errorMessage;
  final Widget Function(T) builder;

  @override
  Widget build(BuildContext context) {
    return RefreshIndicator(
      onRefresh: () async => onRefresh(),
      child: async.when(
        loading: () => const LoadingView(),
        error: (error, _) => ListView(
          children: [
            const SizedBox(height: 80),
            ErrorView(
              message: error is ApiException ? error.message : errorMessage,
              onRetry: onRefresh,
            ),
          ],
        ),
        data: (value) => isEmpty(value)
            ? ListView(
                children: [
                  const SizedBox(height: 80),
                  EmptyState(
                    title: emptyTitle,
                    message: emptyMessage,
                    icon: emptyIcon,
                  ),
                ],
              )
            : ListView(
                padding: const EdgeInsets.all(16),
                children: [builder(value), const SizedBox(height: 80)],
              ),
      ),
    );
  }
}

class IncidentEvidencesSection extends ConsumerWidget {
  const IncidentEvidencesSection({super.key, required this.incidentId});

  final int incidentId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return _SectionShell<List<IncidentEvidence>>(
      async: ref.watch(incidentEvidencesProvider(incidentId)),
      onRefresh: () => ref.invalidate(incidentEvidencesProvider(incidentId)),
      isEmpty: (items) => items.isEmpty,
      emptyTitle: 'Sin evidencias',
      emptyMessage: 'Todavía no se ha registrado evidencia de esta incidencia.',
      emptyIcon: Icons.photo_library_outlined,
      errorMessage: 'No se pudieron cargar las evidencias.',
      builder: (items) => Column(
        children: [
          for (final evidence in items)
            Padding(
              padding: const EdgeInsets.only(bottom: 12),
              child: AppCard(
                child: Row(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Icon(
                      evidence.esImagen
                          ? Icons.image_outlined
                          : Icons.insert_drive_file_outlined,
                      color: AppColors.primary,
                    ),
                    const SizedBox(width: 12),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            evidence.tipoEvidencia,
                            style: const TextStyle(fontWeight: FontWeight.w600),
                          ),
                          if (evidence.descripcion != null &&
                              evidence.descripcion!.isNotEmpty)
                            Padding(
                              padding: const EdgeInsets.only(top: 2),
                              child: Text(evidence.descripcion!),
                            ),
                          const SizedBox(height: 4),
                          Text(
                            evidence.rutaArchivo,
                            style: const TextStyle(
                              fontSize: 11,
                              color: AppColors.mutedForeground,
                            ),
                          ),
                          const SizedBox(height: 4),
                          Text(
                            '${evidence.usuarioSube} · '
                            '${formatDateTime(evidence.fechaSubida)}',
                            style: const TextStyle(
                              fontSize: 12,
                              color: AppColors.mutedForeground,
                            ),
                          ),
                        ],
                      ),
                    ),
                  ],
                ),
              ),
            ),
        ],
      ),
    );
  }
}

class IncidentAnalysisSection extends ConsumerWidget {
  const IncidentAnalysisSection({super.key, required this.incidentId});

  final int incidentId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return _SectionShell<TechnicalAnalysis?>(
      async: ref.watch(incidentAnalysisProvider(incidentId)),
      onRefresh: () => ref.invalidate(incidentAnalysisProvider(incidentId)),
      isEmpty: (analysis) => analysis == null,
      emptyTitle: 'Sin análisis técnico',
      emptyMessage: 'Esta incidencia todavía no tiene diagnóstico registrado.',
      emptyIcon: Icons.science_outlined,
      errorMessage: 'No se pudo cargar el análisis técnico.',
      builder: (analysis) => AppCard(
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('Diagnóstico', style: Theme.of(context).textTheme.titleMedium),
            const SizedBox(height: 6),
            Text(analysis!.diagnostico),
            if (analysis.accionesRecomendadas != null &&
                analysis.accionesRecomendadas!.isNotEmpty) ...[
              const Divider(height: 24),
              Text(
                'Acciones recomendadas',
                style: Theme.of(context).textTheme.titleMedium,
              ),
              const SizedBox(height: 6),
              Text(analysis.accionesRecomendadas!),
            ],
            const Divider(height: 24),
            Text(
              '${analysis.usuarioTecnico} · '
              '${formatDateTime(analysis.fechaAnalisis)}',
              style: const TextStyle(
                fontSize: 12,
                color: AppColors.mutedForeground,
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class IncidentJobsSection extends ConsumerWidget {
  const IncidentJobsSection({super.key, required this.incidentId});

  final int incidentId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return _SectionShell<List<IncidentJob>>(
      async: ref.watch(incidentJobsProvider(incidentId)),
      onRefresh: () => ref.invalidate(incidentJobsProvider(incidentId)),
      isEmpty: (items) => items.isEmpty,
      emptyTitle: 'Sin trabajos',
      emptyMessage: 'No se ha asignado ningún trabajo a esta incidencia.',
      emptyIcon: Icons.build_outlined,
      errorMessage: 'No se pudieron cargar los trabajos.',
      builder: (items) => Column(
        children: [
          for (final job in items)
            Padding(
              padding: const EdgeInsets.only(bottom: 12),
              child: AppCard(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        Expanded(
                          child: Text(
                            'Trabajo #${job.id}',
                            style: const TextStyle(fontWeight: FontWeight.w600),
                          ),
                        ),
                        StatusChip(label: job.estado),
                      ],
                    ),
                    const SizedBox(height: 6),
                    Text(job.descripcionTrabajo),
                    const SizedBox(height: 8),
                    Text(
                      'Asignado a ${job.usuarioAsignado}',
                      style: const TextStyle(
                        fontSize: 12,
                        color: AppColors.mutedForeground,
                      ),
                    ),
                    if (job.fechaInicio != null)
                      Text(
                        'Inicio: ${formatDateTime(job.fechaInicio!)}',
                        style: const TextStyle(
                          fontSize: 12,
                          color: AppColors.mutedForeground,
                        ),
                      ),
                    if (job.fechaFin != null)
                      Text(
                        'Fin: ${formatDateTime(job.fechaFin!)}',
                        style: const TextStyle(
                          fontSize: 12,
                          color: AppColors.mutedForeground,
                        ),
                      ),
                    if (job.resultado != null && job.resultado!.isNotEmpty) ...[
                      const Divider(height: 20),
                      Text('Resultado: ${job.resultado!}'),
                    ],
                  ],
                ),
              ),
            ),
        ],
      ),
    );
  }
}

class IncidentReportsSection extends ConsumerWidget {
  const IncidentReportsSection({super.key, required this.incidentId});

  final int incidentId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return _SectionShell<List<IncidentReport>>(
      async: ref.watch(incidentReportsProvider(incidentId)),
      onRefresh: () => ref.invalidate(incidentReportsProvider(incidentId)),
      isEmpty: (items) => items.isEmpty,
      emptyTitle: 'Sin reportes',
      emptyMessage: 'Todavía no se ha generado un reporte de esta incidencia.',
      emptyIcon: Icons.description_outlined,
      errorMessage: 'No se pudieron cargar los reportes.',
      builder: (items) => Column(
        children: [
          for (final report in items)
            Padding(
              padding: const EdgeInsets.only(bottom: 12),
              child: AppCard(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'Reporte #${report.id}'
                      '${report.trabajoId == null ? '' : ' · Trabajo #${report.trabajoId}'}',
                      style: const TextStyle(fontWeight: FontWeight.w600),
                    ),
                    if (report.contenido != null &&
                        report.contenido!.isNotEmpty) ...[
                      const SizedBox(height: 6),
                      Text(report.contenido!),
                    ],
                    if (report.rutaArchivo != null &&
                        report.rutaArchivo!.isNotEmpty) ...[
                      const SizedBox(height: 6),
                      Text(
                        report.rutaArchivo!,
                        style: const TextStyle(
                          fontSize: 11,
                          color: AppColors.mutedForeground,
                        ),
                      ),
                    ],
                    const SizedBox(height: 8),
                    Text(
                      '${report.generadoPor} · '
                      '${formatDateTime(report.fechaGeneracion)}',
                      style: const TextStyle(
                        fontSize: 12,
                        color: AppColors.mutedForeground,
                      ),
                    ),
                  ],
                ),
              ),
            ),
        ],
      ),
    );
  }
}
