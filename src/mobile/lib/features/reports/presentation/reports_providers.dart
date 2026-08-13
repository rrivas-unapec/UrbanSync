import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../incidents/presentation/incidents_providers.dart';
import '../domain/report_summary.dart';

final reportSummaryProvider = FutureProvider.autoDispose<ReportSummary>((
  ref,
) async {
  final incidencias = await ref.watch(allIncidentsProvider.future);
  return ReportSummary.fromIncidents(incidencias);
});
