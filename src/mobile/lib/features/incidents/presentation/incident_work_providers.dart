import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../data/incident_work_repository.dart';
import '../domain/incident_work.dart';

final incidentEvidencesProvider = FutureProvider.autoDispose
    .family<List<IncidentEvidence>, int>(
      (ref, incidentId) =>
          ref.read(incidentWorkRepositoryProvider).evidences(incidentId),
    );

final incidentAnalysisProvider = FutureProvider.autoDispose
    .family<TechnicalAnalysis?, int>(
      (ref, incidentId) => ref
          .read(incidentWorkRepositoryProvider)
          .technicalAnalysis(incidentId),
    );

final incidentJobsProvider = FutureProvider.autoDispose
    .family<List<IncidentJob>, int>(
      (ref, incidentId) =>
          ref.read(incidentWorkRepositoryProvider).jobs(incidentId),
    );

final incidentReportsProvider = FutureProvider.autoDispose
    .family<List<IncidentReport>, int>(
      (ref, incidentId) =>
          ref.read(incidentWorkRepositoryProvider).reports(incidentId),
    );
