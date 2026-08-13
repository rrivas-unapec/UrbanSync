import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../app/theme.dart';
import '../../../core/network/api_exception.dart';
import '../../../shared/widgets/app_card.dart';
import '../../../shared/widgets/state_views.dart';
import '../../../shared/widgets/status_chip.dart';
import 'incidents_providers.dart';

class AssetsPage extends ConsumerWidget {
  const AssetsPage({super.key, this.showAppBar = true});

  final bool showAppBar;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final async = ref.watch(assetsProvider);

    final body = RefreshIndicator(
      onRefresh: () async => ref.invalidate(assetsProvider),
      child: async.when(
        loading: () => const LoadingView(),
        error: (error, _) => ListView(
          children: [
            const SizedBox(height: 100),
            ErrorView(
              message: error is ApiException
                  ? error.message
                  : 'No se pudieron cargar los activos.',
              onRetry: () => ref.invalidate(assetsProvider),
            ),
          ],
        ),
        data: (assets) {
          if (assets.isEmpty) {
            return ListView(
              children: const [
                SizedBox(height: 100),
                EmptyState(
                  title: 'Sin activos',
                  message: 'No hay activos urbanos registrados.',
                  icon: Icons.lightbulb_outline,
                ),
              ],
            );
          }

          return ListView.builder(
            padding: const EdgeInsets.all(16),
            itemCount: assets.length,
            itemBuilder: (context, index) {
              final asset = assets[index];

              return Padding(
                padding: const EdgeInsets.only(bottom: 12),
                child: AppCard(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        children: [
                          Expanded(
                            child: Text(
                              asset.etiqueta,
                              style: const TextStyle(
                                fontWeight: FontWeight.w600,
                              ),
                            ),
                          ),
                          StatusChip(label: asset.estado),
                        ],
                      ),
                      const SizedBox(height: 6),
                      Text(
                        '${asset.tipo} · ${asset.jurisdiccionNombre}',
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
    );

    if (!showAppBar) return body;

    return Scaffold(
      appBar: AppBar(title: const Text('Activos urbanos')),
      body: body,
    );
  }
}
