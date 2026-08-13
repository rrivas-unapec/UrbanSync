import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../app/theme.dart';
import '../../../core/network/api_exception.dart';
import '../../../shared/utils/validators.dart';
import '../../../shared/widgets/app_text_field.dart';
import '../../../shared/widgets/buttons.dart';
import '../../../shared/widgets/state_views.dart';
import '../../auth/presentation/auth_controller.dart';
import '../../catalogs/domain/catalogs.dart';
import '../../catalogs/presentation/catalogs_providers.dart';
import '../data/claims_repository.dart';
import 'claims_providers.dart';

const _categorias = ['Solicitud', 'Reclamacion', 'Operacion'];

class CreateClaimPage extends ConsumerStatefulWidget {
  const CreateClaimPage({super.key});

  @override
  ConsumerState<CreateClaimPage> createState() => _CreateClaimPageState();
}

class _CreateClaimPageState extends ConsumerState<CreateClaimPage> {
  final _formKey = GlobalKey<FormState>();
  final _tituloController = TextEditingController();
  final _descripcionController = TextEditingController();

  String _categoria = _categorias.first;
  int? _ubicacionId;
  bool _submitting = false;

  @override
  void dispose() {
    _tituloController.dispose();
    _descripcionController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;

    if (_ubicacionId == null) {
      _toast('Selecciona una ubicación.', AppColors.destructive);
      return;
    }

    final user = ref.read(authControllerProvider).user;
    if (user == null) return;

    setState(() => _submitting = true);
    try {
      await ref
          .read(claimsRepositoryProvider)
          .create(
            ciudadanoId: user.id,
            ubicacionId: _ubicacionId!,
            categoria: _categoria,
            titulo: _tituloController.text.trim(),
            descripcion: _descripcionController.text.trim(),
          );

      ref.invalidate(claimsProvider);
      if (!mounted) return;
      _toast('Reclamación registrada.', AppColors.secondary);
      context.pop();
    } on ApiException catch (error) {
      _toast(error.message, AppColors.destructive);
    } catch (_) {
      _toast('No se pudo registrar la reclamación.', AppColors.destructive);
    } finally {
      if (mounted) setState(() => _submitting = false);
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
    final locationsAsync = ref.watch(locationsProvider);

    return Scaffold(
      appBar: AppBar(title: const Text('Nueva reclamación')),
      body: SafeArea(
        child: Form(
          key: _formKey,
          child: ListView(
            padding: const EdgeInsets.all(16),
            children: [
              Text('Categoría', style: Theme.of(context).textTheme.labelLarge),
              const SizedBox(height: 6),
              DropdownButtonFormField<String>(
                initialValue: _categoria,
                items: _categorias
                    .map((c) => DropdownMenuItem(value: c, child: Text(c)))
                    .toList(),
                onChanged: (value) =>
                    setState(() => _categoria = value ?? _categoria),
              ),
              const SizedBox(height: 16),
              _locationField(locationsAsync),
              const SizedBox(height: 16),
              AppTextField(
                label: 'Título',
                controller: _tituloController,
                prefixIcon: Icons.title_outlined,
                validator: (v) => Validators.required(v, field: 'El título'),
              ),
              const SizedBox(height: 16),
              AppTextField(
                label: 'Descripción',
                controller: _descripcionController,
                maxLines: 4,
                validator: (v) =>
                    Validators.required(v, field: 'La descripción'),
              ),
              const SizedBox(height: 24),
              PrimaryButton(
                label: 'Enviar reclamación',
                loading: _submitting,
                onPressed: _submit,
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _locationField(AsyncValue<List<LocationItem>> async) {
    return async.when(
      loading: () => const LinearProgressIndicator(),
      error: (error, _) => ErrorView(
        message: error is ApiException
            ? error.message
            : 'No se pudieron cargar las ubicaciones.',
        onRetry: () => ref.invalidate(locationsProvider),
      ),
      data: (locations) {
        if (locations.isEmpty) {
          return const EmptyState(
            title: 'Sin ubicaciones',
            message:
                'No hay ubicaciones registradas para asociar a la reclamación.',
            icon: Icons.place_outlined,
          );
        }

        return Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('Ubicación', style: Theme.of(context).textTheme.labelLarge),
            const SizedBox(height: 6),
            DropdownButtonFormField<int>(
              initialValue: _ubicacionId,
              isExpanded: true,
              hint: const Text('Selecciona la ubicación'),
              items: locations
                  .map(
                    (l) => DropdownMenuItem(
                      value: l.id,
                      child: Text(l.direccion, overflow: TextOverflow.ellipsis),
                    ),
                  )
                  .toList(),
              onChanged: (value) => setState(() => _ubicacionId = value),
            ),
          ],
        );
      },
    );
  }
}
