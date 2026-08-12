import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../app/theme.dart';
import '../../../core/network/api_exception.dart';
import '../../../shared/widgets/app_card.dart';
import '../../../shared/widgets/state_views.dart';
import '../domain/catalogs.dart';
import 'catalogs_providers.dart';

class CatalogsPage extends StatelessWidget {
  const CatalogsPage({super.key});

  @override
  Widget build(BuildContext context) {
    return DefaultTabController(
      length: 4,
      child: Scaffold(
        appBar: AppBar(
          title: const Text('Catálogos'),
          bottom: const TabBar(
            isScrollable: true,
            tabs: [
              Tab(text: 'Jurisdicciones'),
              Tab(text: 'Departamentos'),
              Tab(text: 'Instituciones'),
              Tab(text: 'Ubicaciones'),
            ],
          ),
        ),
        body: const TabBarView(
          children: [
            _JurisdictionsTab(),
            _DepartmentsTab(),
            _InstitutionsTab(),
            _LocationsTab(),
          ],
        ),
      ),
    );
  }
}

class _CatalogList<T> extends ConsumerWidget {
  const _CatalogList({
    required this.provider,
    required this.emptyTitle,
    required this.errorMessage,
    required this.itemBuilder,
  });

  final FutureProvider<List<T>> provider;
  final String emptyTitle;
  final String errorMessage;
  final Widget Function(T) itemBuilder;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final async = ref.watch(provider);

    return RefreshIndicator(
      onRefresh: () async => ref.invalidate(provider),
      child: async.when(
        loading: () => const LoadingView(),
        error: (error, _) => ListView(
          children: [
            const SizedBox(height: 100),
            ErrorView(
              message: error is ApiException ? error.message : errorMessage,
              onRetry: () => ref.invalidate(provider),
            ),
          ],
        ),
        data: (items) => items.isEmpty
            ? ListView(
                children: [
                  const SizedBox(height: 100),
                  EmptyState(title: emptyTitle, icon: Icons.inbox_outlined),
                ],
              )
            : ListView.builder(
                padding: const EdgeInsets.all(16),
                itemCount: items.length,
                itemBuilder: (context, index) => Padding(
                  padding: const EdgeInsets.only(bottom: 12),
                  child: itemBuilder(items[index]),
                ),
              ),
      ),
    );
  }
}

Widget _tile(String titulo, String subtitulo, {bool activo = true}) {
  return AppCard(
    child: Row(
      children: [
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(titulo, style: const TextStyle(fontWeight: FontWeight.w600)),
              if (subtitulo.isNotEmpty)
                Padding(
                  padding: const EdgeInsets.only(top: 2),
                  child: Text(
                    subtitulo,
                    style: const TextStyle(
                      fontSize: 12,
                      color: AppColors.mutedForeground,
                    ),
                  ),
                ),
            ],
          ),
        ),
        if (!activo)
          const Icon(
            Icons.cancel_outlined,
            size: 18,
            color: AppColors.mutedForeground,
          ),
      ],
    ),
  );
}

class _JurisdictionsTab extends StatelessWidget {
  const _JurisdictionsTab();

  @override
  Widget build(BuildContext context) {
    return _CatalogList<JurisdictionItem>(
      provider: jurisdictionsCatalogProvider,
      emptyTitle: 'Sin jurisdicciones',
      errorMessage: 'No se pudieron cargar las jurisdicciones.',
      itemBuilder: (item) => _tile(
        item.nombre,
        [
          item.nivel,
          if (item.jurisdiccionPadreNombre != null)
            'Depende de ${item.jurisdiccionPadreNombre}',
        ].where((s) => s.isNotEmpty).join(' · '),
        activo: item.activo,
      ),
    );
  }
}

class _DepartmentsTab extends StatelessWidget {
  const _DepartmentsTab();

  @override
  Widget build(BuildContext context) {
    return _CatalogList<Department>(
      provider: departmentsProvider,
      emptyTitle: 'Sin departamentos',
      errorMessage: 'No se pudieron cargar los departamentos.',
      itemBuilder: (item) => _tile(
        item.nombre,
        item.jurisdiccionNombre ?? '',
        activo: item.activo,
      ),
    );
  }
}

class _InstitutionsTab extends StatelessWidget {
  const _InstitutionsTab();

  @override
  Widget build(BuildContext context) {
    return _CatalogList<InstitutionItem>(
      provider: institutionsProvider,
      emptyTitle: 'Sin instituciones',
      errorMessage: 'No se pudieron cargar las instituciones.',
      itemBuilder: (item) => _tile(
        item.nombre,
        [
          item.tipoInstitucion,
          if (item.contactoEmail != null) item.contactoEmail!,
        ].where((s) => s.isNotEmpty).join(' · '),
        activo: item.activo,
      ),
    );
  }
}

class _LocationsTab extends StatelessWidget {
  const _LocationsTab();

  @override
  Widget build(BuildContext context) {
    return _CatalogList<LocationItem>(
      provider: locationsProvider,
      emptyTitle: 'Sin ubicaciones',
      errorMessage: 'No se pudieron cargar las ubicaciones.',
      itemBuilder: (item) => _tile(
        item.direccion,
        [
          item.jurisdiccionNombre,
          if (item.referencia != null) item.referencia!,
        ].where((s) => s.isNotEmpty).join(' · '),
      ),
    );
  }
}
