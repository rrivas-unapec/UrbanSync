import 'dart:convert';

import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

import '../../features/auth/domain/app_user.dart';

final tokenStorageProvider = Provider<TokenStorage>(
  (ref) => const TokenStorage(),
);

class StoredAuthSession {
  const StoredAuthSession({
    required this.token,
    required this.expiresAtUtc,
    required this.user,
  });

  final String token;
  final DateTime expiresAtUtc;
  final AppUser user;

  bool get isExpired {
    return !DateTime.now().toUtc().isBefore(expiresAtUtc);
  }
}

class TokenStorage {
  const TokenStorage();

  static const String _tokenKey =
      'urbansync_access_token';

  static const String _expirationKey =
      'urbansync_token_expiration';

  static const String _userKey =
      'urbansync_authenticated_user';

  static const FlutterSecureStorage _storage =
      FlutterSecureStorage();

  Future<void> saveSession({
    required String token,
    required DateTime expiresAtUtc,
    required AppUser user,
  }) async {
    final normalizedToken = token.trim();

    if (normalizedToken.isEmpty) {
      throw ArgumentError.value(
        token,
        'token',
        'El token no puede estar vacío.',
      );
    }

    await Future.wait([
      _storage.write(
        key: _tokenKey,
        value: normalizedToken,
      ),
      _storage.write(
        key: _expirationKey,
        value: expiresAtUtc.toUtc().toIso8601String(),
      ),
      _storage.write(
        key: _userKey,
        value: jsonEncode(user.toJson()),
      ),
    ]);
  }

  Future<String?> readToken() async {
    final token = await _storage.read(
      key: _tokenKey,
    );

    if (token == null || token.trim().isEmpty) {
      return null;
    }

    return token;
  }

  Future<StoredAuthSession?> readSession() async {
    final values = await Future.wait([
      _storage.read(key: _tokenKey),
      _storage.read(key: _expirationKey),
      _storage.read(key: _userKey),
    ]);

    final token = values[0];
    final expirationText = values[1];
    final userText = values[2];

    if (token == null ||
        token.trim().isEmpty ||
        expirationText == null ||
        expirationText.trim().isEmpty ||
        userText == null ||
        userText.trim().isEmpty) {
      await clear();

      return null;
    }

    final expiresAtUtc = DateTime.tryParse(
      expirationText,
    );

    if (expiresAtUtc == null) {
      await clear();

      return null;
    }

    try {
      final decodedUser = jsonDecode(userText);

      if (decodedUser is! Map<String, dynamic>) {
        await clear();

        return null;
      }

      final session = StoredAuthSession(
        token: token.trim(),
        expiresAtUtc: expiresAtUtc.toUtc(),
        user: AppUser.fromJson(decodedUser),
      );

      if (session.isExpired) {
        await clear();

        return null;
      }

      return session;
    } on FormatException {
      await clear();

      return null;
    } on TypeError {
      await clear();

      return null;
    }
  }

  Future<void> clear() async {
    await Future.wait([
      _storage.delete(key: _tokenKey),
      _storage.delete(key: _expirationKey),
      _storage.delete(key: _userKey),
    ]);
  }
}