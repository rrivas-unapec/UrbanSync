import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../app/theme.dart';
import '../../../shared/widgets/app_card.dart';
import '../../../shared/widgets/buttons.dart';
import '../../auth/presentation/auth_controller.dart';

class ProfilePage extends ConsumerWidget {
  const ProfilePage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final user = ref.watch(authControllerProvider).user;

    if (user == null) {
      return const SizedBox.shrink();
    }

    return SingleChildScrollView(
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Center(
            child: CircleAvatar(
              radius: 40,
              backgroundColor: AppColors.primary.withValues(alpha: 0.12),
              child: const Icon(
                Icons.person,
                size: 44,
                color: AppColors.primary,
              ),
            ),
          ),
          const SizedBox(height: 16),
          Text(
            user.fullName,
            textAlign: TextAlign.center,
            style: Theme.of(context).textTheme.titleLarge,
          ),
          const SizedBox(height: 4),
          Text(
            user.role,
            textAlign: TextAlign.center,
            style: const TextStyle(color: AppColors.mutedForeground),
          ),
          const SizedBox(height: 24),
          AppCard(
            child: Column(
              children: [
                _InfoRow(
                  icon: Icons.email_outlined,
                  label: 'Correo',
                  value: user.email,
                ),
                const Divider(height: 20),
                _InfoRow(
                  icon: Icons.account_circle_outlined,
                  label: 'Nombre de usuario',
                  value: user.username,
                ),
                const Divider(height: 20),
                _InfoRow(
                  icon: Icons.admin_panel_settings_outlined,
                  label: 'Rol',
                  value: user.role,
                ),
                const Divider(height: 20),
                _InfoRow(
                  icon: user.isActive
                      ? Icons.check_circle_outline
                      : Icons.cancel_outlined,
                  label: 'Estado',
                  value: user.isActive ? 'Activo' : 'Inactivo',
                ),
              ],
            ),
          ),
          const SizedBox(height: 24),
          if (!user.isTechnician) ...[
            SecondaryButton(
              label: user.isCitizen ? 'Mis reclamaciones' : 'Reclamaciones',
              icon: Icons.support_agent_outlined,
              onPressed: () {
                context.push('/claims');
              },
            ),
            const SizedBox(height: 12),
          ],
          if (!user.isCitizen) ...[
            SecondaryButton(
              label: 'Catálogos',
              icon: Icons.folder_outlined,
              onPressed: () {
                context.push('/catalogs');
              },
            ),
            const SizedBox(height: 12),
          ],
          SecondaryButton(
            label: 'Cambiar contraseña',
            icon: Icons.lock_reset_outlined,
            onPressed: () {
              context.push('/change-password');
            },
          ),
          const SizedBox(height: 12),
          SecondaryButton(
            label: 'Cerrar sesión',
            icon: Icons.logout,
            onPressed: () {
              ref.read(authControllerProvider.notifier).logout();
            },
          ),
        ],
      ),
    );
  }
}

class _InfoRow extends StatelessWidget {
  const _InfoRow({
    required this.icon,
    required this.label,
    required this.value,
  });

  final IconData icon;
  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Icon(icon, size: 20, color: AppColors.mutedForeground),
        const SizedBox(width: 12),
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
              const SizedBox(height: 2),
              Text(
                value.isEmpty ? '—' : value,
                style: const TextStyle(fontWeight: FontWeight.w500),
              ),
            ],
          ),
        ),
      ],
    );
  }
}
