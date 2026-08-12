import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../app/theme.dart';
import '../../../core/network/api_exception.dart';
import '../../../shared/utils/formatters.dart';
import '../../../shared/widgets/app_card.dart';
import '../../../shared/widgets/state_views.dart';
import '../../../shared/widgets/status_chip.dart';
import '../../auth/presentation/auth_controller.dart';
import '../data/claims_repository.dart';
import '../domain/claim.dart';
import 'claims_providers.dart';

const _estados = ['Abierta', 'EnProceso', 'Cerrada', 'Rechazada'];

class ClaimDetailPage extends ConsumerStatefulWidget {
  const ClaimDetailPage({super.key, required this.claimId});

  final int claimId;

  @override
  ConsumerState<ClaimDetailPage> createState() => _ClaimDetailPageState();
}

class _ClaimDetailPageState extends ConsumerState<ClaimDetailPage> {
  bool _busy = false;

  Future<void> _cambiarEstado(String estado) async {
    setState(() => _busy = true);
    try {
      await ref
          .read(claimsRepositoryProvider)
          .updateStatus(widget.claimId, estado);
      ref.invalidate(claimDetailProvider(widget.claimId));
      ref.invalidate(claimsProvider);
      _toast('Estado actualizado a $estado.', AppColors.secondary);
    } on ApiException catch (error) {
      _toast(error.message, AppColors.destructive);
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  void _toast(String message, Color color) {
    if (!mounted) return;
    ScaffoldMessenger.of(
      context,
    ).showSnackBar(SnackBar(content: Text(message), backgroundColor: color));
  }

  @override
  Widget build(BuildContext context) {
    final async = ref.watch(claimDetailProvider(widget.claimId));
    final puedeGestionar =
        ref.watch(authControllerProvider).user?.isManager ?? false;

    return Scaffold(
      appBar: AppBar(title: const Text('Reclamación')),
      body: async.when(
        loading: () => const LoadingView(),
        error: (error, _) => ErrorView(
          message: error is ApiException
              ? error.message
              : 'No se pudo cargar la reclamación.',
          onRetry: () => ref.invalidate(claimDetailProvider(widget.claimId)),
        ),
        data: (claim) => _content(claim, puedeGestionar),
      ),
    );
  }

  Widget _content(Claim claim, bool puedeGestionar) {
    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        Row(
          children: [
            Expanded(
              child: Text(
                claim.titulo,
                style: Theme.of(context).textTheme.titleLarge,
              ),
            ),
            StatusChip(label: claim.estado),
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
              Text(claim.descripcion),
              const Divider(height: 24),
              _row(Icons.category_outlined, 'Categoría', claim.categoria),
              _row(Icons.place_outlined, 'Ubicación', claim.ubicacionDireccion),
              _row(Icons.person_outline, 'Ciudadano', claim.ciudadano),
              _row(
                Icons.event_outlined,
                'Creada',
                formatDateTime(claim.fechaCreacion),
              ),
            ],
          ),
        ),
        if (puedeGestionar) ...[
          const SizedBox(height: 24),
          Text(
            'Cambiar estado',
            style: Theme.of(context).textTheme.titleMedium,
          ),
          const SizedBox(height: 8),
          Wrap(
            spacing: 8,
            runSpacing: 8,
            children: [
              for (final estado in _estados)
                if (estado != claim.estado)
                  OutlinedButton(
                    onPressed: _busy ? null : () => _cambiarEstado(estado),
                    child: Text(estado),
                  ),
            ],
          ),
        ],
      ],
    );
  }

  Widget _row(IconData icon, String label, String value) {
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
                Text(value.isEmpty ? '—' : value),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
