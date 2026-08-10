import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../data/incidents_repository.dart';
import '../domain/catalog.dart';
import '../domain/incident.dart';
import '../domain/urban_asset.dart';

final incidentTypesProvider = FutureProvider.autoDispose<List<IncidentType>>(
  (ref) => ref.read(incidentsRepositoryProvider).incidentTypes(),
);

final assetsProvider = FutureProvider.autoDispose<List<UrbanAsset>>(
  (ref) => ref.read(incidentsRepositoryProvider).assets(),
);

final assetHistoryProvider = FutureProvider.autoDispose
    .family<List<AssetHistoryEntry>, int>(
      (ref, assetId) =>
          ref.read(incidentsRepositoryProvider).assetHistory(assetId),
    );

final jurisdictionsProvider = FutureProvider.autoDispose<List<Jurisdiction>>(
  (ref) => ref.read(incidentsRepositoryProvider).jurisdictions(),
);

final myIncidentsProvider = FutureProvider.autoDispose<List<Incident>>(
  (ref) => ref.read(incidentsRepositoryProvider).list(mine: true),
);

final allIncidentsProvider = FutureProvider.autoDispose<List<Incident>>(
  (ref) => ref.read(incidentsRepositoryProvider).list(),
);

final triageQueueProvider = FutureProvider.autoDispose<List<Incident>>(
  (ref) => ref.read(incidentsRepositoryProvider).list(status: 'Registrada'),
);

final technicianJobsProvider = FutureProvider.autoDispose<List<Incident>>((
  ref,
) async {
  final all = await ref.read(incidentsRepositoryProvider).list();
  return all
      .where((i) => i.estado == 'Asignada' || i.estado == 'EnProceso')
      .toList();
});

final incidentDetailProvider = FutureProvider.autoDispose.family<Incident, int>(
  (ref, id) => ref.read(incidentsRepositoryProvider).getById(id),
);
