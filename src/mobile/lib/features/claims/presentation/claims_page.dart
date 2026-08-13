import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../app/theme.dart';
import '../../../core/network/api_exception.dart';
import '../../../shared/utils/formatters.dart';
import '../../../shared/widgets/app_card.dart';
import '../../../shared/widgets/state_views.dart';
import '../../../shared/widgets/status_chip.dart';
import '../../auth/presentation/auth_controller.dart';
import 'claims_providers.dart';

class ClaimsPage extends ConsumerWidget {
  const ClaimsPage({super.key, this.showAppBar = true});

  final bool showAppBar;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final async = ref.watch(claimsProvider);
    final user = ref.watch(authControllerProvider).user;

    return Scaffold(
      appBar: showAppBar
          ? AppBar(
              title: Text(
                user?.isCitizen ?? false
                    ? 'Mis reclamaciones'
                    : 'Reclamaciones',
              ),
            )
          : null,
      floatingActionButton: (user?.isCitizen ?? false)
          ? FloatingActionButton.extended(
              onPressed: () => context.push('/claims/new'),
              icon: const Icon(Icons.add),
              label: const Text('Nueva'),
            )
          : null,
      body: RefreshIndicator(
        onRefresh: () async => ref.invalidate(claimsProvider),
        child: async.when(
          loading: () => const LoadingView(),
          error: (error, _) => ListView(
            children: [
              const SizedBox(height: 100),
              ErrorView(
                message: error is ApiException
                    ? error.message
                    : 'No se pudieron cargar las reclamaciones.',
                onRetry: () => ref.invalidate(claimsProvider),
              ),
            ],
          ),
          data: (claims) {
            if (claims.isEmpty) {
              return ListView(
                children: const [
                  SizedBox(height: 100),
                  EmptyState(
                    title: 'Sin reclamaciones',
                    message: 'Todavía no hay reclamaciones registradas.',
                    icon: Icons.support_agent_outlined,
                  ),
                ],
              );
            }

            return ListView.builder(
              padding: const EdgeInsets.all(16),
              itemCount: claims.length,
              itemBuilder: (context, index) {
                final claim = claims[index];

                return Padding(
                  padding: const EdgeInsets.only(bottom: 12),
                  child: AppCard(
                    onTap: () => context.push('/claims/${claim.id}'),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          children: [
                            Expanded(
                              child: Text(
                                claim.titulo,
                                style: const TextStyle(
                                  fontWeight: FontWeight.w600,
                                ),
                              ),
                            ),
                            StatusChip(label: claim.estado),
                          ],
                        ),
                        const SizedBox(height: 6),
                        Text(
                          claim.descripcion,
                          maxLines: 2,
                          overflow: TextOverflow.ellipsis,
                        ),
                        const SizedBox(height: 8),
                        Text(
                          '${claim.categoria} · ${claim.ubicacionDireccion}',
                          style: const TextStyle(
                            fontSize: 12,
                            color: AppColors.mutedForeground,
                          ),
                        ),
                        Text(
                          formatDateTime(claim.fechaCreacion),
                          style: const TextStyle(
                            fontSize: 12,
                            color: AppColors.mutedForeground,
                          ),
                        ),
                      ],
                    ),
                  ),
                );
              },
            );
          },
        ),
      ),
    );
  }
}
