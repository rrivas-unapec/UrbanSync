import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../auth/presentation/auth_controller.dart';
import '../data/claims_repository.dart';
import '../domain/claim.dart';

/// El ciudadano solo puede leer las suyas; gestión ve todas.
final claimsProvider = FutureProvider.autoDispose<List<Claim>>((ref) {
  final user = ref.watch(authControllerProvider).user;
  final repository = ref.read(claimsRepositoryProvider);

  if (user == null) return Future.value(const []);

  return user.isCitizen ? repository.byCitizen(user.id) : repository.all();
});

final claimDetailProvider = FutureProvider.autoDispose.family<Claim, int>(
  (ref, id) => ref.read(claimsRepositoryProvider).getById(id),
);
