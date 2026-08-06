import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/storage/token_storage.dart';
import '../data/auth_repository.dart';
import '../domain/app_user.dart';

enum AuthStatus {
  unknown,
  authenticated,
  unauthenticated,
}

class AuthState {
  const AuthState(
    this.status, {
    this.user,
    this.sessionExpired = false,
  });

  const AuthState.unknown()
      : this(AuthStatus.unknown);

  final AuthStatus status;
  final AppUser? user;
  final bool sessionExpired;
}

final authControllerProvider =
    NotifierProvider<AuthController, AuthState>(
  AuthController.new,
);

class AuthController extends Notifier<AuthState> {
  @override
  AuthState build() {
    Future.microtask(_restoreSession);

    return const AuthState.unknown();
  }

  Future<void> _restoreSession() async {
    final storage = ref.read(tokenStorageProvider);
    final session = await storage.readSession();

    if (session == null) {
      state = const AuthState(
        AuthStatus.unauthenticated,
      );
      return;
    }

    if (session.isExpired) {
      await storage.clear();

      state = const AuthState(
        AuthStatus.unauthenticated,
        sessionExpired: true,
      );
      return;
    }

    state = AuthState(
      AuthStatus.authenticated,
      user: session.user,
    );
  }

  Future<void> login(
    String email,
    String password,
  ) async {
    final result = await ref
        .read(authRepositoryProvider)
        .login(email, password);

    await ref.read(tokenStorageProvider).saveSession(
          token: result.token,
          expiresAtUtc: result.expiresAtUtc,
          user: result.user,
        );

    state = AuthState(
      AuthStatus.authenticated,
      user: result.user,
    );
  }

  Future<void> register({
    required String fullName,
    required String email,
    required String password,
  }) {
    return ref.read(authRepositoryProvider).register(
          fullName: fullName,
          email: email,
          password: password,
        );
  }

  Future<void> changePassword({
    required String currentPassword,
    required String newPassword,
    required String confirmNewPassword,
  }) {
    return ref.read(authRepositoryProvider).changePassword(
          currentPassword: currentPassword,
          newPassword: newPassword,
          confirmNewPassword: confirmNewPassword,
        );
  }

  Future<void> logout() async {
    await ref.read(tokenStorageProvider).clear();

    state = const AuthState(
      AuthStatus.unauthenticated,
    );
  }

  Future<void> markSessionExpired() async {
    await ref.read(tokenStorageProvider).clear();

    state = const AuthState(
      AuthStatus.unauthenticated,
      sessionExpired: true,
    );
  }
}