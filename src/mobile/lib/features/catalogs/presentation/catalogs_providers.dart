import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../data/catalogs_repository.dart';
import '../domain/catalogs.dart';

final jurisdictionsCatalogProvider =
    FutureProvider.autoDispose<List<JurisdictionItem>>(
      (ref) => ref.read(catalogsRepositoryProvider).jurisdictions(),
    );

final departmentsProvider = FutureProvider.autoDispose<List<Department>>(
  (ref) => ref.read(catalogsRepositoryProvider).departments(),
);

final institutionsProvider = FutureProvider.autoDispose<List<InstitutionItem>>(
  (ref) => ref.read(catalogsRepositoryProvider).institutions(),
);

final locationsProvider = FutureProvider.autoDispose<List<LocationItem>>(
  (ref) => ref.read(catalogsRepositoryProvider).locations(),
);
